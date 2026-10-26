"""スクリプト共通のリポジトリパスを提供します。"""

from pathlib import Path

REPO_ROOT: Path = Path(__file__).resolve().parents[2]
