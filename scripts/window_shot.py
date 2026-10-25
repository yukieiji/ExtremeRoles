"""Win32 API で指定タイトルの子ウィンドウをキャプチャし PNG で保存します。"""

import ctypes
import struct
import sys
import zlib
from ctypes import wintypes
from pathlib import Path

PW_RENDERFULLCONTENT = 0x00000002
BI_RGB = 0
DIB_RGB_COLORS = 0
BYTES_PER_PIXEL = 4
PNG_SIGNATURE = b"\x89PNG\r\n\x1a\n"


class Rect(ctypes.Structure):
    _fields_ = [
        ("left", wintypes.LONG),
        ("top", wintypes.LONG),
        ("right", wintypes.LONG),
        ("bottom", wintypes.LONG),
    ]


class BitmapInfoHeader(ctypes.Structure):
    _fields_ = [
        ("size", wintypes.DWORD),
        ("width", wintypes.LONG),
        ("height", wintypes.LONG),
        ("planes", wintypes.WORD),
        ("bit_count", wintypes.WORD),
        ("compression", wintypes.DWORD),
        ("image_size", wintypes.DWORD),
        ("x_pixels_per_meter", wintypes.LONG),
        ("y_pixels_per_meter", wintypes.LONG),
        ("colors_used", wintypes.DWORD),
        ("colors_important", wintypes.DWORD),
    ]


def user32() -> ctypes.WinDLL:
    """署名を宣言済みの user32 を返します。"""
    library = ctypes.windll.user32
    library.FindWindowW.restype = wintypes.HWND
    library.FindWindowW.argtypes = [wintypes.LPCWSTR, wintypes.LPCWSTR]
    library.GetWindowRect.restype = wintypes.BOOL
    library.GetWindowRect.argtypes = [wintypes.HWND, ctypes.POINTER(Rect)]
    library.GetWindowDC.restype = wintypes.HDC
    library.GetWindowDC.argtypes = [wintypes.HWND]
    library.ReleaseDC.restype = ctypes.c_int
    library.ReleaseDC.argtypes = [wintypes.HWND, wintypes.HDC]
    library.PrintWindow.restype = wintypes.BOOL
    library.PrintWindow.argtypes = [wintypes.HWND, wintypes.HDC, wintypes.UINT]
    return library


def gdi32() -> ctypes.WinDLL:
    """署名を宣言済みの gdi32 を返します。"""
    library = ctypes.windll.gdi32
    library.CreateCompatibleDC.restype = wintypes.HDC
    library.CreateCompatibleDC.argtypes = [wintypes.HDC]
    library.CreateCompatibleBitmap.restype = wintypes.HBITMAP
    library.CreateCompatibleBitmap.argtypes = [wintypes.HDC, ctypes.c_int, ctypes.c_int]
    library.SelectObject.restype = wintypes.HGDIOBJ
    library.SelectObject.argtypes = [wintypes.HDC, wintypes.HGDIOBJ]
    library.GetDIBits.restype = ctypes.c_int
    library.GetDIBits.argtypes = [
        wintypes.HDC,
        wintypes.HBITMAP,
        wintypes.UINT,
        wintypes.UINT,
        ctypes.c_void_p,
        ctypes.POINTER(BitmapInfoHeader),
        wintypes.UINT,
    ]
    library.DeleteObject.restype = wintypes.BOOL
    library.DeleteObject.argtypes = [wintypes.HGDIOBJ]
    library.DeleteDC.restype = wintypes.BOOL
    library.DeleteDC.argtypes = [wintypes.HDC]
    return library


def find_window(title: str) -> int | None:
    """タイトルが完全一致するウィンドウハンドルを返します。"""
    if sys.platform != "win32":
        return None

    return user32().FindWindowW(None, title) or None


def capture_window(title: str, path: Path) -> tuple[int, int]:
    """タイトル一致のウィンドウを撮って PNG に書き、(幅, 高さ) を返します。"""
    if sys.platform != "win32":
        raise OSError("ウィンドウキャプチャは Windows のみ対応です")

    handle = find_window(title)
    if handle is None:
        raise LookupError(f"ウィンドウが見つかりません: {title}")

    width, height = _window_size(handle)
    rgba = _grab_pixels(handle, width, height)
    path.parent.mkdir(parents=True, exist_ok=True)
    write_png(path, width, height, rgba)
    return width, height


def _window_size(handle: int) -> tuple[int, int]:
    rect = Rect()
    if not user32().GetWindowRect(handle, ctypes.byref(rect)):
        raise OSError("GetWindowRect に失敗しました")

    width = rect.right - rect.left
    height = rect.bottom - rect.top
    if width <= 0 or height <= 0:
        raise OSError(f"ウィンドウが描画可能なサイズではありません: {width}x{height}")
    return width, height


def _grab_pixels(handle: int, width: int, height: int) -> bytes:
    """ウィンドウを描画したメモリビットマップから RGBA を取り出します。"""
    gdi = gdi32()
    window_dc = user32().GetWindowDC(handle)
    if not window_dc:
        raise OSError("GetWindowDC に失敗しました")

    memory_dc = gdi.CreateCompatibleDC(window_dc)
    bitmap = gdi.CreateCompatibleBitmap(window_dc, width, height)
    if not memory_dc or not bitmap:
        gdi.DeleteDC(memory_dc)
        user32().ReleaseDC(handle, window_dc)
        raise OSError("メモリビットマップの作成に失敗しました")

    original = gdi.SelectObject(memory_dc, bitmap)
    try:
        if not user32().PrintWindow(handle, memory_dc, PW_RENDERFULLCONTENT):
            raise OSError("PrintWindow に失敗しました")

        gdi.SelectObject(memory_dc, original)
        return _read_bitmap(memory_dc, bitmap, width, height)
    finally:
        gdi.DeleteObject(bitmap)
        gdi.DeleteDC(memory_dc)
        user32().ReleaseDC(handle, window_dc)


def _read_bitmap(memory_dc: int, bitmap: int, width: int, height: int) -> bytes:
    header = BitmapInfoHeader(
        size=ctypes.sizeof(BitmapInfoHeader),
        width=width,
        height=-height,
        planes=1,
        bit_count=32,
        compression=BI_RGB,
    )
    buffer = ctypes.create_string_buffer(width * height * BYTES_PER_PIXEL)
    if not gdi32().GetDIBits(
        memory_dc, bitmap, 0, height, buffer, ctypes.byref(header), DIB_RGB_COLORS
    ):
        raise OSError("GetDIBits に失敗しました")

    return _to_rgba(buffer.raw, width * height)


def _to_rgba(pixels: bytes, count: int) -> bytes:
    """BGRX を RGBA に並び替えます。"""
    swapped = bytearray(pixels)
    swapped[0::4] = pixels[2::4]
    swapped[2::4] = pixels[0::4]
    swapped[3::4] = b"\xff" * count
    return bytes(swapped)


def write_png(path: Path, width: int, height: int, rgba: bytes) -> None:
    """RGBA バイト列を PNG 1枚として書き出します。"""
    stride = width * BYTES_PER_PIXEL
    raw = b"".join(
        b"\x00" + rgba[row * stride : (row + 1) * stride] for row in range(height)
    )
    path.write_bytes(
        PNG_SIGNATURE
        + _chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0))
        + _chunk(b"IDAT", zlib.compress(raw, 6))
        + _chunk(b"IEND", b"")
    )


def _chunk(kind: bytes, payload: bytes) -> bytes:
    return (
        struct.pack(">I", len(payload))
        + kind
        + payload
        + struct.pack(">I", zlib.crc32(kind + payload) & 0xFFFFFFFF)
    )
