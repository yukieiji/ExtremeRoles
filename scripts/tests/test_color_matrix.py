"""color_matrix.py と color_space.py のテスト。"""

import pytest

from color_matrix import ColorMatrix, NEAR_DELTA_E, is_achromatic
from color_space import Rgb, delta_e76, parse_color, to_hex
from palette_colors import COLORPALETTE_CS, PaletteColor, load_palette

FIXTURE_PALETTE = (
    PaletteColor(name="PureRed", rgb=Rgb(255, 0, 0), line=1, index=1),
    PaletteColor(name="SkyBlue", rgb=Rgb(105, 165, 255), line=2, index=2),
    PaletteColor(name="MiddleGray", rgb=Rgb(128, 128, 128), line=3, index=3),
)


@pytest.fixture()
def matrix() -> ColorMatrix:
    """テスト用の小さなマトリクス。"""
    return ColorMatrix(FIXTURE_PALETTE)


@pytest.fixture()
def real_matrix() -> ColorMatrix:
    """実 ColorPalette.cs から構築したマトリクス。"""
    return ColorMatrix(load_palette(COLORPALETTE_CS))


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
    # Act & Assert
    with pytest.raises(ValueError):
        Rgb(256, 0, 0)


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


def test_cell_of_places_hue_lightness_and_achromatic(matrix: ColorMatrix):
    """色相・明度・無彩色の座標割り当てが正しいことのテスト。"""
    # Act
    red = matrix.cell_of(Rgb(255, 0, 0))
    gray = matrix.cell_of(Rgb(128, 128, 128))
    white = matrix.cell_of(Rgb(255, 255, 255))
    black = matrix.cell_of(Rgb(0, 0, 0))
    gray_is_achromatic = is_achromatic(Rgb(128, 128, 128))
    red_is_achromatic = is_achromatic(Rgb(255, 0, 0))

    # Assert
    assert red.col == 0
    assert 0 <= red.row < matrix.rows
    assert gray.col == matrix.columns - 1
    assert gray_is_achromatic is True
    assert red_is_achromatic is False
    assert white.row == 0
    assert black.row == matrix.rows - 1


def test_lightness_rows_are_ordered_by_brightness(real_matrix: ColorMatrix):
    """明るい色ほど上の行に置かれることのテスト。"""
    # Act
    white = real_matrix.cell_of(Rgb(255, 255, 255)).row
    middle = real_matrix.cell_of(Rgb(128, 128, 128)).row
    black = real_matrix.cell_of(Rgb(0, 0, 0)).row

    # Assert
    assert white < middle < black


def test_full_palette_is_indexed_by_its_assigned_cell(real_matrix: ColorMatrix):
    """全パレット項目が割り当て先セルから取り出せることのテスト。"""
    # Act
    found = [
        real_matrix.color_at(real_matrix.assigned_cell(color))
        for color in real_matrix.colors
    ]

    # Assert
    assert found == list(real_matrix.colors)


def test_matrix_has_one_color_per_cell(real_matrix: ColorMatrix):
    """1 セルに最大 1 色で、全項目が別のセルに配置されることのテスト。"""
    # Act
    cells = [real_matrix.assigned_cell(color) for color in real_matrix.colors]

    # Assert
    assert len(set(cells)) == len(real_matrix.colors)
    assert real_matrix.columns == 20
    assert real_matrix.rows == 5
    assert all(0 <= cell.col < 20 and 0 <= cell.row < 5 for cell in cells)


def test_assigned_cell_stays_next_to_preferred_cell(real_matrix: ColorMatrix):
    """繰り下げ先は本来の座標の色相隣接列以内に収まることのテスト。"""
    # Act
    drifts = [_circular_drift(real_matrix, color) for color in real_matrix.colors]

    # Assert
    assert max(drifts) <= 4


