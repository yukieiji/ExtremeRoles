"""ExtremeRoles カラーパレットのマトリクス表示 GUI を提供します。"""

import sys
import tkinter as tk
import webbrowser
from pathlib import Path
from tkinter import colorchooser

from .color_matrix import (
    MAX_LISTED_CANDIDATES,
    Analysis,
    ColorMatrix,
)
from .color_matrix_view import MatrixView
from .color_palette_list import PaletteListBox
from .color_space import Rgb, to_hex, to_rgb_text
from .color_wheel import ColorWheel
from .palette_colors import PaletteColor
from .window_shot import capture_window, root_handle

WINDOW_TITLE = "ExtremeRoles ColorPalette Matrix"
WHEEL_RADIUS = 138
DEBUG_DELAY_MS = 900
DEBUG_RETRY_DELAY_MS = 300
DEBUG_MAX_ATTEMPTS = 6
DEBUG_SIZE_RATIO = 0.9
VERDICT_LINE = 2
VERDICT_COLORS = {
    "ok": "#0a7d32",
    "near": "#b26a00",
    "far": "#a40000",
}
# apex (color-sample.com) は https 接続が切断される為 www 付きを使う。遷移先は colors/<16進>/ 形式。
COLOR_SAMPLE_URL = "https://www.color-sample.com/colors/{hex_value}/"


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
        self.debug_status = 0

        self.title(WINDOW_TITLE)
        self.geometry("2300x980")

        self._build_controls()
        body = tk.Frame(self)
        body.pack(fill=tk.X)
        self.matrix_view = MatrixView(body, matrix, self.set_color)
        self.matrix_view.pack(side=tk.LEFT, fill=tk.Y, padx=8)
        self.wheel = ColorWheel(body, WHEEL_RADIUS, self.set_color)
        self.wheel.pack(side=tk.LEFT, fill=tk.Y, padx=8)
        self.wheel.set_entries(matrix.colors)
        self.palette_list = PaletteListBox(body, matrix, self._select_palette_color)
        self.palette_list.pack(side=tk.LEFT, fill=tk.Y, padx=(0, 8))
        self._build_detail()

        if debug_shot is not None:
            self.after(DEBUG_DELAY_MS, lambda: self._capture_debug_shot(debug_shot, 1))

    # --- UI 構築 ---

    def _build_controls(self) -> None:
        bar = tk.Frame(self, padx=8, pady=6)
        bar.pack(fill=tk.X)
        tk.Label(bar, text="色 (r, g, b / hex / 名前):").pack(side=tk.LEFT)
        self.entry = tk.Entry(bar, width=16)
        self.entry.pack(side=tk.LEFT, padx=4)
        self.entry.bind("<Return>", lambda _: self.analyze_text(self.entry.get()))
        tk.Button(
            bar, text="判定", command=lambda: self.analyze_text(self.entry.get())
        ).pack(side=tk.LEFT)
        tk.Button(bar, text="色を選ぶ…", command=self._pick_color).pack(
            side=tk.LEFT, padx=4
        )
        tk.Button(
            bar, text="color-sample.com で検索", command=self._open_color_sample
        ).pack(side=tk.LEFT)
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
            self.matrix_view.show(None)
            self.wheel.highlight((), None)
            self.palette_list.highlight((), None)
            self.palette_list.set_input_color(None)
            self.status.config(
                text=unparsable_text(text), foreground=VERDICT_COLORS["far"]
            )
            return

        analysis = self.matrix.analyze(rgb, self.near_threshold)
        self.palette_list.set_input_color(rgb)
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
            f"入力色        : {to_rgb_text(analysis.target)}  {self.matrix.position_text(analysis.target)}",
            f"判定          : {analysis.verdict} (dE={nearest.delta_e:.2f}, 閾値 {self.near_threshold:.0f})",
            f"最近傍パレット: {nearest}",
            f"  パレット上  : {self.matrix.palette_position(nearest.color)}",
            f"  マトリクス  : {self.matrix.position_text_of(nearest.color)}",
            f"近似候補 ({len(analysis.near_matches)}件)",
        ]
        for match in analysis.near_matches[:MAX_LISTED_CANDIDATES]:
            lines.append(
                f"  #{match.color.index:>3} {self.matrix.cell_summary(match.color):<16} {match}"
            )
        same = self.matrix.names_with_rgb(analysis.target)
        lines.append(f"入力色と同一RGB: {', '.join(same) if same else 'なし'}")

        self.detail.config(state=tk.NORMAL)
        self.detail.delete("1.0", tk.END)
        self.detail.insert(tk.END, "\n".join(lines) + "\n")
        self.detail.tag_add(
            _verdict_tag(analysis),
            f"{VERDICT_LINE}.0",
            f"{VERDICT_LINE + 1}.0",
        )
        self.detail.config(state=tk.DISABLED)

    # --- イベント ---

    def set_color(self, text: str) -> None:
        """入力欄に色を表す文字列を載せて判定します。"""
        self.entry.delete(0, tk.END)
        self.entry.insert(tk.END, text)
        self.analyze_text(text)

    def _select_palette_color(self, color: PaletteColor) -> None:
        self.set_color(to_rgb_text(color.rgb))

    def _pick_color(self) -> None:
        rgb_values, _ = colorchooser.askcolor(parent=self, title="判定したい色を選択")
        if rgb_values is None:
            return

        red, green, blue = rgb_values
        self.set_color(to_rgb_text(Rgb(round(red), round(green), round(blue))))

    def _open_color_sample(self) -> None:
        """color-sample.com の色詳細ページをブラウザで開きます。"""
        rgb = self.matrix.resolve(self.entry.get())
        if rgb is None:
            self.status.config(
                text=unparsable_text(self.entry.get()),
                foreground=VERDICT_COLORS["far"],
            )
            return

        webbrowser.open(color_sample_url(rgb))

    def _capture_debug_shot(self, path: Path, attempt: int) -> None:
        """ウィンドウが期待サイズで描画できるまで撮影を再試行します。"""
        self.update_idletasks()
        try:
            width, height = capture_window(root_handle(self.winfo_id()), path)
            ready = is_rendered(width, height, self.winfo_width(), self.winfo_height())
            result = f"スクリーンショット保存: {path} ({width}x{height})"
        except (OSError, LookupError) as error:
            ready = False
            result = f"スクリーンショット失敗: {error}"

        if not ready and attempt < DEBUG_MAX_ATTEMPTS:
            self.after(
                DEBUG_RETRY_DELAY_MS,
                lambda: self._capture_debug_shot(path, attempt + 1),
            )
            return

        self.debug_status = 0 if ready else 1
        stream = sys.stdout if ready else sys.stderr
        print(f"{result} [{attempt}回目]", file=stream)
        self.destroy()


def color_sample_url(color: Rgb) -> str:
    """color-sample.com の色詳細 URL を返します。"""
    return COLOR_SAMPLE_URL.format(hex_value=to_hex(color).lstrip("#"))


def unparsable_text(text: str) -> str:
    """解釈できない入力を表す共通メッセージを返します。"""
    return f"色を解釈できません: {text}"


def is_rendered(
    width: int, height: int, expected_width: int, expected_height: int
) -> bool:
    """撮影サイズがウィンドウの期待サイズに達しているかを返します。

    Tk は未マッピングのウィジェット寸法を 1 と報告する為、期待値が 1 以下なら未描画とします。
    """
    if expected_width <= 1 or expected_height <= 1:
        return False

    return (
        width >= expected_width * DEBUG_SIZE_RATIO
        and height >= expected_height * DEBUG_SIZE_RATIO
    )


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
