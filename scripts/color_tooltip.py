"""マトリクスのセルにマウスを乗せたとき、そのセルの内訳を出すツールチップ。"""

import tkinter as tk
from collections.abc import Callable

TOOLTIP_PAD = 16


class CellTooltip:
    """セル上のホバーでそのセルに含まれる色の一覧を表示する。"""

    def __init__(self, canvas: tk.Canvas, provider: Callable[[int, int], list[str]]):
        self._canvas = canvas
        self._provider = provider
        self._window: tk.Toplevel | None = None
        self._label: tk.Label | None = None

        canvas.bind("<Motion>", self._on_motion, add=True)
        canvas.bind("<Leave>", lambda _: self.hide(), add=True)

    def _on_motion(self, event: tk.Event) -> None:
        lines = self._provider(event.x, event.y)
        if not lines:
            self.hide()
            return

        window, label = self._ensure_window()
        label.config(text="\n".join(lines))
        window.wm_geometry(
            f"+{self._canvas.winfo_rootx() + event.x + TOOLTIP_PAD}"
            f"+{self._canvas.winfo_rooty() + event.y + TOOLTIP_PAD}"
        )
        window.deiconify()

    def _ensure_window(self) -> tuple[tk.Toplevel, tk.Label]:
        if self._window is None or self._label is None:
            window = tk.Toplevel(self._canvas)
            window.withdraw()
            window.wm_overrideredirect(True)
            label = tk.Label(
                window,
                justify=tk.LEFT,
                bg="#1b1b1b",
                fg="#f2f2f2",
                font=("Meiryo", 9),
                padx=7,
                pady=5,
            )
            label.pack()
            self._window, self._label = window, label
        return self._window, self._label

    def hide(self) -> None:
        if self._window is not None:
            self._window.withdraw()
