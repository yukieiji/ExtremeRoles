import pytest
import os
import sys
import re
import enum
import shutil
from pathlib import Path
from hypothesis import given, strategies as st, settings, HealthCheck
from pytest import MonkeyPatch, CaptureFixture

from add_role_translation_keys import (
    generate_translation_keys,
    parse_options_from_class_body,
    main,
    ParsedRoleData,
    ParsedOptionsData
)

# --- Dynamically load ExtremeRoleId from C# source ---

def get_extreme_role_ids() -> enum.Enum:
    """C#のソースファイルからExtremeRoleIdのenum値を読み込んでPythonのEnumを生成します。

    Raises:
        FileNotFoundError: C#ソースファイルが見つからない場合。
        ValueError: enum定義が見つからない場合。

    Returns:
        動的に生成されたExtremeRoleIdのEnum。
    """
    cs_file_path = Path("ExtremeRoles/Roles/ExtremeRoleManager.cs")
    if not cs_file_path.exists():
        raise FileNotFoundError(f"C# source file not found at {cs_file_path}")

    cs_content = cs_file_path.read_text(encoding="utf-8")

    match = re.search(r"public enum ExtremeRoleId\s*:\s*int\s*\{([^}]+)\}", cs_content, re.DOTALL)
    if not match:
        raise ValueError("ExtremeRoleId enum definition not found in C# file.")

    enum_body = match.group(1)

    role_names = [name for name in re.findall(r"^\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*,?", enum_body, re.MULTILINE) if name not in ["Null", "VanillaRole"]]

    return enum.Enum("ExtremeRoleId", {name: name for name in role_names})

try:
    ExtremeRoleId = get_extreme_role_ids()
    extreme_role_id_strategy = st.sampled_from(list(ExtremeRoleId))
except (FileNotFoundError, ValueError) as e:
    print(f"Skipping property-based tests for roles: {e}", file=sys.stderr)
    # ストラテジーをNoneに設定し、テストをスキップできるようにする
    extreme_role_id_strategy = None

# --- Unit Tests ---

def test_generate_translation_keys() -> None:
    """単純なケースに対してキー生成ロジックが正しいことをテストします。"""
    class_name = "TestRole"
    option_names: set[str] = {"OptionA", "OptionB"}
    expected_keys: list[str] = [
        "TestRole", "TestRoleFullDescription", "TestRoleShortDescription",
        "TestRoleIntroDescription", "TestRoleOptionA", "TestRoleOptionB"
    ]
    actual_keys = generate_translation_keys(class_name, option_names)
    assert sorted(actual_keys) == sorted(expected_keys)

# --- Integration Test Fixtures ---

DUMMY_CS_VALID: str = """
public class DummyRole {
    public enum Option { AnOption }
    protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) {
        factory.CreateBoolOption(Option.AnOption, false);
    }
}
"""

DUMMY_CS_INVALID: str = """
public class InvalidRole {
    public enum Option { Defined, AlsoDefined }
    protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) {
        factory.CreateBoolOption(Option.Defined, false);
    }
}
"""

@pytest.fixture
def valid_env(tmp_path: Path) -> Path:
    """テスト用の有効な一時環境をセットアップします。

    Args:
        tmp_path: pytestが提供する一時的なパスオブジェクト。

    Returns:
        セットアップされた一時環境へのパス。
    """
    roles_dir: Path = tmp_path / "ExtremeRoles" / "Roles" / "Solo" / "Crewmate"
    roles_dir.mkdir(parents=True)
    (roles_dir / "DummyRole.cs").write_text(DUMMY_CS_VALID, encoding="utf-8")

    trans_dir: Path = tmp_path / "ExtremeRoles" / "Translation" / "resx"
    trans_dir.mkdir(parents=True)

    return tmp_path

