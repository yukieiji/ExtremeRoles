"""マトリクス横に表示するカラーパレット一覧（宣言順）を提供します。"""

import tkinter as tk
from collections.abc import Callable, Iterable
from tkinter import font as tkfont

from .color_matrix import ColorMatrix
from .color_space import Rgb, to_hex, to_rgb_text
from .palette_colors import PaletteColor

ROW_HEIGHT = 20
LIST_WIDTH = 396
SWATCH_SIZE = 12
ROW_COLOR = "#252525"
ROW_MATCH_COLOR = "#4a3f18"
ROW_SELECT_COLOR = "#1f4a63"
ROW_INPUT_COLOR = "#3a2b45"
POSITION_COLOR = "#9cdcfe"
VISIBLE_LEAD = 6


class PaletteListBox(tk.Frame):
    """パレット項目を宣言順に並べ、マトリクス上の位置を添えて表示する一覧。"""

    def __init__(
        self,
        parent: tk.Misc,
        matrix: ColorMatrix,
        on_select: Callable[[PaletteColor], None],
    ):
        super().__init__(parent)
        self._matrix = matrix
        self._on_select = on_select
        self._row_backgrounds: dict[str, int] = {}
        self._orders: dict[str, int] = {}

        self._face = tkfont.Font(family="Meiryo", size=9)
        self._input = tk.Canvas(
            self,
            width=LIST_WIDTH,
            height=ROW_HEIGHT,
            bg=ROW_INPUT_COLOR,
            highlightthickness=0,
        )
        self._input.grid(row=0, column=0, columnspan=2, sticky="ew")

        self._canvas = tk.Canvas(
            self, width=LIST_WIDTH, height=200, bg=ROW_COLOR, highlightthickness=0
        )
        bar = tk.Scrollbar(self, orient=tk.VERTICAL, command=self._canvas.yview)
        self._canvas.configure(yscrollcommand=bar.set)
        self._canvas.grid(row=1, column=0, sticky="nsew")
        bar.grid(row=1, column=1, sticky="ns")
        self.grid_rowconfigure(1, weight=1)
        self.grid_columnconfigure(0, weight=1)

        self._draw_rows()
        self._canvas.bind("<Button-1>", self._handle_click)
        self._canvas.bind("<MouseWheel>", self._handle_wheel)
        self._input.bind("<MouseWheel>", self._handle_wheel)

    def set_input_color(self, rgb: Rgb | None) -> None:
        """判定中の入力色を一覧の最上段 (宣言順とは別枠) に表示します。"""
        self._input.delete("all")
        if rgb is None:
            return

        middle = ROW_HEIGHT // 2
        self._input.create_rectangle(
            6,
            middle - SWATCH_SIZE // 2,
            6 + SWATCH_SIZE,
            middle + SWATCH_SIZE // 2,
            fill=to_hex(rgb),
            outline="#777777",
        )
        self._input.create_text(
            28,
            middle,
            anchor="w",
            font=self._face,
            fill="#ffffff",
            text=f"入力色 {to_rgb_text(rgb)}",
        )
        self._input.create_text(
            LIST_WIDTH - 10,
            middle,
            anchor="e",
            font=self._face,
            fill=POSITION_COLOR,
            text=self._matrix.ideal_summary(rgb),
        )

    def highlight(self, names: Iterable[str], selected: str | None) -> None:
        """近似候補を強調し、選択された項目を別色にしてその行へスクロールします。"""
        wanted = set(names)
        for name, background in self._row_backgrounds.items():
            fill = ROW_COLOR
            if name in wanted:
                fill = ROW_MATCH_COLOR
            if name == selected:
                fill = ROW_SELECT_COLOR
            self._canvas.itemconfigure(background, fill=fill)

        order = self._orders.get(selected)
        if order is not None:
            self._canvas.after_idle(self._scroll_to, order)

    def _scroll_to(self, order: int) -> None:
        total = ROW_HEIGHT * len(self._matrix.colors)
        top = max(0.0, (order - VISIBLE_LEAD) * ROW_HEIGHT / total)
        self._canvas.yview_moveto(min(top, 1.0))

    def _draw_rows(self) -> None:
        for order, color in enumerate(self._matrix.colors):
            self._orders[color.name] = order
            top = order * ROW_HEIGHT
            middle = top + ROW_HEIGHT // 2
            self._row_backgrounds[color.name] = self._canvas.create_rectangle(
                0, top, LIST_WIDTH, top + ROW_HEIGHT, fill=ROW_COLOR, outline=""
            )
            self._canvas.create_rectangle(
                6,
                top + (ROW_HEIGHT - SWATCH_SIZE) // 2,
                6 + SWATCH_SIZE,
                top + (ROW_HEIGHT + SWATCH_SIZE) // 2,
                fill=to_hex(color.rgb),
                outline="#555555",
            )
            self._canvas.create_text(
                28,
                middle,
                anchor="w",
                font=self._face,
                fill="#eeeeee",
                text=f"{color.index:>3} {color.name} {to_rgb_text(color.rgb)}",
            )
            self._canvas.create_text(
                LIST_WIDTH - 10,
                middle,
                anchor="e",
                font=self._face,
                fill=POSITION_COLOR,
                text=self._matrix.cell_summary(color),
            )

        height = ROW_HEIGHT * len(self._matrix.colors)
        self._canvas.configure(scrollregion=(0, 0, LIST_WIDTH, height))

    def _handle_click(self, event: tk.Event) -> None:
        color = self._color_at(event.y)
        if color is not None:
            self._on_select(color)

    def _handle_wheel(self, event: tk.Event) -> None:
        units = int(-event.delta / 120)
        if units == 0:
            units = -1 if event.delta > 0 else 1
        self._canvas.yview_scroll(units, "units")

    def _color_at(self, y: int) -> PaletteColor | None:
        visible_top = self._canvas.yview()[0]
        offset = visible_top * ROW_HEIGHT * len(self._matrix.colors)
        order = int((y + offset) // ROW_HEIGHT)
        if order < 0 or order >= len(self._matrix.colors):
            return None
        return self._matrix.colors[order]
