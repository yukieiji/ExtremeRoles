"""cell_layout.Box のテスト。"""

from cell_layout import Box


def test_box_size_and_contains():
    """矩形の寸法と内包判定が半開区間であることのテスト。"""
    # Arrange
    box = Box(left=10, top=20, right=30, bottom=40)

    # Act / Assert
    assert box.width == 20
    assert box.height == 20
    assert box.contains(10, 20)
    assert box.contains(29, 39)
    assert not box.contains(30, 30)
    assert not box.contains(9, 30)
