"""scripts 配下のテスト共通部品。sys.path 設定と Tk ウィンドウ用 fixture を置きます。"""

import sys
from collections.abc import Callable, Iterator
from pathlib import Path
from typing import TYPE_CHECKING

import pytest

if TYPE_CHECKING:
    from tkinter import Tk

SCRIPTS_DIR = Path(__file__).resolve().parent
TK_CREATE_ATTEMPTS = 3

if str(SCRIPTS_DIR) not in sys.path:
    sys.path.insert(0, str(SCRIPTS_DIR))


def create_tk_window[T](factory: Callable[[], T]) -> T:
    """Tk 系ウィンドウを作ります。一時的な Tcl 初期化失敗は再試行し、無理なら skip。"""
    try:
        import tkinter as tk
    except ImportError as error:
        pytest.skip(f"tkinter が利用できないため: {error}")

    last_error: Exception = RuntimeError("ウィンドウを 1 度も生成できていません")
    for _ in range(TK_CREATE_ATTEMPTS):
        try:
            return factory()
        except tk.TclError as error:
            last_error = error

    pytest.skip(f"Tk を初期化できないため: {last_error}")


@pytest.fixture()
def tk_root() -> "Iterator[Tk]":
    """テストで使う非表示の Tk ルートを提供し、必ず破棄します。"""
    import tkinter as tk

    root = create_tk_window(tk.Tk)
    root.withdraw()
    yield root
    root.destroy()
