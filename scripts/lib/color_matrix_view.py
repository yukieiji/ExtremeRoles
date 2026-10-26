"""20×5 のカラーマトリクスを描画し、1 セル 1 色を表示・選択できるようにします。"""

import tkinter as tk
from collections.abc import Callable
from tkinter import font as tkfont

from .cell_layout import Box
from .color_matrix import Analysis, Cell, ColorMatrix
from .color_space import to_hex, to_rgb_text
from .color_tooltip import CellTooltip

CELL_WIDTH = 66
CELL_HEIGHT = 104
CELL_GAP = 4
MARGIN_LEFT = 92
MARGIN_TOP = 26
PAD = 3
LABEL_HEIGHT = 30


class MatrixView(tk.Canvas):
    """明度 band × 色相列のマトリクス。1 セルに 1 色だけを描きます。"""

    def __init__(
        self, parent: tk.Misc, matrix: ColorMatrix, on_select: Callable[[str], None]
    ):
        width = MARGIN_LEFT + matrix.columns * (CELL_WIDTH + CELL_GAP) + 8
        height = MARGIN_TOP + matrix.rows * (CELL_HEIGHT + CELL_GAP) + 8
        super().__init__(
            parent, width=width, height=height, bg="#1e1e1e", highlightthickness=0
        )
        self._matrix = matrix
        self._on_select = on_select
        self._small = tkfont.Font(family="Yu Gothic UI", size=7)
        self._draw_grid()
        self._redraw_cells()
        self.bind("<Button-1>", self._handle_click)
        self._tooltip = CellTooltip(self, self._tooltip_lines)

    def show(self, analysis: Analysis | None) -> None:
        """判定結果に合わせたマーカーを描き直します。"""
        self.delete("mark")
        if analysis is None:
            return

        for match in analysis.near_matches:
            self._outline(self._matrix.assigned_cell(match.color), "#ffd54f", 2, (4, 2))
        self._outline(
            self._matrix.assigned_cell(analysis.nearest.color), "#ffffff", 3, ()
        )
        box = self._cell_box(self._matrix.cell_of(analysis.target))
        self.create_oval(
            box.left + 4,
            box.top + 4,
            box.left + 12,
            box.top + 12,
            fill="#ffffff",
            outline="#000000",
            tags=("mark",),
        )

    def _draw_grid(self) -> None:
        for col in range(self._matrix.columns):
            cell = Cell(0, col)
            label = self._matrix.column_name(cell)
            if col != self._matrix.columns - 1:
                label = f"{label} {self._matrix.column_start_degree(cell):.0f}deg"
            self.create_text(
                MARGIN_LEFT + col * (CELL_WIDTH + CELL_GAP) + CELL_WIDTH // 2,
                MARGIN_TOP - 12,
                text=label,
                fill="#dddddd",
                font=self._small,
            )
        for row in range(self._matrix.rows):
            self.create_text(
                MARGIN_LEFT - 8,
                MARGIN_TOP + row * (CELL_HEIGHT + CELL_GAP) + CELL_HEIGHT // 2,
                anchor=tk.E,
                text=f"L{self._matrix.rows - row}\n{self._matrix.row_range(row)}",
                fill="#dddddd",
                font=self._small,
            )

    def _redraw_cells(self) -> None:
        """全セルを描き直します。"""
        self.delete("cell")
        for row in range(self._matrix.rows):
            for col in range(self._matrix.columns):
                self._draw_cell(Cell(row, col))

    def _draw_cell(self, cell: Cell) -> None:
        box = self._cell_box(cell)
        self.create_rectangle(
            box.left,
            box.top,
            box.right,
            box.bottom,
            fill="#2a2a2a",
            outline="#3a3a3a",
            tags=("cell",),
        )
        color = self._matrix.color_at(cell)
        if color is None:
            return

        self.create_rectangle(
            box.left + PAD,
            box.top + PAD,
            box.right - PAD,
            box.bottom - PAD - LABEL_HEIGHT,
            fill=to_hex(color.rgb),
            outline="#555555",
            tags=("cell",),
        )
        self.create_text(
            box.left + CELL_WIDTH // 2,
            box.bottom - LABEL_HEIGHT + 6,
            text=_shorten(color.name),
            fill="#eeeeee",
            font=self._small,
            tags=("cell",),
        )
        self.create_text(
            box.left + CELL_WIDTH // 2,
            box.bottom - 8,
            text=to_rgb_text(color.rgb),
            fill="#9cdcfe",
            font=self._small,
            tags=("cell",),
        )

    @staticmethod
    def _cell_box(cell: Cell) -> Box:
        """セル全体の矩形を返します。"""
        left = MARGIN_LEFT + cell.col * (CELL_WIDTH + CELL_GAP)
        top = MARGIN_TOP + cell.row * (CELL_HEIGHT + CELL_GAP)
        return Box(
            left=left, top=top, right=left + CELL_WIDTH, bottom=top + CELL_HEIGHT
        )

    def _outline(
        self, cell: Cell, color: str, width: int, dash: tuple[int, ...]
    ) -> None:
        box = self._cell_box(cell)
        self.create_rectangle(
            box.left - 2,
            box.top - 2,
            box.right + 2,
            box.bottom + 2,
            outline=color,
            width=width,
            dash=dash,
            tags=("mark",),
        )

    def _tooltip_lines(self, x: int, y: int) -> list[str]:
        cell = self._cell_at(x, y)
        if cell is None:
            return []
        color = self._matrix.color_at(cell)
        if color is None:
            return [f"{self._matrix.cell_label(cell)} / 空きセル"]

        lines = [
            f"{color.index:>3} {color.name} {to_rgb_text(color.rgb)}",
            self._matrix.palette_position(color),
            self._matrix.cell_label(cell),
        ]
        preferred = self._matrix.cell_of(color.rgb)
        if preferred != cell:
            lines.append(f"元の座標 {preferred} は使用中の為ここへ配置")
        return lines

    def _cell_at(self, x: int, y: int) -> Cell | None:
        """指定座標のセルを返します。セル間の隙間なら None。"""
        col = (x - MARGIN_LEFT) // (CELL_WIDTH + CELL_GAP)
        row = (y - MARGIN_TOP) // (CELL_HEIGHT + CELL_GAP)
        if (
            col < 0
            or row < 0
            or col >= self._matrix.columns
            or row >= self._matrix.rows
        ):
            return None

        cell = Cell(row, col)
        if not self._cell_box(cell).contains(x, y):
            return None
        return cell

    def _handle_click(self, event: tk.Event) -> None:
        cell = self._cell_at(event.x, event.y)
        if cell is None:
            return
        color = self._matrix.color_at(cell)
        if color is not None:
            self._on_select(to_rgb_text(color.rgb))


def _shorten(name: str) -> str:
    return name if len(name) <= 11 else name[:10] + "…"
