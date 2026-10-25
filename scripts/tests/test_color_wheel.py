"""カラーホイール座標のテスト。"""

import math

from color_wheel import wheel_point


def test_wheel_point_places_hues_like_the_reference_wheel():
    """ホイール上での主色位置が基準画像と同じ方向のことのテスト。"""
    # Act
    red = wheel_point(0.0, 1.0)
    green = wheel_point(120.0, 1.0)
    blue = wheel_point(240.0, 1.0)

    # Assert
    assert red[0] > 0 and abs(red[1]) < 1e-9
    assert green[0] < 0 and green[1] < 0
    assert blue[0] < 0 and blue[1] > 0


def test_wheel_point_radius_follows_saturation():
    """彩度が半径になることのテスト。"""
    # Act
    center = math.hypot(*wheel_point(0.0, 0.0))
    outer = math.hypot(*wheel_point(0.0, 1.0))

    # Assert
    assert center == 0.0
    assert math.isclose(outer, 1.0)
