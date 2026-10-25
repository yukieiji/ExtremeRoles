"""カラーパレット一覧 (PaletteListBox) のテスト。"""

import sys
import tkinter as tk
from collections.abc import Iterator

import pytest

from lib.color_matrix import ColorMatrix
from lib.color_palette_list import ROW_HEIGHT, PaletteListBox
from lib.color_space import Rgb
from lib.palette_colors import PaletteColor

pytestmark = pytest.mark.skipif(
    sys.platform != "win32", reason="tkinter のウィンドウ生成が必要なため"
)

FIXTURE_PALETTE = (
    PaletteColor(name="PureRed", rgb=Rgb(255, 0, 0), line=1, index=1),
    PaletteColor(name="SkyBlue", rgb=Rgb(105, 165, 255), line=2, index=2),
    PaletteColor(name="MiddleGray", rgb=Rgb(128, 128, 128), line=3, index=3),
)


@pytest.fixture()
def box(tk_root: object) -> Iterator[PaletteListBox]:
    """非表示ウィンドウに配置した一覧ウィジェット。"""
    widget = PaletteListBox(tk_root, ColorMatrix(FIXTURE_PALETTE), lambda color: None)
    widget.update_idletasks()
    yield widget


def _input_row(box: PaletteListBox) -> tk.Canvas:
    """一覧の先頭にある入力色行のキャンバスを返します。"""
    return box.winfo_children()[0]


def _text_items(canvas: tk.Canvas) -> list[str]:
    """キャンバスに描画されたテキスト項目の中身を返します。"""
    return [
        canvas.itemcget(item, "text")
        for item in canvas.find_all()
        if canvas.type(item) == "text"
    ]


def test_set_input_color_draws_label_and_position(box: PaletteListBox):
    """入力色行にラベルと理想位置が描画されることのテスト。"""
    # Arrange
    matrix = ColorMatrix(FIXTURE_PALETTE)
    rgb = Rgb(105, 165, 255)
    row = _input_row(box)

    # Act
    box.set_input_color(rgb)
    texts = _text_items(row)

    # Assert
    assert any(text.startswith("入力色 #69A5FF") for text in texts)
    assert matrix.ideal_summary(rgb) in texts


def test_set_input_color_clears_when_unparsable(box: PaletteListBox):
    """解釈失敗の入力色では行が空になることのテスト。"""
    # Arrange
    row = _input_row(box)
    box.set_input_color(Rgb(1, 2, 3))

    # Act
    box.set_input_color(None)
    drawn = row.find_all()

    # Assert
    assert drawn == ()


def test_color_at_returns_the_palette_color_of_the_row(box: PaletteListBox):
    """一覧の行座標から対応するパレット項目が返ることのテスト。"""
    # Arrange
    middle_of_second_row = ROW_HEIGHT + ROW_HEIGHT // 2

    # Act
    color = box._color_at(middle_of_second_row)

    # Assert
    assert color is FIXTURE_PALETTE[1]
