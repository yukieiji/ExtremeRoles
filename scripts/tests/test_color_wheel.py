"""カラーホイール座標のテスト。"""

import math
import sys
import tkinter as tk

import pytest

from lib.color_wheel import (
    LABEL_TEXT_HALF,
    MARGIN,
    ColorWheel,
    color_at,
    label_points,
    wheel_point,
)
from lib.color_space import Rgb, from_hsv


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


def test_color_labels_fit_inside_the_canvas():
    """色名ラベルがキャンバス幅に収まることのテスト。"""
    # Arrange
    radius = 138
    limit = radius + MARGIN - LABEL_TEXT_HALF

    # Act
    positions = label_points(radius)

    # Assert
    assert len(positions) == 6
    assert all(abs(x) <= limit and abs(y) <= limit for _, x, y in positions)


def test_color_at_returns_the_wheel_color_of_the_point():
    """ホイール座標から色が戻ることを右クリック相当の位置で検証するテスト。"""
    # Arrange
    center = 200
    radius = 100

    # Act
    hub = color_at(center, center, center, radius)
    right = color_at(center + radius, center, center, radius)
    up = color_at(center, center - radius, center, radius)
    half_right = color_at(center + radius // 2, center, center, radius)

    # Assert
    assert hub == Rgb(255, 255, 255)
    assert right == Rgb(255, 0, 0)
    assert up == Rgb(128, 255, 0)
    assert half_right == Rgb(255, 128, 128)


@pytest.mark.parametrize("hue_degrees", [0.0, 60.0, 120.0, 190.0, 240.0, 300.0])
def test_color_at_inverts_wheel_point(hue_degrees: float):
    """wheel_point の位置から拾った色が元の色相に戻ることのテスト。"""
    # Arrange
    center = 200
    radius = 100
    x, y = wheel_point(hue_degrees, 1.0)
    expected = from_hsv(hue_degrees, 1.0, 1.0)

    # Act
    picked = color_at(center + x * radius, center + y * radius, center, radius)

    # Assert
    assert picked is not None
    assert abs(picked.r - expected.r) <= 1
    assert abs(picked.g - expected.g) <= 1
    assert abs(picked.b - expected.b) <= 1


def test_color_at_returns_none_outside_the_wheel():
    """円外座標は None になることのテスト。"""
    # Arrange
    center = 200
    radius = 100

    # Act
    outside = color_at(center + radius + 1, center, center, radius)

    # Assert
    assert outside is None


@pytest.mark.skipif(
    sys.platform != "win32", reason="tkinter のウィンドウ生成が必要なため"
)
def test_show_target_marks_the_input_color(tk_root: tk.Misc):
    """判定中の色がホイール上の該当位置に示されることのテスト。"""
    # Arrange
    radius = 100
    wheel = ColorWheel(tk_root, radius, lambda _: None)

    # Act
    wheel.show_target(Rgb(255, 0, 0))
    marked = wheel.find_withtag("target")
    box = wheel.bbox(marked[0])

    # Assert
    assert len(marked) == 3
    assert (box[0] + box[2]) / 2 == pytest.approx(MARGIN + radius + radius, abs=0.5)
    assert (box[1] + box[3]) / 2 == pytest.approx(MARGIN + radius, abs=0.5)


@pytest.mark.skipif(
    sys.platform != "win32", reason="tkinter のウィンドウ生成が必要なため"
)
def test_show_target_replaces_the_previous_mark(tk_root: tk.Misc):
    """別の色を判定すると古い目印が消えて動き直すことのテスト。"""
    # Arrange
    wheel = ColorWheel(tk_root, 100, lambda _: None)
    wheel.show_target(Rgb(255, 0, 0))
    placed = wheel.bbox(wheel.find_withtag("target")[0])

    # Act
    wheel.show_target(Rgb(0, 0, 255))
    marked = wheel.find_withtag("target")
    moved = wheel.bbox(marked[0])

    # Assert
    assert len(marked) == 3
    assert moved != placed


@pytest.mark.skipif(
    sys.platform != "win32", reason="tkinter のウィンドウ生成が必要なため"
)
def test_show_target_clears_when_color_is_none(tk_root: tk.Misc):
    """入力色なしではホイールの目印が消えることのテスト。"""
    # Arrange
    wheel = ColorWheel(tk_root, 100, lambda _: None)
    wheel.show_target(Rgb(0, 255, 0))

    # Act
    wheel.show_target(None)

    # Assert
    assert wheel.find_withtag("target") == ()
