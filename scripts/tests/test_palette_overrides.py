"""palette_overrides.py のマトリクス専用足し引きのテスト。"""

from lib.color_space import Rgb
from lib.palette_colors import COLORPALETTE_CS, PaletteColor, load_palette
from lib.palette_overrides import apply_matrix_overrides


def _fixture() -> list[PaletteColor]:
    return [
        PaletteColor(name="DefaultColor", rgb=Rgb(255, 255, 255), line=1, index=1),
        PaletteColor(name="SheriffOrange", rgb=Rgb(255, 122, 0), line=2, index=2),
    ]


def test_apply_matrix_overrides_swaps_colors():
    """DefaultColor を除き ImpostorRed を先頭へ足すことのテスト。"""
    # Act
    names = [color.name for color in apply_matrix_overrides(_fixture())]

    # Assert
    assert names == ["ImpostorRed", "SheriffOrange"]


def test_apply_matrix_overrides_renumbers_indexes():
    """除外と追加のあと宣言順が 1 から詰まっていることのテスト。"""
    # Act
    indexes = [color.index for color in apply_matrix_overrides(_fixture())]

    # Assert
    assert indexes == [1, 2]


def test_apply_matrix_overrides_on_real_palette():
    """実パレットでも DefaultColor が消え ImpostorRed が先頭になる事のテスト。"""
    # Arrange
    loaded = load_palette(COLORPALETTE_CS)

    # Act
    result = apply_matrix_overrides(loaded)
    impostor_red = result[0]

    # Assert
    assert len(result) == len(loaded)
    assert [color.index for color in result] == list(range(1, len(result) + 1))
    assert all(color.name != "DefaultColor" for color in result)
    assert impostor_red.name == "ImpostorRed"
    assert impostor_red.rgb == Rgb(255, 25, 25)
    assert impostor_red.line == 0
