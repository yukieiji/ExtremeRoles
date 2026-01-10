
import unittest
from hypothesis import given, strategies as st
import string

from refactoring.refactor_constructors import refactor_content

class TestConstructorRefactoring(unittest.TestCase):

    def test_single_role_base_example(self):
        """Tests the specific SingleRoleBase example with CrewmateDefault."""
        code_before = """
public Teleporter() : base(
    RoleArgs.BuildCrewmate(
        ExtremeRoleId.Teleporter,
        ColorPalette.TeleporterCherry,
        RolePropPresets.CrewmateDefault)
) { }
"""
        code_after = """
public Teleporter() : base(
    RoleArgs.BuildCrewmate(
        ExtremeRoleId.Teleporter,
        ColorPalette.TeleporterCherry)
) { }
"""
        self.assertEqual(refactor_content(code_before), code_after)

    def test_multi_assign_role_base_example(self):
        """Tests the specific MultiAssignRoleBase example with ImpostorDefault."""
        code_before = """
public Assassin() : base(
    RoleArgs.BuildImpostor(ExtremeRoleId.Assassin, RolePropPresets.ImpostorDefault),
    tab: OptionTab.CombinationTab)
{ }
"""
        code_after = """
public Assassin() : base(
    RoleArgs.BuildImpostor(ExtremeRoleId.Assassin),
    tab: OptionTab.CombinationTab)
{ }
"""
        self.assertEqual(refactor_content(code_before), code_after)

    def test_other_default_preset_example(self):
        """Tests that another preset like OptionalDefault is also removed."""
        code_before = """
public SomeOtherRole() : base(
    SomeBuilder.Build(
        arg1,
        arg2,
        RolePropPresets.OptionalDefault)
) {}
"""
        code_after = """
public SomeOtherRole() : base(
    SomeBuilder.Build(
        arg1,
        arg2)
) {}
"""
        self.assertEqual(refactor_content(code_before), code_after)


    def test_no_change_if_not_present(self):
        """Tests that content remains unchanged if no Default presets are present."""
        code = """
public class SomeClass() : base(
    SomeOtherClass.SomeMethod(arg1, arg2)
) {}
"""
        self.assertEqual(refactor_content(code), code)

    @given(
        preset_name=st.text(alphabet=string.ascii_letters, min_size=1).map(lambda s: s.capitalize() + "Default"),
        ws1=st.text(string.whitespace),
        ws2=st.text(string.whitespace),
        ws3=st.text(string.whitespace),
    )
    def test_property_based_generic_refactoring(self, preset_name, ws1, ws2, ws3):
        """
        Tests the generalized refactoring with varied whitespace and preset names.
        """
        code_before = f"""
public SomeRole() : base(
    SomeBuilder.Build(
        SomeId,{ws1}
        SomeColor,{ws2}
        RolePropPresets.{preset_name}{ws3})
) {{}}
"""
        code_after = f"""
public SomeRole() : base(
    SomeBuilder.Build(
        SomeId,{ws1}
        SomeColor)
) {{}}
"""
        self.assertEqual(refactor_content(code_before), code_after)

if __name__ == '__main__':
    unittest.main()
