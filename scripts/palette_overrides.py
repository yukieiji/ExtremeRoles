"""ColorPalette.cs の読み取り結果に適用する、マトリクス専用の足し引きを定義します。

DefaultColor の代わりに先頭へ ImpostorRed を据えます。
"""

from collections.abc import Sequence
from dataclasses import replace

from color_space import Rgb
from palette_colors import PaletteColor

MATRIX_EXCLUDED_NAMES = ("DefaultColor",)
MATRIX_LEADING_COLORS = (
    PaletteColor(name="ImpostorRed", rgb=Rgb(255, 25, 25), line=0, index=0),
)


def apply_matrix_overrides(colors: Sequence[PaletteColor]) -> list[PaletteColor]:
    """除外対象を除き、追加色を先頭へ足して宣言順を振り直します。"""
    kept = [color for color in colors if color.name not in MATRIX_EXCLUDED_NAMES]
    combined = [*MATRIX_LEADING_COLORS, *kept]
    return [replace(color, index=number) for number, color in enumerate(combined, 1)]
