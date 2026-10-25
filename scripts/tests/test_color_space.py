"""color_space.py の色変換・色表記のテスト。"""

import pytest

from color_space import (
    Rgb,
    delta_e76,
    from_hsv,
    hue,
    parse_color,
    saturation,
    to_hex,
    value,
)


@pytest.mark.parametrize(
    ("text", "expected"),
    [
        ("#FF8C00", Rgb(255, 140, 0)),
        ("ff8c00", Rgb(255, 140, 0)),
        ("#F80", Rgb(255, 136, 0)),
        ("FF8C0080", Rgb(255, 140, 0)),
        ("255, 140, 0", Rgb(255, 140, 0)),
        ("(255 140 0)", Rgb(255, 140, 0)),
    ],
)
def test_parse_color_accepts_formats(text: str, expected: Rgb):
    """各色表記が Rgb に変換できることのテスト。"""
    # Act
    actual = parse_color(text)

    # Assert
    assert actual == expected


@pytest.mark.parametrize(
    "text", ["", "  ", "notacolor", "#12345", "300,0,0", "1,2,3,4"]
)
def test_parse_color_rejects_invalid(text: str):
    """不正な色表記は None になることのテスト。"""
    # Act
    actual = parse_color(text)

    # Assert
    assert actual is None


def test_rgb_rejects_out_of_range_channel():
    """Rgb は 0〜255 のチャンネル値だけを受け付けることのテスト。"""

    # Act
    def build_out_of_range() -> Rgb:
        return Rgb(256, 0, 0)

    # Assert
    with pytest.raises(ValueError):
        build_out_of_range()


def test_to_hex_and_delta_e76():
    """hex 表現と色差計算が正しいことのテスト。"""
    # Act
    hex_value = to_hex(Rgb(255, 140, 0))
    same = delta_e76(Rgb(10, 20, 30), Rgb(10, 20, 30))
    opposite = delta_e76(Rgb(0, 0, 0), Rgb(255, 255, 255))

    # Assert
    assert hex_value == "#FF8C00"
    assert same == 0.0
    assert opposite > 90.0


def test_saturation_matches_hsv_definition():
    """HSV 彩度が (max-min)/max で求まることのテスト。"""
    # Act
    pure_red = saturation(Rgb(255, 0, 0))
    white = saturation(Rgb(255, 255, 255))
    black = saturation(Rgb(0, 0, 0))
    mixed = saturation(Rgb(128, 128, 200))

    # Assert
    assert pure_red == 1.0
    assert white == 0.0
    assert black == 0.0
    assert mixed == pytest.approx(0.36)


def test_value_is_the_highest_channel():
    """HSV 明度 V が最大チャンネル / 255 で求まることのテスト。"""
    # Act
    black = value(Rgb(0, 10, 20))
    white = value(Rgb(255, 255, 255))
    mixed = value(Rgb(10, 200, 30))

    # Assert
    assert black == pytest.approx(20 / 255)
    assert white == 1.0
    assert mixed == pytest.approx(200 / 255)


def test_hue_of_primaries_and_achromatic():
    """主色と無彩色の色相角が HSV の定義どおりのことのテスト。"""
    # Act
    red = hue(Rgb(255, 0, 0))
    green = hue(Rgb(0, 255, 0))
    blue = hue(Rgb(0, 0, 255))
    gray = hue(Rgb(120, 120, 120))

    # Assert
    assert red == pytest.approx(0.0)
    assert green == pytest.approx(120.0)
    assert blue == pytest.approx(240.0)
    assert gray == 0.0


def test_from_hsv_primary_colors():
    """主色と無彩色の HSV → RGB 変換が正しいことのテスト。"""
    # Act
    red = from_hsv(0.0, 1.0, 1.0)
    green = from_hsv(120.0, 1.0, 1.0)
    blue = from_hsv(240.0, 1.0, 1.0)
    white = from_hsv(0.0, 0.0, 1.0)
    black = from_hsv(0.0, 1.0, 0.0)
    gray = from_hsv(180.0, 0.0, 0.5)

    # Assert
    assert red == Rgb(255, 0, 0)
    assert green == Rgb(0, 255, 0)
    assert blue == Rgb(0, 0, 255)
    assert white == Rgb(255, 255, 255)
    assert black == Rgb(0, 0, 0)
    assert gray == Rgb(128, 128, 128)


def test_from_hsv_wraps_angles():
    """360 度を超えても同じ色になることのテスト。"""
    # Act
    straight = to_hex(from_hsv(30.0, 1.0, 1.0))
    wrapped = to_hex(from_hsv(390.0, 1.0, 1.0))

    # Assert
    assert straight == wrapped


def test_from_hsv_roundtrip_keeps_hue_and_saturation():
    """from_hsv で作った色が HSV 変換でもとに戻る事のテスト。"""
    # Arrange
    expected = Rgb(255, 230, 0)

    # Act
    rebuilt = to_hex(from_hsv(hue(expected), saturation(expected), value(expected)))

    # Assert
    assert rebuilt == to_hex(expected)
