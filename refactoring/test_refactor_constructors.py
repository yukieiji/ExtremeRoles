
import unittest
from hypothesis import given, strategies as st
import string

# Assuming the refactor_constructors.py is in the same directory
from refactor_constructors import refactor_content

class TestConstructorRefactoring(unittest.TestCase):

    def test_single_role_base_example(self):
        """
        Tests the specific SingleRoleBase example provided by the user.
        """
        code_before = """
// SingleRoleBase
public Teleporter() : base(
    RoleArgs.BuildCrewmate(
        ExtremeRoleId.Teleporter,
        ColorPalette.TeleporterCherry,
        RolePropPresets.CrewmateDefault)
{ }
"""
        code_after = """
// SingleRoleBase
public Teleporter() : base(
    RoleArgs.BuildCrewmate(
        ExtremeRoleId.Teleporter,
        ColorPalette.TeleporterCherry)
{ }
"""
        self.assertEqual(refactor_content(code_before), code_after)

    def test_multi_assign_role_base_example(self):
        """
        Tests the specific MultiAssignRoleBase example provided by the user.
        """
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

    def test_no_change_if_not_present(self):
        """
        Tests that the content remains unchanged if the target arguments are not present.
        """
        code = """
public class SomeClass() : base(
    SomeOtherClass.SomeMethod(arg1, arg2)
) {}
"""
        self.assertEqual(refactor_content(code), code)

    @given(
        ws1=st.text(string.whitespace),
        ws2=st.text(string.whitespace),
        ws3=st.text(string.whitespace),
    )
    def test_property_based_crewmate_refactoring(self, ws1, ws2, ws3):
        """
        Tests Crewmate refactoring with varied whitespace using Hypothesis.
        """
        code_before = f"""
public SomeRole() : base(
    RoleArgs.BuildCrewmate(
        SomeId,{ws1}
        SomeColor,{ws2}
        RolePropPresets.CrewmateDefault{ws3})
) {{}}
"""
        code_after = f"""
public SomeRole() : base(
    RoleArgs.BuildCrewmate(
        SomeId,{ws1}
        SomeColor)
) {{}}
"""
        self.assertEqual(refactor_content(code_before), code_after)

    @given(
        ws1=st.text(string.whitespace),
        ws2=st.text(string.whitespace),
        ws3=st.text(string.whitespace),
    )
    def test_property_based_impostor_refactoring(self, ws1, ws2, ws3):
        """
        Tests Impostor refactoring with varied whitespace using Hypothesis.
        """
        code_before = f"""
public SomeImpostorRole() : base(
    RoleArgs.BuildImpostor(SomeId,{ws1}RolePropPresets.ImpostorDefault{ws2}),{ws3}
    some_other_arg: true
) {{}}
"""
        code_after = f"""
public SomeImpostorRole() : base(
    RoleArgs.BuildImpostor(SomeId),{ws3}
    some_other_arg: true
) {{}}
"""
        self.assertEqual(refactor_content(code_before), code_after)

if __name__ == '__main__':
    unittest.main()
