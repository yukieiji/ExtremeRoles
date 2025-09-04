import pytest
import os
import sys
import xml.etree.ElementTree as ET
from pathlib import Path
from typing import Any
from hypothesis import given, strategies as st, settings, HealthCheck
from pytest import MonkeyPatch, CaptureFixture

from add_translation_key import (
    generate_translation_keys, update_resx_file, find_role_file,
    parse_cs_file, main
)

# --- Unit Tests ---

def test_generate_translation_keys() -> None:
    """Tests that the key generation logic is correct for a simple case."""
    class_name = "TestRole"
    option_names: set[str] = {"OptionA", "OptionB"}
    expected_keys = [
        "TestRole", "TestRoleFullDescription", "TestRoleShortDescription",
        "TestRoleIntroDescription", "TestRoleOptionA", "TestRoleOptionB"
    ]
    actual_keys = generate_translation_keys(class_name, option_names)
    assert sorted(actual_keys) == sorted(expected_keys)

# --- Integration Test Fixtures ---

DUMMY_CS_CONTENT_VALID: str = """
namespace ExtremeRoles.Roles.Solo.Crewmate {
    public sealed class DummyRole : SingleRoleBase {
        public enum DummyRoleOption { AnOption }
        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) {
            factory.CreateBoolOption(DummyRoleOption.AnOption, false);
        }
    }
}
"""

DUMMY_RESX_CONTENT: str = """<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
  <data name="ExistingKey" xml:space="preserve"><value>Existing Value</value></data>
</root>
"""

DUMMY_CS_CONTENT_INVALID: str = """
namespace ExtremeRoles.Roles.Solo.Crewmate {
    public sealed class BadRole : SingleRoleBase {
        public enum BadRoleOption { ShouldWork, IsMissing }
        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) {
            factory.CreateBoolOption(BadRoleOption.ShouldWork, true);
        }
    }
}
"""

@pytest.fixture
def valid_env(tmp_path: Path) -> Path:
    """Sets up a valid temporary environment for testing file operations."""
    roles_dir = tmp_path / "ExtremeRoles" / "Roles" / "Solo" / "Crewmate"
    roles_dir.mkdir(parents=True)
    (roles_dir / "DummyRole.cs").write_text(DUMMY_CS_CONTENT_VALID, encoding="utf-8")

    trans_dir = tmp_path / "ExtremeRoles" / "Translation" / "resx"
    trans_dir.mkdir(parents=True)
    (trans_dir / "Crewmate.resx").write_text(DUMMY_RESX_CONTENT, encoding="utf-8")

    return tmp_path

@pytest.fixture
def invalid_env(tmp_path: Path) -> Path:
    """Sets up a temporary environment with a C# file containing a discrepancy."""
    roles_dir = tmp_path / "ExtremeRoles" / "Roles" / "Solo" / "Crewmate"
    roles_dir.mkdir(parents=True)
    (roles_dir / "BadRole.cs").write_text(DUMMY_CS_CONTENT_INVALID, encoding="utf-8")
    return tmp_path

# --- Integration Tests with Mock Data ---

def test_adds_strmiss_placeholder(valid_env: Path, monkeypatch: MonkeyPatch) -> None:
    """Tests that a new key is added with the 'STRMISS' placeholder."""
    monkeypatch.chdir(valid_env)
    monkeypatch.setattr(sys, 'argv', ['add_translation_key.py', 'DummyRole'])

    main()

    resx_path = "ExtremeRoles/Translation/resx/Crewmate.resx"
    tree = ET.parse(resx_path)
    root = tree.getroot()

    # Find one of the newly added keys
    new_key_element = root.find(".//data[@name='DummyRoleAnOption']")
    assert new_key_element is not None

    # Check that its value is STRMISS
    value_element = new_key_element.find("value")
    assert value_element is not None
    assert value_element.text == "STRMISS"


def test_detects_unimplemented_option_error(invalid_env: Path, monkeypatch: MonkeyPatch, capsys: CaptureFixture[str]) -> None:
    """Tests that the script detects an unimplemented option and exits with an error."""
    monkeypatch.chdir(invalid_env)
    monkeypatch.setattr(sys, 'argv', ['add_translation_key.py', 'BadRole'])
    with pytest.raises(SystemExit) as e:
        main()
    assert e.value.code == 1
    captured = capsys.readouterr()
    assert "エラー: 定義と実装の間に矛盾が発見されました。" in captured.err
    assert "IsMissing" in captured.err

# --- Hypothesis Strategies ---

cs_identifier: st.SearchStrategy[str] = st.text(
    alphabet=st.characters(min_codepoint=97, max_codepoint=122),
    min_size=3, max_size=10
).map(lambda s: s.capitalize())

@st.composite
def csharp_role_strategy(draw: st.DrawFn) -> tuple[str, set[str], set[str]]:
    """A Hypothesis strategy that generates random C# role file content."""
    class_name = draw(cs_identifier)
    defined_options = draw(st.sets(cs_identifier, min_size=0, max_size=10))

    if not defined_options:
        implemented_options = set()
    else:
        implemented_options = draw(
            st.lists(st.sampled_from(sorted(list(defined_options))), unique=True).map(set)
        )

    enum_options_str = ",\n            ".join(sorted(list(defined_options)))
    enum_str = f"public enum {class_name}Option {{ {enum_options_str} }}" if defined_options else ""

    factory_calls = []
    for option in sorted(list(implemented_options)):
        if len(factory_calls) % 2 == 0:
            factory_calls.append(f"factory.CreateBoolOption({class_name}Option.{option}, false);")
        else:
            factory_calls.append(f"factory.CreateSelectionOption<{class_name}Option, Another>({class_name}Option.{option}, someVar);")

    factory_calls_str = "\n            ".join(factory_calls)
    factory_str = f"""
        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
        {{
            {factory_calls_str}
        }}""" if implemented_options else ""

    full_code = f"""
    namespace ExtremeRoles.Roles.Solo.Crewmate
    {{
        public sealed class {class_name} : SingleRoleBase
        {{
            {enum_str}
            {factory_str}
        }}
    }}
    """
    return (full_code, defined_options, implemented_options)

# --- Property-Based Test ---

@given(role_data=csharp_role_strategy())
@settings(
    max_examples=50,
    suppress_health_check=[HealthCheck.function_scoped_fixture]
)
def test_parser_properties(tmp_path: Path, role_data: tuple[str, set[str], set[str]]) -> None:
    """Tests the property that the C# parser works correctly for generated code."""
    generated_code, expected_defined, expected_implemented = role_data
    temp_cs_file = tmp_path / "temp_role.cs"
    temp_cs_file.write_text(generated_code, encoding="utf-8")
    _, actual_defined, actual_implemented = parse_cs_file(str(temp_cs_file))
    assert actual_defined == expected_defined
    assert actual_implemented == expected_implemented
