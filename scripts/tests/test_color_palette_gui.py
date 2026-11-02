"""color_palette_gui.py の判定表示ヘルパーと操作のテスト。"""

import sys
import tkinter as tk
from collections.abc import Iterator

import pytest
from pytest import MonkeyPatch

import lib.color_palette_gui as color_palette_gui
from conftest import create_tk_window
from lib.color_matrix import EXACT_DELTA_E, Analysis, ColorMatrix, MatchResult
from lib.color_palette_gui import (
    DEBUG_SIZE_RATIO,
    VERDICT_COLORS,
    PaletteApp,
    _verdict_color,
    _verdict_tag,
    color_sample_url,
    is_rendered,
    unparsable_text,
)
from lib.color_space import Rgb
from lib.palette_colors import PaletteColor

TARGET = Rgb(105, 165, 255)
NEAR_THRESHOLD = 25.0
FIXTURE_PALETTE = (
    PaletteColor(name="PureRed", rgb=Rgb(255, 0, 0), line=1, index=1),
    PaletteColor(name="SkyBlue", rgb=Rgb(105, 165, 255), line=2, index=2),
    PaletteColor(name="MiddleGray", rgb=Rgb(128, 128, 128), line=3, index=3),
)

WINDOWS_ONLY = pytest.mark.skipif(
    sys.platform != "win32", reason="tkinter のウィンドウ生成が必要なため"
)


def _analysis(delta_e: float) -> Analysis:
    """色差だけを決めて最小の判定結果を作ります。"""
    color = PaletteColor(name="Any", rgb=TARGET, line=1, index=1)
    nearest = MatchResult(color=color, delta_e=delta_e)
    matches = () if delta_e > NEAR_THRESHOLD else (nearest,)
    return Analysis(
        target=TARGET,
        nearest=nearest,
        near_matches=matches,
        near_threshold=NEAR_THRESHOLD,
    )


@pytest.mark.parametrize(
    ("delta_e", "expected_tag"),
    [
        (0.0, "ok"),
        (EXACT_DELTA_E, "ok"),
        (EXACT_DELTA_E + 0.1, "near"),
        (NEAR_THRESHOLD, "near"),
        (NEAR_THRESHOLD + 0.1, "far"),
    ],
)
def test_verdict_tag_and_color(delta_e: float, expected_tag: str):
    """色差に応じて判定タグと色が決まり、色は共通定数と一致ことのテスト。"""
    # Arrange
    analysis = _analysis(delta_e)

    # Act
    tag = _verdict_tag(analysis)
    foreground = _verdict_color(analysis)

    # Assert
    assert tag == expected_tag
    assert foreground == VERDICT_COLORS[expected_tag]


def test_verdict_colors_cover_every_tag():
    """VERDICT_COLORS が判定タグの全分岐を網羅していることのテスト。"""
    # Act
    tags = {_verdict_tag(_analysis(delta)) for delta in (0.0, 10.0, 40.0)}

    # Assert
    assert tags == {"ok", "near", "far"}
    assert set(VERDICT_COLORS) == tags


@pytest.mark.parametrize(
    ("width", "height", "expected_width", "expected_height", "expected"),
    [
        (2316, 1019, 2300, 980, True),
        (302, 68, 2300, 980, False),
        (2316, 1019, 1, 1, False),
        (2316, 1019, 1, 980, False),
        (2000, 1019, 2300, 980, False),
    ],
)
def test_is_rendered(
    width: int,
    height: int,
    expected_width: int,
    expected_height: int,
    expected: bool,
):
    """撮影サイズが期待サイズに達したかの判定が正しいことのテスト。"""
    # Act
    actual = is_rendered(width, height, expected_width, expected_height)

    # Assert
    assert actual is expected


def test_is_rendered_uses_the_shared_ratio():
    """閾値境目が DEBUG_SIZE_RATIO と一致ことのテスト。"""
    # Arrange
    expected_width = 1000
    just_enough = int(expected_width * DEBUG_SIZE_RATIO)

    # Act
    accepted = is_rendered(just_enough, 900, expected_width, 900)
    rejected = is_rendered(just_enough - 1, 900, expected_width, 900)

    # Assert
    assert accepted is True
    assert rejected is False


@pytest.mark.parametrize(
    ("color", "expected"),
    [
        (Rgb(105, 165, 255), "https://www.color-sample.com/colors/69A5FF/"),
        (Rgb(255, 23, 25), "https://www.color-sample.com/colors/FF1719/"),
        (Rgb(0, 0, 0), "https://www.color-sample.com/colors/000000/"),
    ],
)
def test_color_sample_url(color: Rgb, expected: str):
    """色から color-sample.com の色詳細 URL が組み立つことのテスト。"""
    # Act
    url = color_sample_url(color)

    # Assert
    assert url == expected


def test_unparsable_text_shows_the_offending_input():
    """解釈失敗メッセージに入力文字列が添えられることのテスト。"""
    # Act
    message = unparsable_text("notacolor")

    # Assert
    assert message == "色を解釈できません: notacolor"


@pytest.fixture()
def app() -> Iterator[PaletteApp]:
    """非表示の PaletteApp。Tk を初期化できない環境ではスキップする。"""
    window = create_tk_window(
        lambda: PaletteApp(ColorMatrix(FIXTURE_PALETTE), NEAR_THRESHOLD, None)
    )
    window.withdraw()
    yield window
    window.destroy()


def _opened_urls(monkeypatch: MonkeyPatch) -> list[str]:
    """webbrowser.open を差し替え、開かれた URL を記録する。"""
    opened: list[str] = []
    monkeypatch.setattr(
        color_palette_gui.webbrowser, "open", lambda url: opened.append(url)
    )
    return opened


@WINDOWS_ONLY
@pytest.mark.parametrize(
    ("text", "expected_url"),
    [
        ("#69A5FF", "https://www.color-sample.com/colors/69A5FF/"),
        ("255, 0, 0", "https://www.color-sample.com/colors/FF0000/"),
        ("PureRed", "https://www.color-sample.com/colors/FF0000/"),
    ],
)
def test_open_color_sample_opens_the_entry_color(
    app: PaletteApp, monkeypatch: MonkeyPatch, text: str, expected_url: str
):
    """検索ボタンは入力欄のパレット名も含めた色で URL を開くことのテスト。"""
    # Arrange
    opened = _opened_urls(monkeypatch)
    app.entry.insert(tk.END, text)

    # Act
    app._open_color_sample()

    # Assert
    assert opened == [expected_url]


@WINDOWS_ONLY
def test_open_color_sample_rejects_unparsable_entry(
    app: PaletteApp, monkeypatch: MonkeyPatch
):
    """解釈できない入力のときはブラウザを開かず状態欄へ出すことのテスト。"""
    # Arrange
    opened = _opened_urls(monkeypatch)
    app.entry.insert(tk.END, "notacolor")

    # Act
    app._open_color_sample()

    # Assert
    assert opened == []
    assert app.status.cget("text") == unparsable_text("notacolor")
