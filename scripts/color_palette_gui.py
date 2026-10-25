"""ExtremeRoles カラーパレットのマトリクス表示 GUI を提供します。"""

import sys
import tkinter as tk
from pathlib import Path
from tkinter import colorchooser

from color_matrix import Analysis, ColorMatrix
from color_matrix_view import MatrixView
from color_palette_list import PaletteListBox
from color_space import to_hex
from color_wheel import ColorWheel
from palette_colors import PaletteColor
from window_shot import capture_window

WINDOW_TITLE = "ExtremeRoles ColorPalette Matrix"
WHEEL_RADIUS = 138
DEBUG_DELAY_MS = 900
VERDICT_COLORS = {
    "ok": "#0a7d32",
    "near": "#b26a00",
    "far": "#a40000",
}


class PaletteApp(tk.Tk):
    """カラーマトリクス・カラーホイール・一覧と近似色判定を表示するウィンドウ。"""

    def __init__(
        self,
        matrix: ColorMatrix,
        near_threshold: float,
        debug_shot: Path | None,
    ):
        super().__init__()
        self.matrix = matrix
        self.near_threshold = near_threshold
        self.debug_shot = debug_shot
        self.debug_status = 0
        self.analysis: Analysis | None = None

        self.title(WINDOW_TITLE)
        self.geometry("2300x980")

        self._build_controls()
        body = tk.Frame(self)
        body.pack(fill=tk.X)
        self.matrix_view = MatrixView(body, matrix, self._set_entry)
        self.matrix_view.pack(side=tk.LEFT, fill=tk.Y, padx=8)
        self.wheel = ColorWheel(body, WHEEL_RADIUS, self._set_entry)
        self.wheel.pack(side=tk.LEFT, fill=tk.Y, padx=8)
        self.wheel.set_entries(matrix.colors)
        self.palette_list = PaletteListBox(body, matrix, self._select_palette_color)
        self.palette_list.pack(side=tk.LEFT, fill=tk.Y, padx=(0, 8))
        self._build_detail()

        if self.debug_shot is not None:
            shot = self.debug_shot
            self.after(DEBUG_DELAY_MS, lambda: self._capture_debug_shot(shot))

    # --- UI 構築 ---

    def _build_controls(self) -> None:
        bar = tk.Frame(self, padx=8, pady=6)
        bar.pack(fill=tk.X)
        tk.Label(bar, text="色 (hex / r,g,b):").pack(side=tk.LEFT)
        self.entry = tk.Entry(bar, width=16)
        self.entry.pack(side=tk.LEFT, padx=4)
        self.entry.bind("<Return>", lambda _: self.analyze_text(self.entry.get()))
        tk.Button(
            bar, text="判定", command=lambda: self.analyze_text(self.entry.get())
        ).pack(side=tk.LEFT)
        tk.Button(bar, text="色を選ぶ…", command=self._pick_color).pack(
            side=tk.LEFT, padx=4
        )
        self.status = tk.Label(bar, text="クリックでその色を判定", anchor=tk.W)
        self.status.pack(side=tk.LEFT, fill=tk.X, padx=10)

    def _build_detail(self) -> None:
        self.detail = tk.Text(
            self, height=13, wrap=tk.WORD, font=("Consolas", 10), state=tk.DISABLED
        )
        self.detail.pack(fill=tk.BOTH, expand=True, padx=8, pady=8)
        for tag, color in VERDICT_COLORS.items():
            self.detail.tag_configure(tag, foreground=color)

    # --- 判定 ---

    def analyze_text(self, text: str) -> None:
        """入力文字列を解析してマトリクス・ホイール・詳細欄を更新します。"""
        rgb = self.matrix.resolve(text)
        if rgb is None:
            self.analysis = None
            self.matrix_view.show(None)
            self.wheel.highlight((), None)
            self.palette_list.highlight((), None)
            self.status.config(text=f"色を解釈できません: {text}", foreground="#a40000")
            return

        analysis = self.matrix.analyze(rgb, self.near_threshold)
        self.analysis = analysis
        self.status.config(
            text=_verdict_text(analysis), foreground=_verdict_color(analysis)
        )
        self.matrix_view.show(analysis)
        self._write_detail(analysis)
        names = tuple(match.color.name for match in analysis.near_matches)
        self.wheel.highlight(names, analysis.nearest.color.name)
        self.palette_list.highlight(names, analysis.nearest.color.name)

    def _write_detail(self, analysis: Analysis) -> None:
        nearest = analysis.nearest
        lines = [
            f"入力色        : {to_hex(analysis.target)}  {self.matrix.position_text(analysis.target)}",
            f"判定          : {analysis.verdict} (dE={nearest.delta_e:.2f}, 閾値 {self.near_threshold:.0f})",
            f"最近傍パレット: {nearest}",
            f"  パレット上  : {self.matrix.palette_position(nearest.color)}",
            f"  マトリクス  : {self.matrix.position_text_of(nearest.color)}",
            f"近似候補 ({len(analysis.near_matches)}件)",
        ]
        for match in analysis.near_matches[:8]:
            lines.append(
                f"  #{match.color.index:>3} {self.matrix.cell_summary(match.color):<16} {match}"
            )
        same = self.matrix.names_with_rgb(analysis.target)
        lines.append(f"入力色と同一RGB: {', '.join(same) if same else 'なし'}")

        self.detail.config(state=tk.NORMAL)
        self.detail.delete("1.0", tk.END)
        self.detail.insert(tk.END, "\n".join(lines) + "\n")
        self.detail.tag_add(_verdict_tag(analysis), "2.0", "3.0")
        self.detail.config(state=tk.DISABLED)

    # --- イベント ---

    def _set_entry(self, text: str) -> None:
        self.entry.delete(0, tk.END)
        self.entry.insert(tk.END, text)
        self.analyze_text(text)

    def _select_palette_color(self, color: PaletteColor) -> None:
        self._set_entry(to_hex(color.rgb))

    def _pick_color(self) -> None:
        _, hex_value = colorchooser.askcolor(parent=self, title="判定したい色を選択")
        if hex_value is None:
            return
        self._set_entry(hex_value)

    def _capture_debug_shot(self, path: Path) -> None:
        try:
            width, height = capture_window(WINDOW_TITLE, path)
            print(f"スクリーンショット保存: {path} ({width}x{height})")
        except (OSError, LookupError) as error:
            self.debug_status = 1
            print(f"スクリーンショット失敗: {error}", file=sys.stderr)
        self.destroy()


def _verdict_text(analysis: Analysis) -> str:
    return f"{analysis.verdict}: {analysis.nearest.color.name} (dE={analysis.nearest.delta_e:.1f})"


def _verdict_tag(analysis: Analysis) -> str:
    if analysis.is_exact:
        return "ok"
    if analysis.is_near:
        return "near"
    return "far"


def _verdict_color(analysis: Analysis) -> str:
    return VERDICT_COLORS[_verdict_tag(analysis)]
