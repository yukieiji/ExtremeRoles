"""カラーパレットを明度×色相のマトリクスに配置し、近似色を判定します。

マトリクスは既定 20 列 × 5 行 (100 色を超えると 20 色ごとに 1 行増) です。
行は明度 L* の件数分位、列は HSV 色相 (最終列は無彩色) で決めます。
1 セルには 1 色だけを入れ、争われた場合は「同じ行の近い列」→「近い行の近い列」
の順で空きセルへ繰り下げます。
"""

from collections.abc import Sequence
from dataclasses import dataclass

from .color_space import (
    Rgb,
    chroma,
    delta_e76,
    hue,
    lightness,
    parse_color,
    to_rgb_text,
)
from .palette_colors import PaletteColor

HUE_COLUMNS = 19
ACHROMATIC_COLUMN = HUE_COLUMNS
COLUMNS = 20
PREFERRED_ROWS = 5
HUE_DEGREES_PER_COLUMN = 360.0 / HUE_COLUMNS
ACHROMATIC_CHROMA = 1.0
HUE_LABELS = (
    "赤",
    "橙",
    "黄",
    "黄緑",
    "緑",
    "青緑",
    "水色",
    "スカイブルー",
    "青",
    "青紫",
    "マゼンタ",
    "赤紫",
)
EXACT_DELTA_E = 1.0
NEAR_DELTA_E = 25.0
MAX_LISTED_CANDIDATES = 6


@dataclass(frozen=True)
class Cell:
    """マトリクス上の座標（row=0 が最も明るい行）。"""

    row: int
    col: int

    def __str__(self) -> str:
        return f"R{self.row + 1}C{self.col + 1}"


@dataclass(frozen=True)
class MatchResult:
    """入力色とパレット項目の比較結果。"""

    color: PaletteColor
    delta_e: float

    @property
    def is_exact(self) -> bool:
        """dE が EXACT_DELTA_E 以下なら完全一致とみなします。"""
        return self.delta_e <= EXACT_DELTA_E

    def __str__(self) -> str:
        return f"{self.color.name} {to_rgb_text(self.color.rgb)} dE={self.delta_e:.2f}"


@dataclass(frozen=True)
class Analysis:
    """入力色の判定結果。"""

    target: Rgb
    nearest: MatchResult
    near_matches: tuple[MatchResult, ...]
    near_threshold: float

    @property
    def is_exact(self) -> bool:
        return self.nearest.is_exact

    @property
    def is_near(self) -> bool:
        return self.nearest.delta_e <= self.near_threshold

    @property
    def verdict(self) -> str:
        if self.is_exact:
            return "完全一致"
        if self.is_near:
            return "近似色"
        return "近似色ではない"


