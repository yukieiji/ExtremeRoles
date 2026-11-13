"""マトリクス描画で使う矩形の単純な型。"""

from dataclasses import dataclass


@dataclass(frozen=True)
class Box:
    """矩形領域。"""

    left: int
    top: int
    right: int
    bottom: int

    @property
    def width(self) -> int:
        return self.right - self.left

    @property
    def height(self) -> int:
        return self.bottom - self.top

    def contains(self, x: int, y: int) -> bool:
        """座標が矩形の内側かどうかを返します (右端・下端は含まない)。"""
        return self.left <= x < self.right and self.top <= y < self.bottom
