
import unittest
from hypothesis import given, strategies as st
import string

from refactoring.refactor_constructors import refactor_content

class TestConstructorRefactoring(unittest.TestCase):

    def test_single_role_base_example(self):
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
        self.assertEqual(refactor_content(code_before), code_after)

    def test_multi_assign_role_base_example(self):
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
        self.assertEqual(refactor_content(code_before), code_after)

    def test_no_change_on_unrelated_method(self):
        """Tests that an unrelated method call is not changed."""
        code = """
public SomeOtherRole() : base(
    SomeBuilder.Build(
        arg1,
        arg2,
        RolePropPresets.OptionalDefault)
) {}
"""
        self.assertEqual(refactor_content(code), code)

    @given(
        ws1=st.text(string.whitespace),
        ws2=st.text(string.whitespace),
        ws3=st.text(string.whitespace),
    )
    def test_property_based_crewmate(self, ws1, ws2, ws3):
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
        self.assertEqual(refactor_content(code_before), code_after)

    @given(
        ws1=st.text(string.whitespace),
        ws2=st.text(string.whitespace),
    )
    def test_property_based_impostor(self, ws1, ws2):
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
        self.assertEqual(refactor_content(code_before), code_after)

if __name__ == '__main__':
    unittest.main()
