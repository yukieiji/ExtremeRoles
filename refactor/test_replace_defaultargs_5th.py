
from hypothesis import given, strategies as st
import string

from refactor.replace_defaultargs_5th import refactor_content

def test_single_role_base_example():
    """Tests the specific SingleRoleBase example with BuildCrewmate."""
    code_before = """
// SingleRoleBase
public Teleporter() : base(
RoleArgs.BuildCrewmate(
    ExtremeRoleId.Teleporter,
    ColorPalette.TeleporterCherry,
    RolePropPresets.CrewmateDefault)
) { }
"""
    code_after = """
// SingleRoleBase
public Teleporter() : base(
RoleArgs.BuildCrewmate(
    ExtremeRoleId.Teleporter,
    ColorPalette.TeleporterCherry)
) { }
"""
    assert refactor_content(code_before) == code_after

def test_multi_assign_role_base_example():
    """Tests the specific MultiAssignRoleBase example with BuildImpostor."""
    code_before = """
// MultiAssignRoleBase
public Assassin() : base(
    RoleArgs.BuildImpostor(ExtremeRoleId.Assassin, RolePropPresets.ImpostorDefault),
        tab: OptionTab.CombinationTab)
{
}
"""
    code_after = """
// MultiAssignRoleBase
public Assassin() : base(
    RoleArgs.BuildImpostor(ExtremeRoleId.Assassin),
        tab: OptionTab.CombinationTab)
{
}
"""
    assert refactor_content(code_before) == code_after

def test_no_change_on_unrelated_method():
    """Tests that an unrelated method call is not changed."""
    code = """
public SomeOtherRole() : base(
SomeBuilder.Build(
    arg1,
    arg2,
    RolePropPresets.OptionalDefault)
) {}
"""
    assert refactor_content(code) == code

@given(
    ws1=st.text(string.whitespace),
    ws2=st.text(string.whitespace),
    ws3=st.text(string.whitespace),
)
def test_property_based_crewmate(ws1, ws2, ws3):
    """
    Tests RoleArgs.BuildCrewmate refactoring with varied whitespace.
    """
    code_before = f"""
public SomeRole() : base(
RoleArgs.BuildCrewmate({ws1}
    SomeId,{ws2}
    SomeColor,
    RolePropPresets.CrewmateDefault{ws3})
) {{}}
"""
    code_after = f"""
public SomeRole() : base(
RoleArgs.BuildCrewmate({ws1}
    SomeId,{ws2}
    SomeColor)
) {{}}
"""
    assert refactor_content(code_before) == code_after

@given(
    ws1=st.text(string.whitespace),
    ws2=st.text(string.whitespace),
)
def test_property_based_impostor(ws1, ws2):
    """
    Tests RoleArgs.BuildImpostor refactoring with varied whitespace.
    """
    code_before = f"""
public SomeImpostorRole() : base(
RoleArgs.BuildImpostor({ws1}SomeId, RolePropPresets.ImpostorDefault{ws2}),
some_other_arg: true
) {{}}
"""
    code_after = f"""
public SomeImpostorRole() : base(
RoleArgs.BuildImpostor({ws1}SomeId),
some_other_arg: true
) {{}}
"""
    assert refactor_content(code_before) == code_after