def _circular_drift(matrix: ColorMatrix, color: PaletteColor) -> int:
    """割り当て先のずれ幅を、色相列が循環する前提で返します。"""
    preferred = matrix.cell_of(color.rgb)
    assigned = matrix.assigned_cell(color)
    hue_columns = matrix.columns - 1
    difference = abs(assigned.col - preferred.col)
    if hue_columns in (assigned.col, preferred.col):
        return difference
    return abs(assigned.row - preferred.row) + min(difference, hue_columns - difference)


def test_analyze_reports_exact_match(matrix: ColorMatrix):
    """パレットに存在する色は完全一致と判定されることのテスト。"""
    # Act
    analysis = matrix.analyze(Rgb(105, 165, 255), NEAR_DELTA_E)

    # Assert
    assert analysis.is_exact is True
    assert analysis.verdict == "完全一致"
    assert analysis.nearest.color.name == "SkyBlue"
    assert analysis.near_matches[0].color.name == "SkyBlue"


def test_analyze_reports_near_and_far(matrix: ColorMatrix):
    """近似色・非近似色の切り分けが閾値どおり行われることのテスト。"""
    # Arrange
    target = Rgb(120, 175, 250)

    # Act
    near = matrix.analyze(target, 10.0)
    strict = matrix.analyze(target, 0.5)

    # Assert
    assert near.is_near is True
    assert near.verdict == "近似色"
    assert strict.verdict == "近似色ではない"
    assert strict.near_matches == ()


def test_color_matrix_requires_colors():
    """空パレットではマトリクスを構築できないことのテスト。"""
    # Act & Assert
    with pytest.raises(ValueError):
        ColorMatrix(())


def test_resolve_accepts_name_and_hex(matrix: ColorMatrix):
    """パレット名・hex 表記の両方から色が解決できることのテスト。"""
    # Act
    by_name = matrix.resolve("skyblue")
    by_hex = matrix.resolve("#FF8C00")
    unknown = matrix.resolve("unknown-color")

    # Assert
    assert by_name == Rgb(105, 165, 255)
    assert by_hex == Rgb(255, 140, 0)
    assert unknown is None


def test_names_with_rgb_lists_all_same_color_entries(real_matrix: ColorMatrix):
    """同一 RGB の全項目名が列挙されることのテスト。"""
    # Act
    names = real_matrix.names_with_rgb(Rgb(128, 128, 128))

    # Assert
    assert set(names) >= {"NeutralColor", "ImitaterColor"}


def test_position_text_contains_cell(matrix: ColorMatrix):
    """位置説明にマトリクス座標と列名が含まれることのテスト。"""
    # Arrange
    gray = Rgb(128, 128, 128)

    # Act
    text = matrix.position_text(gray)
    cell = matrix.cell_of(gray)

    # Assert
    assert str(cell) in text
    assert "無彩色" in text


def test_palette_position_and_cell_summary(matrix: ColorMatrix):
    """パレット上の位置（宣言順・行番号）とマトリクス要約が返る事のテスト。"""
    # Arrange
    sky_blue = matrix.colors[1]

    # Act
    palette_position = matrix.palette_position(sky_blue)
    summary = matrix.cell_summary(sky_blue)
    cell = matrix.assigned_cell(sky_blue)

    # Assert
    assert palette_position == "宣言順 2/3 (ColorPalette.cs:2)"
    assert summary.startswith(str(cell))
    assert matrix.column_name(cell) in summary
    assert summary.endswith(f"L{matrix.rows - cell.row}")


def test_rows_grow_when_palette_exceeds_grid():
    """100 色を超えると行を増やして全項目を 1 セル 1 色に収めることのテスト。"""
    # Arrange
    palette = tuple(
        PaletteColor(
            name=f"Color{index}",
            rgb=Rgb(index % 251, (index * 7) % 251, (index * 13) % 251),
            line=index,
            index=index,
        )
        for index in range(1, 122)
    )

    # Act
    matrix = ColorMatrix(palette)
    cells = [matrix.assigned_cell(color) for color in matrix.colors]

    # Assert
    assert matrix.rows == 7
    assert len(set(cells)) == len(palette)
