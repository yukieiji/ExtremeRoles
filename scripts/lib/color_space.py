"""色空間の変換と色差計算を提供します。"""

import math
import re
from collections.abc import Iterable
from dataclasses import dataclass

_HEX = re.compile(r"^#?(?P<value>[0-9a-fA-F]+)$")
_RGB_TRIPLE = re.compile(
    r"^\(?\s*(?P<r>\d{1,3})\s*(?P<sep>[,\s])\s*(?P<g>\d{1,3})\s*(?P=sep)\s*"
    r"(?P<b>\d{1,3})\s*,?\s*\)?$"
)


@dataclass(frozen=True)
class Rgb:
    """0〜255 の RGB 値。"""

    r: int
    g: int
    b: int

    def __post_init__(self) -> None:
        for name, value in (("r", self.r), ("g", self.g), ("b", self.b)):
            if not 0 <= value <= 255:
                raise ValueError(f"Rgb.{name} が 0〜255 の範囲外です: {value}")


@dataclass(frozen=True)
class Lab:
    """CIE L*a*b* (D65) 値。"""

    l_star: float
    a_star: float
    b_star: float


def parse_color(text: str) -> Rgb | None:
    """色表記を Rgb に変換します。解釈できない場合は None を返します。

    対応形式: #RGB / #RRGGBB / #RRGGBBAA (AA 無視) / # 無し / "r,g,b" / "r g b"。
    """
    stripped = text.strip()
    if not stripped:
        return None

    triple = _RGB_TRIPLE.match(stripped)
    if triple:
        return _to_rgb(int(triple.group(part)) for part in ("r", "g", "b"))

    hex_match = _HEX.match(stripped)
    if not hex_match:
        return None

    digits = _expand_hex(hex_match.group("value"))
    if digits is None:
        return None

    return _to_rgb(int(digits[i : i + 2], 16) for i in (0, 2, 4))


def _expand_hex(digits: str) -> str | None:
    if len(digits) in (3, 4):
        return "".join(digit * 2 for digit in digits[:3])
    if len(digits) in (6, 8):
        return digits[:6]
    return None


def _to_rgb(values: Iterable[int]) -> Rgb | None:
    channels = tuple(values)
    if any(channel > 255 for channel in channels):
        return None
    return Rgb(*channels)


def to_hex(rgb: Rgb) -> str:
    """大文字の #RRGGBB 表現を返します。"""
    return f"#{rgb.r:02X}{rgb.g:02X}{rgb.b:02X}"


def to_rgb_text(rgb: Rgb) -> str:
    """表示用の RGB 文字列 (例: 255, 25, 25) を返します。"""
    return f"{rgb.r}, {rgb.g}, {rgb.b}"


def to_lab(rgb: Rgb) -> Lab:
    """sRGB を CIE L*a*b* (D65) に変換します。"""
    x, y, z = _to_xyz(rgb)
    fx = _pivot(x / 0.95047)
    fy = _pivot(y)
    fz = _pivot(z / 1.08883)
    return Lab(
        l_star=(116.0 * fy) - 16.0,
        a_star=500.0 * (fx - fy),
        b_star=200.0 * (fy - fz),
    )


def delta_e76(first: Rgb, second: Rgb) -> float:
    """2色の CIE76 色差 (ΔE*ab) を返します。"""
    origin, target = to_lab(first), to_lab(second)
    return math.sqrt(
        (origin.l_star - target.l_star) ** 2
        + (origin.a_star - target.a_star) ** 2
        + (origin.b_star - target.b_star) ** 2
    )


def hue(rgb: Rgb) -> float:
    """HSV 色相角 (0〜360度) を返します。無彩色の扱いは color_matrix.is_achromatic() 側で行います。"""
    red, green, blue = (channel / 255.0 for channel in (rgb.r, rgb.g, rgb.b))
    highest = max(red, green, blue)
    spread = highest - min(red, green, blue)
    if spread <= 0.0:
        return 0.0

    if highest == red:
        offset = ((green - blue) / spread) % 6.0
    elif highest == green:
        offset = ((blue - red) / spread) + 2.0
    else:
        offset = ((red - green) / spread) + 4.0
    return (60.0 * offset) % 360.0


def saturation(rgb: Rgb) -> float:
    """HSV 彩度 (0〜1) を返します。"""
    channels = tuple(channel / 255.0 for channel in (rgb.r, rgb.g, rgb.b))
    highest = max(channels)
    lowest = min(channels)
    if highest <= 0.0:
        return 0.0
    return (highest - lowest) / highest


def value(rgb: Rgb) -> float:
    """HSV 明度 V (0〜1) を返します。"""
    return max(rgb.r, rgb.g, rgb.b) / 255.0


def from_hsv(hue_degrees: float, saturation_value: float, brightness: float) -> Rgb:
    """HSV から Rgb を作ります (h:0〜360, s/v:0〜1)。"""
    position = (hue_degrees % 360.0) / 60.0
    chroma = brightness * saturation_value
    second = chroma * (1.0 - abs((position % 2.0) - 1.0))
    sectors = (
        (chroma, second, 0.0),
        (second, chroma, 0.0),
        (0.0, chroma, second),
        (0.0, second, chroma),
        (second, 0.0, chroma),
        (chroma, 0.0, second),
    )
    red, green, blue = sectors[int(position) % 6]

    offset = brightness - chroma
    return Rgb(
        r=round((red + offset) * 255),
        g=round((green + offset) * 255),
        b=round((blue + offset) * 255),
    )


def chroma(rgb: Rgb) -> float:
    """彩度 C*ab を返します。"""
    lab = to_lab(rgb)
    return math.hypot(lab.a_star, lab.b_star)


def lightness(rgb: Rgb) -> float:
    """明度 L* を返します。"""
    return to_lab(rgb).l_star


def _to_xyz(rgb: Rgb) -> tuple[float, float, float]:
    r, g, b = (_to_linear(channel / 255.0) for channel in (rgb.r, rgb.g, rgb.b))
    return (
        0.4124 * r + 0.3576 * g + 0.1805 * b,
        0.2126 * r + 0.7152 * g + 0.0722 * b,
        0.0193 * r + 0.1192 * g + 0.9505 * b,
    )


def _to_linear(channel: float) -> float:
    if channel <= 0.04045:
        return channel / 12.92
    return ((channel + 0.055) / 1.055) ** 2.4


def _pivot(value: float) -> float:
    if value > 0.008856:
        return value ** (1.0 / 3.0)
    return (7.787 * value) + (16.0 / 116.0)
