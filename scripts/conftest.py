"""scripts 配下のテストがスクリプト本体を import できるようにします。"""

import sys
from collections.abc import Iterator
from pathlib import Path

import pytest

SCRIPTS_DIR = Path(__file__).resolve().parent
TK_CREATE_ATTEMPTS = 3

if str(SCRIPTS_DIR) not in sys.path:
    sys.path.insert(0, str(SCRIPTS_DIR))


def _create_tk_root():
    """Tcl 初期化が一時的に失敗する環境がある為、作成を繰り返して返します。"""
    import tkinter as tk

    last_error: Exception = RuntimeError("Tk を 1 度も生成できていません")
    for _ in range(TK_CREATE_ATTEMPTS):
        try:
            return tk.Tk()
        except (ImportError, tk.TclError) as error:
            last_error = error

    pytest.skip(f"Tk を初期化できないため: {last_error}")


@pytest.fixture()
def tk_root() -> Iterator[object]:
    """テストで使う非表示の Tk ルートを提供し、必ず破棄します。"""
    root = _create_tk_root()
    root.withdraw()
    yield root
    root.destroy()
