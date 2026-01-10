# Assuming refactor_constructors.py is in the same directory
from refactor.rolecore_to_roleargs_2nd import refactor_content, refactor_csharp_files

def _create_file(tmp_path, filename, content):
    """Helper function to create a file in the temporary directory."""
    filepath = tmp_path / filename
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    return filepath

def test_refactor_content_single_occurrence():
    """Tests that a single instance of RoleCore.Build is replaced."""
    original = 'public Teleporter() : base(RoleCore.BuildCrewmate(...))'
    expected = 'public Teleporter() : base(RoleArgs.BuildCrewmate(...))'
    assert refactor_content(original) == expected

def test_refactor_content_no_occurrence():

    """Tests that the content remains unchanged if the target string is not present."""

    original = 'public class MyClass { }'

    assert refactor_content(original) == original

def test_refactor_content_multiple_occurrences():
    """Tests that all instances of RoleCore.Build are replaced."""
    original = '''
    var args1 = RoleCore.BuildImpostor(id);
    // Some comments
    var args2 = RoleCore.BuildCrewmate(id, color);
    '''
    expected = '''
    var args1 = RoleArgs.BuildImpostor(id);
    // Some comments
    var args2 = RoleArgs.BuildCrewmate(id, color);
    '''
    assert refactor_content(original) == expected

def test_refactor_content_empty_string():
    """Tests that an empty string is handled correctly."""
    assert refactor_content("") == ""

def test_modifies_correct_file(tmp_path):
    """Tests that a file with the target string is modified."""
    content = "base(RoleCore.BuildCrewmate())"
    filepath = _create_file(tmp_path, "Target.cs", content)

    refactor_csharp_files(tmp_path)

    with open(filepath, 'r', encoding='utf-8') as f:
        new_content = f.read()

    assert new_content == "base(RoleArgs.BuildCrewmate())"

def test_does_not_modify_unrelated_file(tmp_path):
    """Tests that a file without the target string is not modified."""
    content = "class Unrelated {}"
    filepath = _create_file(tmp_path, "Unrelated.cs", content)

    refactor_csharp_files(tmp_path)

    with open(filepath, 'r', encoding='utf-8') as f:
        new_content = f.read()

    assert new_content == content

def test_skips_excluded_files(tmp_path):
    """Tests that RoleCore.cs and RoleArgs.cs are skipped even if they contain the target."""
    rolecore_content = "public static RoleCore BuildImpostor() => RoleCore.BuildImpostor();"
    roleargs_content = "public static RoleArgs BuildImpostor() => new RoleArgs(RoleCore.BuildImpostor());"

    rolecore_path = _create_file(tmp_path, "RoleCore.cs", rolecore_content)
    roleargs_path = _create_file(tmp_path, "RoleArgs.cs", roleargs_content)

    refactor_csharp_files(tmp_path)

    with open(rolecore_path, 'r', encoding='utf-8') as f:
        assert f.read() == rolecore_content

    with open(roleargs_path, 'r', encoding='utf-8') as f:
        assert f.read() == roleargs_content