@pytest.fixture
def role_test_env(tmp_path: Path) -> Path:
    """プロパティベースの役職テスト用の一次環境をセットアップします。

    - 一時的なディレクトリ構造を作成します。
    - 実際の.resxファイルを一時ディレクトリにコピーします。

    Args:
        tmp_path: pytestが提供する一時的なパスオブジェクト。

    Returns:
        セットアップされた一時環境へのパス。
    """
    project_root = Path.cwd()
    original_resx_dir = project_root / "ExtremeRoles" / "Translation" / "resx"

    temp_roles_dir = tmp_path / "ExtremeRoles" / "Roles"
    temp_roles_dir.mkdir(parents=True)

    temp_trans_dir = tmp_path / "ExtremeRoles" / "Translation" / "resx"

    if original_resx_dir.exists() and original_resx_dir.is_dir():
        shutil.copytree(original_resx_dir, temp_trans_dir)
    else:
        temp_trans_dir.mkdir(parents=True)

    return tmp_path

# --- Integration Tests ---

def test_main_logic_with_valid_role(valid_env: Path, monkeypatch: MonkeyPatch, capsys: CaptureFixture[str]) -> None:
    """メインスクリプトのロジックが有効なロールを見つけて処理することをテストします。

    Args:
        valid_env: テスト用の有効な環境を提供するフィクスチャ。
        monkeypatch: pytestのモンキーパッチフィクスチャ。
        capsys: pytestのキャプチャフィクスチャ。
    """
    monkeypatch.chdir(valid_env)
    monkeypatch.setattr(sys, 'argv', ['add_role_translation_keys.py', 'DummyRole'])
    main()
    captured = capsys.readouterr()
    assert "役職クラス 'DummyRole' をファイル内で発見" in captured.out
    assert "定義済みのオプション 1個、実装済みのオプション 1個を発見しました。" in captured.out

def test_main_handles_discrepancy(valid_env: Path, monkeypatch: MonkeyPatch, capsys: CaptureFixture[str]) -> None:
    """メインスクリプトが矛盾を正しく報告して終了することをテストします。

    Args:
        valid_env: テスト用の有効な環境を提供するフィクスチャ。
        monkeypatch: pytestのモンキーパッチフィクスチャ。
        capsys: pytestのキャプチャフィクスチャ。
    """
    (valid_env / "ExtremeRoles/Roles/Solo/Crewmate/InvalidRole.cs").write_text(DUMMY_CS_INVALID)
    monkeypatch.chdir(valid_env)
    monkeypatch.setattr(sys, 'argv', ['add_role_translation_keys.py', 'InvalidRole'])

    with pytest.raises(SystemExit) as e:
        main()

    assert e.value.code == 1
    captured = capsys.readouterr()
    assert "エラー: 定義と実装の間に矛盾が発見されました。" in captured.err
    assert "AlsoDefined" in captured.err

# --- Hypothesis Strategies ---

cs_identifier: st.SearchStrategy[str] = st.text(alphabet=st.characters(min_codepoint=97, max_codepoint=122), min_size=3, max_size=10).map(lambda s: s.capitalize())

@st.composite
def csharp_class_body_strategy(draw: st.DrawFn) -> tuple[str, str, set[str], set[str]]:
    """ランダムなC#クラス本体のコンテンツを生成するHypothesisストラテジ。

    Args:
        draw: HypothesisのDrawFnオブジェクト。

    Returns:
        クラス本体、クラス名、定義済みオプション、実装済みオプションのタプル。
    """
    class_name = draw(cs_identifier)
    defined_options = draw(st.sets(cs_identifier, min_size=0, max_size=10))

    if defined_options:
        implemented_options = draw(st.lists(st.sampled_from(sorted(list(defined_options))), unique=True).map(set))
        enum_options_str = ",\n            ".join(sorted(list(defined_options)))
        enum_str = f"public enum Option {{ {enum_options_str} }}"
    else:
        implemented_options = set()
        enum_str = ""

    if implemented_options:
        factory_calls = [f"factory.CreateBoolOption(Option.{option}, false);" for option in sorted(list(implemented_options))]
        factory_calls_str = "\n            ".join(factory_calls)
        factory_str = f"protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) {{ {factory_calls_str} }}"
    else:
        factory_str = ""

    full_body = f"{enum_str}\n{factory_str}"
    return (full_body, class_name, defined_options, implemented_options)

