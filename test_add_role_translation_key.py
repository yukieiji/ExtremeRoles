import pytest
import os
import sys
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
