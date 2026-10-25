"""ExtremeRoles/Module/ColorPalette.cs を解析してカラーパレットを読み取ります。"""

import re
import sys
from dataclasses import dataclass, replace
from pathlib import Path

from .color_space import Rgb
from .paths import REPO_ROOT

COLORPALETTE_CS = REPO_ROOT / "ExtremeRoles" / "Module" / "ColorPalette.cs"

_MODIFIERS = r"(?:readonly\s+static|static\s+readonly)"
_DECLARATION = re.compile(
    rf"^\s*public\s+{_MODIFIERS}\s+Color\s+(?P<name>\w+)\s*=\s*(?P<value>[^;]+);",
    re.MULTILINE,
)
_LINE_COMMENT = re.compile(r"^\s*//.*$", re.MULTILINE)
_CALL = re.compile(r"new\s+(?P<kind>Color32|Color)\s*\((?P<args>[^)]*)\)")
_UNITY_COLOR = re.compile(r"^Color\.(?P<name>\w+)$")
_ALIAS = re.compile(r"^[A-Za-z_]\w*$")
_ARGUMENT = re.compile(
    r"(?:0[xX][0-9a-fA-F]+|Byte\.MaxValue|byte\.MaxValue|\d+(?:\.\d+)?f?)"
)

_UNITY_NAMED_COLORS: dict[str, tuple[int, int, int]] = {
    "white": (255, 255, 255),
    "black": (0, 0, 0),
    "red": (255, 0, 0),
    "cyan": (0, 255, 255),
    "blue": (0, 0, 255),
    "green": (0, 255, 0),
    "yellow": (255, 255, 0),
    "magenta": (255, 0, 255),
    "gray": (128, 128, 128),
    "grey": (128, 128, 128),
}


@dataclass(frozen=True)
class PaletteColor:
    """カラーパレットの1項目。"""

    name: str
    rgb: Rgb
    line: int
    index: int
    alias_of: str | None = None


def parse_source(source: str) -> tuple[list[PaletteColor], list[str]]:
    """ColorPalette.cs のソースからパレット項目を解析します。

    Returns:
        (宣言順の PaletteColor リスト, 解釈できなかった宣言の警告リスト)。
    """
    colors: list[PaletteColor] = []
    warnings: list[str] = []

    for number, match in enumerate(
        _DECLARATION.finditer(_LINE_COMMENT.sub("", source)), 1
    ):
        line = source.count("\n", 0, match.start()) + 1
        name = match.group("name")
        value = " ".join(match.group("value").split())
        parsed = _parse_value(value)
        if isinstance(parsed, str):
            colors.append(
                PaletteColor(
                    name=name,
                    rgb=Rgb(0, 0, 0),
                    line=line,
                    index=number,
                    alias_of=parsed,
                )
            )
            continue
        if parsed is None:
            warnings.append(f"{name} (line {line}): 解釈できない宣言 '{value}'")
            continue
        try:
            colors.append(PaletteColor(name=name, rgb=parsed, line=line, index=number))
        except ValueError as error:
            warnings.append(f"{name} (line {line}): {error}")

    resolved = _resolve_aliases(colors, warnings)
    return [
        replace(color, index=number) for number, color in enumerate(resolved, 1)
    ], warnings


def load_palette(file_path: Path) -> list[PaletteColor]:
    """ColorPalette.cs を読み込んでパレット項目を返します。解釈失敗は stderr に出します。"""
    colors, warnings = parse_source(file_path.read_text(encoding="utf-8"))
    for warning in warnings:
        print(f"警告: {warning}", file=sys.stderr)
    return colors


def _parse_value(value: str) -> Rgb | str | None:
    call = _CALL.search(value)
    if call:
        return _parse_constructor(call)

    unity_color = _UNITY_COLOR.match(value)
    if unity_color:
        named = _UNITY_NAMED_COLORS.get(unity_color.group("name").lower())
        return None if named is None else Rgb(*named)

    if _ALIAS.match(value):
        return value
    return None


def _parse_constructor(call: re.Match[str]) -> Rgb | None:
    channels = [_to_float(token) for token in _ARGUMENT.findall(call.group("args"))]
    if len(channels) < 3 or any(channel is None for channel in channels[:3]):
        return None

    red, green, blue = channels[0], channels[1], channels[2]
    if call.group("kind") == "Color32":
        return _rgb_or_none(red, green, blue)

    if max(red, green, blue) > 1.0 or min(red, green, blue) < 0.0:
        return None
    return _rgb_or_none(red * 255.0, green * 255.0, blue * 255.0)


def _to_float(token: str) -> float | None:
    if token.lower().endswith("maxvalue"):
        return 255.0

    normalized = token[:-1] if token.endswith("f") else token
    try:
        if normalized[:2].lower() == "0x":
            return float(int(normalized, 16))
        return float(normalized)
    except ValueError:
        return None


def _rgb_or_none(red: float, green: float, blue: float) -> Rgb | None:
    channels = (round(red), round(green), round(blue))
    if any(channel < 0 or channel > 255 for channel in channels):
        return None
    return Rgb(*channels)


def _resolve_aliases(
    colors: list[PaletteColor], warnings: list[str]
) -> list[PaletteColor]:
    by_name = {color.name: color for color in colors}
    resolved: list[PaletteColor] = []
    for color in colors:
        if color.alias_of is None:
            resolved.append(color)
            continue

        original = by_name.get(color.alias_of)
        if original is None or original.alias_of is not None:
            warnings.append(
                f"{color.name} (line {color.line}): "
                f"解決できないエイリアス参照 '{color.alias_of}'"
            )
            continue

        resolved.append(
            PaletteColor(
                name=color.name,
                rgb=original.rgb,
                line=color.line,
                index=color.index,
                alias_of=color.alias_of,
            )
        )
    return resolved