# --- Property-Based Test ---

@given(data=csharp_class_body_strategy())
@settings(max_examples=50)
def test_parser_options_properties(data: tuple[str, str, set[str], set[str]]) -> None:
    """オプションパーサーがジェネレーターと一致しているというプロパティをテストします。

    Args:
        data: csharp_class_body_strategyによって生成されたデータ。
    """
    class_body, class_name, expected_defined, expected_implemented = data

    result = parse_options_from_class_body(class_body, class_name)

    assert result.defined == expected_defined
    assert result.implemented == expected_implemented

# --- Property-Based Test for Real Roles ---

def find_role_file(role_name: str) -> Path | None:
    """指定された役職名のC#ソースファイルを検索します。

    Args:
        role_name: 検索する役職名。

    Returns:
        見つかったファイルのPathオブジェクト、またはNone。
    """
    roles_root = Path("ExtremeRoles/Roles")
    for filepath in roles_root.glob(f"**/{role_name}.cs"):
        return filepath
    return None

@pytest.mark.skipif(extreme_role_id_strategy is None, reason="Could not parse ExtremeRoleId from C# source")
@given(role_id=extreme_role_id_strategy)
@settings(suppress_health_check=[HealthCheck.function_scoped_fixture], deadline=None, max_examples=20)
def test_add_translation_key_for_random_roles(role_test_env: Path, monkeypatch: MonkeyPatch, capsys: CaptureFixture[str], role_id: enum.Enum) -> None:
    """ランダムに選択された実際の役職に対して、翻訳キーの追加が正しく行われることをテストします。"""
    role_name = role_id.name

    original_role_file = find_role_file(role_name)
    if not original_role_file:
        pytest.skip(f"Source file for role '{role_name}' not found.")
        return

    # 一時環境に役職のソースファイルをコピー
    relative_path = original_role_file.relative_to(Path.cwd())
    temp_role_file = role_test_env / relative_path
    temp_role_file.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy(original_role_file, temp_role_file)

    # スクリプトを実行
    monkeypatch.chdir(role_test_env)
    monkeypatch.setattr(sys, 'argv', ['add_role_translation_keys.py', role_name])

    try:
        main()
    except SystemExit as e:
        # 正常終了(exit(0))は問題ない
        assert e.code == 0, f"Script exited with non-zero code {e.code} for role {role_name}"

    # --- Verification ---
    captured = capsys.readouterr()
    role_file_content = temp_role_file.read_text(encoding="utf-8")
    parsed_data = parse_options_from_class_body(role_file_content, role_name)

    # スクリプトが矛盾を報告した場合、キーは追加されないはず
    if "エラー: 定義と実装の間に矛盾が発見されました。" in captured.err:
        expected_options = set()
    else:
        expected_options = parsed_data.defined

    expected_keys = generate_translation_keys(role_name, expected_options)

    if not expected_keys:
        return # 検証することがない

    # すべての.resxファイルにキーが追加されたか確認
    resx_dir = role_test_env / "ExtremeRoles" / "Translation" / "resx"

    # 少なくとも1つのresxファイルが存在することを確認
    resx_files = list(resx_dir.glob("*.resx"))
    assert resx_files, "No .resx files found in the temporary directory for verification."

    for resx_file in resx_files:
        content = resx_file.read_text(encoding="utf-8")
        for key in expected_keys:
            assert f'<data name="{key}"' in content, \
                f"Key '{key}' not found in {resx_file.name} for role '{role_name}'"