class ColorMatrix:
    """カラーパレットの明度×色相マトリクス。1 セル 1 色で割り当てます。"""

    def __init__(self, colors: Sequence[PaletteColor]):
        self._colors = tuple(colors)
        if not self._colors:
            raise ValueError("カラーパレットが空のためマトリクスを構築できません。")

        self._cells: dict[Cell, PaletteColor] = {}
        self._assigned: dict[str, Cell] = {}
        self._rows = max(PREFERRED_ROWS, -(-len(self._colors) // COLUMNS))
        self._row_edges = _lightness_edges(self._colors, self._rows)
        self._place_colors()

    @property
    def colors(self) -> tuple[PaletteColor, ...]:
        """パレット全項目。"""
        return self._colors

    @property
    def rows(self) -> int:
        """マトリクスの行数（既定 5、100 色超では 20 列ごとに 1 行増）。"""
        return self._rows

    @property
    def columns(self) -> int:
        """マトリクスの列数（色相 19 列 + 無彩色 1 列）。"""
        return COLUMNS

    def cell_of(self, rgb: Rgb) -> Cell:
        """任意の RGB が属すべきセル（空き状況は考慮しない理想座標）を返します。"""
        return Cell(row=self._row_of(rgb), col=column_of(rgb))

    def assigned_cell(self, color: PaletteColor) -> Cell:
        """パレット項目が実際に割り当てられたセルを返します。"""
        return self._assigned[color.name]

    def color_at(self, cell: Cell) -> PaletteColor | None:
        """指定セルに割り当てられた 1 色を返します。"""
        return self._cells.get(cell)

    def column_name(self, cell: Cell) -> str:
        """列見出し用の色相名を返します。"""
        if cell.col == ACHROMATIC_COLUMN:
            return "無彩色"
        label = (cell.col * len(HUE_LABELS)) // HUE_COLUMNS
        return HUE_LABELS[label % len(HUE_LABELS)]

    def column_start_degree(self, cell: Cell) -> float:
        """列の開始色相角を返します。"""
        return cell.col * HUE_DEGREES_PER_COLUMN

    def column_range(self, cell: Cell) -> str:
        """列が担う色相角度範囲を返します。"""
        if cell.col == ACHROMATIC_COLUMN:
            return "無彩色"
        left = cell.col * HUE_DEGREES_PER_COLUMN
        right = (cell.col + 1) * HUE_DEGREES_PER_COLUMN
        return f"{left:.0f}〜{right:.0f}deg"

    def row_range(self, row: int) -> str:
        """行が担う明度 L* の範囲を返します。"""
        highest = 100.0 if row == 0 else self._row_edges[row - 1]
        lowest = 0.0 if row == self.rows - 1 else self._row_edges[row]
        return f"L*{lowest:.0f}〜{highest:.0f}"

    def cell_label(self, cell: Cell) -> str:
        """セルの意味を示すラベルを返します。"""
        if cell.col == ACHROMATIC_COLUMN:
            column = self.column_name(cell)
        else:
            column = f"{self.column_name(cell)} ({self.column_range(cell)})"
        return f"色相: {column} / 明度 band L{self.rows - cell.row} {self.row_range(cell.row)}"

    def position_text(self, rgb: Rgb) -> str:
        """入力色 (未登録含み) の理想位置説明文を返します。"""
        cell = self.cell_of(rgb)
        return f"{cell} ({self.cell_label(cell)})"

    def position_text_of(self, color: PaletteColor) -> str:
        """パレット項目の実配置位置の説明文を返します。"""
        cell = self.assigned_cell(color)
        return f"{cell} ({self.cell_label(cell)})"

    def names_with_rgb(self, rgb: Rgb) -> tuple[str, ...]:
        """指定 RGB と完全に一致するパレット項目名を返します。"""
        return tuple(color.name for color in self._colors if color.rgb == rgb)

    def palette_position(self, color: PaletteColor) -> str:
        """パレット上の位置を表す文字列を返します (line=0 はソース外定義)。"""
        location = f"ColorPalette.cs:{color.line}" if color.line > 0 else "定義元なし"
        return f"宣言順 {color.index}/{len(self._colors)} ({location})"

    def cell_summary(self, color: PaletteColor) -> str:
        """実配置のマトリクス座標・色相名・明度 band をまとめた短い文字列を返します。"""
        return self._summary(self.assigned_cell(color))

    def ideal_summary(self, rgb: Rgb) -> str:
        """任意 RGB の理想位置の短い表示を返します (未登録の入力色用)。"""
        return self._summary(self.cell_of(rgb))

    def _summary(self, cell: Cell) -> str:
        return f"{cell} {self.column_name(cell)} L{self.rows - cell.row}"

    def resolve(self, text: str) -> Rgb | None:
        """パレット名または hex / "r,g,b" 文字列を Rgb へ変換します。"""
        wanted = text.strip().lower()
        for color in self._colors:
            if color.name.lower() == wanted:
                return color.rgb
        return parse_color(text)

    def analyze(self, target: Rgb, near_threshold: float) -> Analysis:
        """入力色をパレット全体と比較し、最も近い項目と近似候補を返します。"""
        matches = sorted(
            (
                MatchResult(color=color, delta_e=delta_e76(target, color.rgb))
                for color in self._colors
            ),
            key=lambda match: (match.delta_e, match.color.name),
        )
        return Analysis(
            target=target,
            nearest=matches[0],
            near_matches=tuple(
                match for match in matches if match.delta_e <= near_threshold
            ),
            near_threshold=near_threshold,
        )

    def _row_of(self, rgb: Rgb) -> int:
        value = lightness(rgb)
        for index, edge in enumerate(self._row_edges):
            if value >= edge:
                return index
        return len(self._row_edges)

    def _place_colors(self) -> None:
        rows: dict[int, list[PaletteColor]] = {row: [] for row in range(self.rows)}
        for color in self._colors:
            rows[self.cell_of(color.rgb).row].append(color)

        for row, colors in rows.items():
            for color in sorted(colors, key=lambda item: column_of(item.rgb)):
                self._reserve(color, row)

    def _reserve(self, color: PaletteColor, row: int) -> None:
        """同じ行の近い列から順に空きを探して割り当てます。"""
        preferred = column_of(color.rgb)
        for candidate in _rows_by_distance(row, self.rows):
            for column in _columns_by_distance(preferred):
                cell = Cell(row=candidate, col=column)
                if cell in self._cells:
                    continue
                self._cells[cell] = color
                self._assigned[color.name] = cell
                return
        # rows = max(PREFERRED_ROWS, ceil(件数 / COLUMNS)) なので空きは必ず存在する。
        raise ValueError("マトリクスに空きセルがありません。")


def column_of(rgb: Rgb) -> int:
    """RGB の色相が決める列番号を返します。"""
    if is_achromatic(rgb):
        return ACHROMATIC_COLUMN
    return int(hue(rgb) / HUE_DEGREES_PER_COLUMN) % HUE_COLUMNS


def is_achromatic(rgb: Rgb) -> bool:
    """色相を持たない (R=G=B に実質等しい) かどうかを返します。"""
    return rgb.r == rgb.g == rgb.b or chroma(rgb) < ACHROMATIC_CHROMA


def _columns_by_distance(preferred: int) -> list[int]:
    """preferred に近い列順。色相列同士は一周し、無彩色列は別扱いにします。"""
    return sorted(range(COLUMNS), key=lambda col: _column_distance(col, preferred))


def _column_distance(col: int, preferred: int) -> int:
    if col == ACHROMATIC_COLUMN or preferred == ACHROMATIC_COLUMN:
        return abs(col - preferred)
    difference = abs(col - preferred)
    return min(difference, HUE_COLUMNS - difference)


def _rows_by_distance(preferred: int, rows: int) -> list[int]:
    """preferred 行に近い順の行番号。同じ行を最初に調べます。"""
    return sorted(range(rows), key=lambda row: abs(row - preferred))


def _lightness_edges(colors: Sequence[PaletteColor], rows: int) -> tuple[float, ...]:
    """明度 L* を件数で等分に割った境目を (高い順) に返します。"""
    values = sorted((lightness(color.rgb) for color in colors), reverse=True)
    return tuple(values[index * len(values) // rows] for index in range(1, rows))
