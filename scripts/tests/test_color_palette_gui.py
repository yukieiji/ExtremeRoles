"""color_palette_gui.py の判定表示ヘルパーのテスト。"""

import pytest

from color_matrix import EXACT_DELTA_E, Analysis, MatchResult
from color_palette_gui import VERDICT_COLORS, _verdict_color, _verdict_tag
from color_space import Rgb
from palette_colors import PaletteColor

TARGET = Rgb(105, 165, 255)
NEAR_THRESHOLD = 25.0


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
