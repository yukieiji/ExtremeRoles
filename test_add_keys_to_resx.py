import pytest
import shutil
import re
from pathlib import Path

# テスト対象の関数をインポート
from add_keys_to_resx import add_keys_to_resx_text_based

# テスト対象とする実際の.resxファイル
# Note: これらのファイルがリポジトリに存在することを前提としています
REAL_RESX_FILES = [
    "ExtremeRoles/Translation/resx/Crewmate.resx",
    "ExtremeRoles/Translation/resx/Text.resx",
    "ExtremeRoles/Translation/resx/Impostor.resx",
]

@pytest.fixture(params=REAL_RESX_FILES)
def real_resx_file_copy(tmp_path: Path, request) -> Path:
    """
    pytestフィクスチャ。
    実際の.resxファイルを一時ディレクトリにコピーし、そのパスを返す。
    テストはこのコピーに対して行われる。
    """
    original_path = Path(request.param)
    if not original_path.exists():
        pytest.skip(f"テスト用の.resxファイルが見つかりません: {original_path}")

    temp_file_path = tmp_path / original_path.name
    shutil.copy(original_path, temp_file_path)
    return temp_file_path

def test_add_single_new_key(real_resx_file_copy: Path):
    """単一の新しいキーを追加するテスト。"""
    # ユニークで存在しないであろうキー
    new_key = "Test.Key.Single.12345"
    add_keys_to_resx_text_based(str(real_resx_file_copy), [new_key])

    content = real_resx_file_copy.read_text(encoding="utf-8")

    assert f'name="{new_key}"' in content
    assert f"<value>{new_key}</value>" in content

def test_add_multiple_new_keys(real_resx_file_copy: Path):
    """複数の新しいキーを追加するテスト。"""
    keys_to_add = ["Test.Key.Multiple.1", "Test.Key.Multiple.2"]
    add_keys_to_resx_text_based(str(real_resx_file_copy), keys_to_add)

    content = real_resx_file_copy.read_text(encoding="utf-8")

    assert 'name="Test.Key.Multiple.1"' in content
    assert 'name="Test.Key.Multiple.2"' in content

def test_skip_existing_key(real_resx_file_copy: Path):
    """既に存在するキーを追加しようとした場合、スキップされることをテスト。"""
    content = real_resx_file_copy.read_text(encoding="utf-8")

    # ファイルから既存のキーを1つ見つける
    match = re.search(r'<data name="([^"]+)"', content)
    if not match:
        pytest.skip("テスト対象の.resxファイルにdata要素が見つかりません。")

    existing_key = match.group(1)

    original_content = real_resx_file_copy.read_text(encoding="utf-8")

    # 見つけた既存のキーのみを追加しようとする
    add_keys_to_resx_text_based(str(real_resx_file_copy), [existing_key])

    new_content = real_resx_file_copy.read_text(encoding="utf-8")

    # 内容が変更されていないことを確認
    assert original_content == new_content

def test_add_mixed_keys(real_resx_file_copy: Path):
    """新規キーと既存キーを混合して追加するテスト。"""
    content = real_resx_file_copy.read_text(encoding="utf-8")

    # ファイルから既存のキーを1つ見つける
    match = re.search(r'<data name="([^"]+)"', content)
    if not match:
        pytest.skip("テスト対象の.resxファイルにdata要素が見つかりません。")

    existing_key = match.group(1)
    new_key = "Test.Key.Mixed.New"

    keys_to_add = [new_key, existing_key]
    add_keys_to_resx_text_based(str(real_resx_file_copy), keys_to_add)

    new_content = real_resx_file_copy.read_text(encoding="utf-8")

    # 新規キーは追加されている
    assert f'name="{new_key}"' in new_content
    # 既存キーは重複して追加されていない (countで確認)
    assert new_content.count(f'name="{existing_key}"') == 1

def test_preserve_comments_and_structure(real_resx_file_copy: Path):
    """キー追加後もコメントや構造が保持されることをテスト。"""
    original_content = real_resx_file_copy.read_text(encoding="utf-8")

    add_keys_to_resx_text_based(str(real_resx_file_copy), ["Test.Key.Structure"])

    new_content = real_resx_file_copy.read_text(encoding="utf-8")

    # コメントやスキーマが保持されているか（簡易チェック）
    assert ('<!--' not in original_content) or ('<!--' in new_content)
    assert ('<xsd:schema' not in original_content) or ('<xsd:schema' in new_content)

    # 新しいキーが正しい場所に追加されているか
    assert 'name="Test.Key.Structure"' in new_content
    assert new_content.endswith("</root>")
