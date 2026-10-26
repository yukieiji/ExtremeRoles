"""HSV カラーホイール（角度=色相 / 半径=彩度）でパレット上の位置を示します。"""

import math
import tkinter as tk
from collections.abc import Callable, Iterable

from .color_space import Rgb, from_hsv, hue, saturation, to_hex, to_rgb_text
from .color_tooltip import CellTooltip
from .palette_colors import PaletteColor

SECTORS = 72
RINGS = 12
MARKER_RADIUS = 4.5
SELECTED_RADIUS = 7.5
MARGIN = 30
MIN_MARKED_SATURATION = 0.05
LABEL_RADIUS_RATIO = 1.1
LABEL_TEXT_HALF = 12
LABELS = (
    ("赤", 0.0),
    ("黄", 60.0),
    ("緑", 120.0),
    ("水色", 190.0),
    ("青", 240.0),
    ("マゼンタ", 300.0),
)
NEARBY_PIXELS = 14.0
RIM_TOLERANCE = 1e-6
TARGET_RADIUS = 10.0
TARGET_DOT_RADIUS = 3.5


def wheel_point(hue_degrees: float, saturation_value: float) -> tuple[float, float]:
    """単位円上の位置 (x, y) を返します。x 軸正方向が赤、反時計回りに増えます。"""
    angle = math.radians(hue_degrees)
    return (
        saturation_value * math.cos(angle),
        -saturation_value * math.sin(angle),
    )


def color_at(x: float, y: float, center: float, radius: float) -> Rgb | None:
    """ホイール上の座標からの色を返します。円外 (右クリック無効) は None を返します。"""
    offset_x = x - center
    offset_y = y - center
    distance = math.hypot(offset_x, offset_y)
    if distance > radius + RIM_TOLERANCE:
        return None

    angle = math.degrees(math.atan2(-offset_y, offset_x)) % 360.0
    return from_hsv(angle, min(distance / radius, 1.0), 1.0)


def label_points(radius: int) -> list[tuple[str, float, float]]:
    """色名ラベルの、ホイール中心からの相対座標を返します。"""
    reach = radius * LABEL_RADIUS_RATIO
    return [
        (text, x * reach, y * reach)
        for text, angle in LABELS
        for x, y in [wheel_point(angle, 1.0)]
    ]


