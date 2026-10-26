"""ExtremeRoles カラーパレットのカラーマトリクス表示ツール。

使い方:
    uv run scripts/color_palette_tool.py                       # GUI を起動
    uv run scripts/color_palette_tool.py "#69A5FF" "#FFF"      # CLI で近似色判定
    uv run scripts/color_palette_tool.py --matrix              # 配置表を表示
    uv run scripts/color_palette_tool.py --near 10 SheriffOrange
    uv run scripts/color_palette_tool.py --debug "#69A5FF"     # GUI のスクショを保存して終了
    uv run scripts/color_palette_tool.py --debug --shot tmp.png
"""

import argparse
import sys
from collections.abc import Sequence
from pathlib import Path

from lib.color_matrix import MAX_LISTED_CANDIDATES, Cell, ColorMatrix, NEAR_DELTA_E
from lib.color_space import to_rgb_text
from lib.palette_colors import COLORPALETTE_CS, load_palette
from lib.palette_overrides import apply_matrix_overrides

DEBUG_SHOT = "color_palette_debug.png"
INITIAL_COLOR = "105, 165, 255"


def build_parser() -> argparse.ArgumentParser:
    """コマンドライン引数のパーサーを返します。"""
    parser = argparse.ArgumentParser(
        prog="color_palette_tool.py",
        description="ColorPalette のカラーマトリクス表示と近似色判定を行います。",
    )
    parser.add_argument(
        "colors", nargs="*", help="判定したい色 (#RRGGBB / r,g,b / パレット名)"
    )
    parser.add_argument(
        "--near", type=float, default=NEAR_DELTA_E, help="近似色とみなす dE 閾値"
    )
    parser.add_argument(
        "--matrix", action="store_true", help="マトリクス上の配置を表で表示"
    )
    parser.add_argument(
        "--debug",
        action="store_true",
        help="GUI を開いて少し後にウィンドウを PNG で保存して終了します",
    )
    parser.add_argument(
        "--shot",
        default=DEBUG_SHOT,
        help="--debug 時に保存する PNG のパス (--debug なしでは使われず、既定: %(default)s)",
    )
    return parser


def build_matrix() -> ColorMatrix:
    """ColorPalette.cs からマトリクスを構築します (マトリクス専用の足し引きを適用)。"""
    return ColorMatrix(apply_matrix_overrides(load_palette(COLORPALETTE_CS)))


def format_report(text: str, matrix: ColorMatrix, near_threshold: float) -> str | None:
    """1色分の判定結果テキストを返します。色を解釈できなかった場合は None。"""
    rgb = matrix.resolve(text)
    if rgb is None:
        return None

    analysis = matrix.analyze(rgb, near_threshold)
    lines = [
        f"{to_rgb_text(rgb)}: {analysis.verdict} -> {analysis.nearest}",
        f"  入力色の位置: {matrix.position_text(rgb)}",
        f"  最近傍の位置: {matrix.position_text_of(analysis.nearest.color)}",
        f"  パレット上  : {matrix.palette_position(analysis.nearest.color)}",
        f"  近似候補 {len(analysis.near_matches)}件 (dE <= {near_threshold:g}):",
    ]
    for match in analysis.near_matches[:MAX_LISTED_CANDIDATES]:
        lines.append(
            f"    #{match.color.index:>3} "
            f"{matrix.cell_summary(match.color):<18} {match}"
        )

    same = matrix.names_with_rgb(rgb)
    lines.append(f"  同一RGBのパレット項目: {', '.join(same) if same else 'なし'}")
    return "\n".join(lines)


def format_matrix(matrix: ColorMatrix) -> str:
    """マトリクス上の配置表テキストを返します。"""
    lines: list[str] = []
    for row in range(matrix.rows):
        lines.append(f"--- 明度 L{matrix.rows - row} " + "-" * 56)
        for col in range(matrix.columns):
            cell = Cell(row, col)
            color = matrix.color_at(cell)
            if color is None:
                continue
            names = f"{color.name} {to_rgb_text(color.rgb)}"
            lines.append(f"{matrix.cell_label(cell):<34} {names}")
    return "\n".join(lines)


def main(argv: list[str]) -> int:
    """ツール本体。終了コードを返します。"""
    arguments = build_parser().parse_args(argv)
    matrix = build_matrix()

    if arguments.debug:
        return launch_gui(
            matrix, arguments.near, arguments.colors, Path(arguments.shot)
        )

    if arguments.matrix:
        print(format_matrix(matrix))
        if not arguments.colors:
            return 0

    if not arguments.colors:
        return launch_gui(matrix, arguments.near, (), None)

    for text in arguments.colors:
        report = format_report(text, matrix, arguments.near)
        if report is None:
            print(f"{text}: 色を解釈できません", file=sys.stderr)
            return 1
        print(report)
    return 0


def launch_gui(
    matrix: ColorMatrix,
    near_threshold: float,
    colors: Sequence[str],
    debug_shot: Path | None,
) -> int:
    """GUI を起動します。色が指定されていない場合は見本色を判定しておきます。"""
    from lib.color_palette_gui import PaletteApp

    app = PaletteApp(matrix, near_threshold, debug_shot)
    for text in colors or (INITIAL_COLOR,):
        app.set_color(text)
    app.mainloop()
    return app.debug_status


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8", errors="backslashreplace")
    raise SystemExit(main(sys.argv[1:]))
