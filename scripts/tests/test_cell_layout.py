"""cell_layout.Box のテスト。"""

from lib.cell_layout import Box


def test_box_size_and_contains():
    """矩形の寸法と内包判定が半開区間であることのテスト。"""
    # Arrange
    box = Box(left=10, top=20, right=30, bottom=40)

    # Act
    covers_left_top = box.contains(10, 20)
    covers_inside = box.contains(29, 39)
    covers_right_edge = box.contains(30, 30)
    covers_outside = box.contains(9, 30)

    # Assert
    assert box.width == 20
    assert box.height == 20
    assert covers_left_top is True
    assert covers_inside is True
    assert covers_right_edge is False
    assert covers_outside is False