class ColorWheel(tk.Canvas):
    """色相を角度・彩度を半径としたホイール。マーカーでパレット色を示します。"""

    def __init__(self, parent: tk.Misc, radius: int, on_select: Callable[[str], None]):
        size = radius * 2 + MARGIN * 2
        super().__init__(
            parent, width=size, height=size, bg="#1e1e1e", highlightthickness=0
        )
        self._radius = radius
        self._center = radius + MARGIN
        self._on_select = on_select
        self._entries: tuple[PaletteColor, ...] = ()
        self._markers: dict[str, list[tuple[int, float, float]]] = {}

        self._draw_background()
        self.bind("<Button-1>", self._handle_click)
        self.bind("<Button-3>", self._handle_pick)
        self._tooltip = CellTooltip(self, self._tooltip_lines)

    def set_entries(self, entries: tuple[PaletteColor, ...]) -> None:
        """表示するパレット項目を設定してマーカーを描き直します。"""
        self._entries = entries
        self._redraw_markers()

    def highlight(self, names: Iterable[str], selected: str | None) -> None:
        """近似候補と選択中マーカーの見た目を変えます。"""
        wanted = set(names)
        for name, markers in self._markers.items():
            radius = SELECTED_RADIUS if name == selected else MARKER_RADIUS
            outline = "#ffd54f" if name in wanted else "#ffffff"
            width = 3.0 if name == selected else 1.2
            for item, center_x, center_y in markers:
                self.coords(
                    item,
                    center_x - radius,
                    center_y - radius,
                    center_x + radius,
                    center_y + radius,
                )
                self.itemconfigure(item, outline=outline, width=width)

    def show_target(self, rgb: Rgb | None) -> None:
        """判定中の色をホイール上のリングで示します。None では消去します。"""
        self.delete("target")
        if rgb is None:
            return

        center_x, center_y = self._marker_center(rgb)
        self.create_oval(
            center_x - TARGET_RADIUS,
            center_y - TARGET_RADIUS,
            center_x + TARGET_RADIUS,
            center_y + TARGET_RADIUS,
            outline="#000000",
            width=4.0,
            tags=("target",),
        )
        self.create_oval(
            center_x - TARGET_RADIUS,
            center_y - TARGET_RADIUS,
            center_x + TARGET_RADIUS,
            center_y + TARGET_RADIUS,
            outline="#ffffff",
            width=1.6,
            tags=("target",),
        )
        self.create_oval(
            center_x - TARGET_DOT_RADIUS,
            center_y - TARGET_DOT_RADIUS,
            center_x + TARGET_DOT_RADIUS,
            center_y + TARGET_DOT_RADIUS,
            fill=to_hex(rgb),
            outline="#000000",
            width=1.0,
            tags=("target",),
        )
        self.tag_raise("target")

    def _draw_background(self) -> None:
        step_angle = 360.0 / SECTORS
        for ring in range(RINGS):
            inner = self._radius * ring / RINGS
            outer = self._radius * (ring + 1) / RINGS
            for sector in range(SECTORS):
                start = sector * step_angle
                end = start + step_angle
                corners = ((start, inner), (start, outer), (end, outer), (end, inner))
                points: list[float] = []
                for angle, radius in corners:
                    x, y = wheel_point(angle, radius / self._radius)
                    points.extend(
                        (
                            self._center + x * self._radius,
                            self._center + y * self._radius,
                        )
                    )
                self.create_polygon(
                    *points,
                    fill=to_hex(
                        from_hsv(start + step_angle / 2.0, (ring + 1) / RINGS, 1.0)
                    ),
                    outline="",
                )
        for text, x, y in label_points(self._radius):
            self.create_text(
                self._center + x,
                self._center + y,
                text=text,
                fill="#dddddd",
                font=("Meiryo", 8),
            )

    def _redraw_markers(self) -> None:
        self.delete("marker")
        self._markers = {}
        for entry in self._entries:
            center_x, center_y = self._marker_center(entry.rgb)
            item = self.create_oval(
                center_x - MARKER_RADIUS,
                center_y - MARKER_RADIUS,
                center_x + MARKER_RADIUS,
                center_y + MARKER_RADIUS,
                fill=to_hex(entry.rgb),
                outline="#ffffff",
                width=1.2,
                tags=("marker",),
            )
            self._markers.setdefault(entry.name, []).append((item, center_x, center_y))
        self.tag_raise("target")

    def _marker_center(self, rgb: Rgb) -> tuple[float, float]:
        """指定色をホイール上に置いたときの中心座標 (パレットマーカーと目印で共通)。"""
        x, y = wheel_point(hue(rgb), max(saturation(rgb), MIN_MARKED_SATURATION))
        return (self._center + x * self._radius, self._center + y * self._radius)

    def _tooltip_lines(self, x: int, y: int) -> list[str]:
        nearby = self._entries_near(x, y)
        if not nearby:
            return []
        return [
            f"{entry.index:>3} {entry.name} {to_rgb_text(entry.rgb)}"
            for entry in nearby
        ]

    def _entries_near(self, x: int, y: int) -> list[PaletteColor]:
        found: list[tuple[float, PaletteColor]] = []
        for entry in self._entries:
            center_x, center_y = self._marker_center(entry.rgb)
            distance = math.hypot(center_x - x, center_y - y)
            if distance <= NEARBY_PIXELS:
                found.append((distance, entry))
        return [entry for _, entry in sorted(found, key=lambda pair: pair[0])[:6]]

    def _handle_click(self, event: tk.Event) -> None:
        nearby = self._entries_near(event.x, event.y)
        if nearby:
            self._on_select(to_rgb_text(nearby[0].rgb))

    def _handle_pick(self, event: tk.Event) -> None:
        """右クリック位置のホイール上の色を判定対象にします。"""
        picked = color_at(event.x, event.y, self._center, self._radius)
        if picked is not None:
            self._on_select(to_rgb_text(picked))
