"""palette_colors.py のテスト。"""

from pathlib import Path

import pytest

from color_space import Rgb
from palette_colors import COLORPALETTE_CS, PaletteColor, load_palette, parse_source

SAMPLE_SOURCE = """
public static class ColorPalette
{
	public readonly static Color DefaultColor = Color.white;
	public readonly static Color SheriffOrange = new Color32(255, 140, 0, byte.MaxValue);
	public readonly static Color SkaterMizuiro = new Color32(
		172, 219, 218, byte.MaxValue);
	public static readonly Color HexColor = new Color32(0x10, 0x20, 0x30, byte.MaxValue);
	public readonly static Color SuffixColor = new Color(0.8f, 0.5f, 0f, 1f);
	public readonly static Color NeutralColor = Color.gray;
	public readonly static Color ImitaterColor = NeutralColor;
	// public readonly static Color CommentedColor = new Color32(1, 2, 3, byte.MaxValue);
}
"""


def parse_names(source: str) -> dict[str, PaletteColor]:
    """警告を無視して名前順に解決した結果を返します。"""
    colors, _ = parse_source(source)
    return {color.name: color for color in colors}


def test_parse_source_reads_color32_and_line():
    """Color32 宣言の RGB と行番号が読み取れることのテスト。"""
    # Act
    colors = parse_names(SAMPLE_SOURCE)

    # Assert
    assert colors["SheriffOrange"].rgb == Rgb(255, 140, 0)
    assert colors["SheriffOrange"].line == 5


@pytest.mark.parametrize(
    ("name", "expected"),
    [
        ("SkaterMizuiro", Rgb(172, 219, 218)),
        ("HexColor", Rgb(16, 32, 48)),
        ("SuffixColor", Rgb(204, 128, 0)),
        ("DefaultColor", Rgb(255, 255, 255)),
        ("NeutralColor", Rgb(128, 128, 128)),
        ("ImitaterColor", Rgb(128, 128, 128)),
    ],
)
def test_parse_source_supported_declarations(name: str, expected: Rgb):
    """折り返し・16進・f サフィックス・名前色・エイリアスを解析できることのテスト。"""
    # Act
    colors = parse_names(SAMPLE_SOURCE)

    # Assert
    assert colors[name].rgb == expected


def test_parse_source_reports_unsupported_as_warning():
    """解釈できない宣言は警告になり、結果には含まれないことのテスト。"""
    # Act
    colors, warnings = parse_source(
        "public readonly static Color X = default;\n"
        "public readonly static Color Y = new Color32(1, 2, 3);\n"
    )

    # Assert
    assert [color.name for color in colors] == ["Y"]
    assert warnings == ["X (line 1): 解決できないエイリアス参照 'default'"]


def test_parse_source_skips_commented_declaration():
    """コメント化された宣言はパレットに現れないことのテスト。"""
    # Act
    colors = parse_names(SAMPLE_SOURCE)

    # Assert
    assert "CommentedColor" not in colors


def test_parse_source_warns_out_of_range_channel():
    """Color の 0〜1 超えなど不正値の宣言は警告となることのテスト。"""
    # Act
    colors, warnings = parse_source(
        "public readonly static Color TooBright = new Color(1.5, 0, 0);"
    )

    # Assert
    assert colors == []
    assert warnings[0].startswith("TooBright (line 1):")
    assert "解釈できない宣言" in warnings[0]


def test_load_palette_matches_real_color_palette():
    """実ファイルの主要項目が期待どおりの値であることのテスト。"""
    # Act
    colors = {color.name: color for color in load_palette(Path(COLORPALETTE_CS))}

    # Assert
    assert colors["UmbrerRed"].rgb == Rgb(200, 0, 21)
    assert colors["LiberalColor"].rgb == Rgb(249, 240, 111)
    assert colors["NeutralColor"].rgb == Rgb(128, 128, 128)
    assert colors["ImitaterColor"].alias_of == "NeutralColor"
