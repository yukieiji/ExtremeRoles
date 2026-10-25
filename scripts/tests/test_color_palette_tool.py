"""color_palette_tool.py のテスト。"""

import pytest

from color_matrix import ColorMatrix
from color_space import Rgb
from color_palette_tool import format_matrix, format_report, main
from palette_colors import PaletteColor
from pytest import CaptureFixture

FIXTURE_PALETTE = (
    PaletteColor(name="PureRed", rgb=Rgb(255, 0, 0), line=1, index=1),
    PaletteColor(name="SkyBlue", rgb=Rgb(105, 165, 255), line=2, index=2),
    PaletteColor(name="MiddleGray", rgb=Rgb(128, 128, 128), line=3, index=3),
)


@pytest.fixture()
def matrix() -> ColorMatrix:
    """テスト用の小さなマトリクス。"""
    return ColorMatrix(FIXTURE_PALETTE)


def test_format_report_shows_exact_verdict(matrix: ColorMatrix):
    """完全一致の色は判定・位置・同一RGB項目まで表示されることのテスト。"""
    # Act
    report = format_report("#FF0000", matrix, 25.0)

    # Assert
    assert report is not None
    assert "#FF0000: 完全一致 -> PureRed #FF0000" in report
    assert "入力色の位置:" in report
    assert "同一RGBのパレット項目: PureRed" in report


def test_format_report_shows_near_verdict(matrix: ColorMatrix):
    """近似色は閾値内の候補件数とともに表示されることのテスト。"""
    # Act
    report = format_report("#79A5FA", matrix, 10.0)

    # Assert
    assert report is not None
    assert "近似色 -> SkyBlue" in report
    assert "近似候補 1件" in report


def test_format_report_rejects_invalid_color(matrix: ColorMatrix):
    """解釈できない色は None が返ることをテスト。"""
    # Act
    report = format_report("notacolor", matrix, 25.0)

    # Assert
    assert report is None


def test_format_matrix_lists_palette_by_cell(matrix: ColorMatrix):
    """配置表に明度見出しと各セルのパレット項目が並ぶことのテスト。"""
    # Act
    table = format_matrix(matrix)

    # Assert
    assert "--- 明度 L5" in table
    assert "PureRed #FF0000" in table
    assert "MiddleGray #808080" in table
    assert "無彩色" in table


def test_main_reports_verdict_and_returns_zero(capsys: CaptureFixture[str]):
    """CLI は実パレットで判定表示し 0 を返すことのテスト。"""
    # Act
    code = main(["UmbrerRed"])
    captured = capsys.readouterr()

    # Assert
    assert code == 0
    assert "完全一致 -> UmbrerRed #C80015" in captured.out


def test_main_returns_one_on_invalid_color(capsys: CaptureFixture[str]):
    """解釈できない色を渡すと 1 を返しエラーを stderr へ出すことのテスト。"""
    # Act
    code = main(["notacolor"])
    captured = capsys.readouterr()

    # Assert
    assert code == 1
    assert captured.out == ""
    assert "色を解釈できません" in captured.err
