"""window_shot.py の PNG 書き出しとキャプチャ開始部分のテスト。"""

import struct
import sys

import pytest

from window_shot import PNG_SIGNATURE, _to_rgba, capture_window, write_png


def test_to_rgba_swaps_blue_and_orders_alpha():
    """BGRX 入力が RGBA に並び替わる事のテスト。"""
    # Act
    rgba = _to_rgba(b"\x11\x22\x33\x00\x44\x55\x66\x00", 2)

    # Assert
    assert rgba == b"\x33\x22\x11\xff\x66\x55\x44\xff"


def test_write_png_writes_signature_and_header(tmp_path):
    """PNG 署名と IHDR に正しい寸法が書かれることのテスト。"""
    # Arrange
    path = tmp_path / "shot.png"
    rgba = b"\x00\x00\x00\xff" * 4

    # Act
    write_png(path, 2, 2, rgba)
    payload = path.read_bytes()

    # Assert
    assert payload.startswith(PNG_SIGNATURE)
    assert payload[12:16] == b"IHDR"
    width, height, depth, kind = struct.unpack(">IIBB", payload[16:26])
    assert (width, height, depth, kind) == (2, 2, 8, 6)


@pytest.mark.skipif(sys.platform != "win32", reason="Windows のみの機能のため")
def test_capture_window_without_window(tmp_path):
    """存在しないタイトルでは LookupError になることのテスト。"""
    # Arrange
    path = tmp_path / "存在しない" / "x.png"

    # Act
    def capture() -> None:
        capture_window("このウィンドウは存在しません", path)

    # Assert
    with pytest.raises(LookupError):
        capture()
