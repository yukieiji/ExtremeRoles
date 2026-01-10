import unittest
from refactor_constructors import refactor_constructor, PROPS
from hypothesis import given, strategies as st, settings

class TestRefactorConstructors(unittest.TestCase):

    def test_single_role_base_refactoring(self):
        content = """
// SingleRoleBase
public Teleporter() : base(
    RoleArgs.BuildCrewmate(
        ExtremeRoleId.Teleporter,
        ColorPalette.TeleporterCherry),
    false, true, false, false,
    true, true, true, true, true)
{ }
        """
        expected = """
// SingleRoleBase
public Teleporter() : base(
    RoleArgs.BuildCrewmate(
        ExtremeRoleId.Teleporter,
        ColorPalette.TeleporterCherry,
            RoleProp.HasTask |
            RoleProp.CanCallMeeting |
            RoleProp.CanRepairSabotage |
            RoleProp.CanUseAdmin |
            RoleProp.CanUseSecurity |
            RoleProp.CanUseVital))
{ }
        """
        result_content, _ = refactor_constructor(content)
        self.assertEqual(
            " ".join(result_content.split()),
            " ".join(expected.split())
        )

    def test_multi_assign_role_base_refactoring(self):
        content = """
// MultiAssignRoleBase
 public Assassin() : base(
        RoleArgs.BuildImpostor(ExtremeRoleId.Assassin),
         true, false, true, true,
         true, true, true, true, true,
         tab: OptionTab.CombinationTab)
 {
 }
        """
        expected = """
// MultiAssignRoleBase
 public Assassin() : base(
    RoleArgs.BuildImpostor(ExtremeRoleId.Assassin,
            RoleProp.CanKill |
            RoleProp.UseVent |
            RoleProp.UseSabotage |
            RoleProp.CanCallMeeting |
            RoleProp.CanRepairSabotage |
            RoleProp.CanUseAdmin |
            RoleProp.CanUseSecurity |
            RoleProp.CanUseVital),
         tab: OptionTab.CombinationTab)
 {
 }
        """
        result_content, _ = refactor_constructor(content)
        self.assertEqual(
            " ".join(result_content.split()),
            " ".join(expected.split())
        )

    def test_no_change_for_already_refactored(self):
        content = """
public Teleporter() : base(
    RoleArgs.BuildCrewmate(
        ExtremeRoleId.Teleporter,
        ColorPalette.TeleporterCherry,
            RoleProp.HasTask |
            RoleProp.CanCallMeeting))
{ }
        """
        result_content, changes = refactor_constructor(content)
        self.assertEqual(content, result_content)
        self.assertEqual(changes, 0)

    def test_real_assassin_constructor(self):
        content = """
 public Assassin() : base(
        RoleCore.BuildImpostor(ExtremeRoleId.Assassin),
         true, false, true, true,
         tab: OptionTab.CombinationTab)
 {
 }
        """
        expected = """
 public Assassin() : base(
    RoleArgs.BuildImpostor(ExtremeRoleId.Assassin,
            RoleProp.CanKill |
            RoleProp.UseVent |
            RoleProp.UseSabotage |
            RoleProp.CanCallMeeting |
            RoleProp.CanRepairSabotage |
            RoleProp.CanUseAdmin |
            RoleProp.CanUseSecurity |
            RoleProp.CanUseVital),
         tab: OptionTab.CombinationTab)
 {
 }
        """
        result_content, _ = refactor_constructor(content)
        self.assertEqual(
            " ".join(result_content.split()),
            " ".join(expected.split())
        )

    @given(st.lists(st.booleans(), min_size=2, max_size=9), st.sampled_from(["RoleArgs", "RoleCore"]), st.booleans())
    @settings(max_examples=50, deadline=None) # Keep the test run time reasonable, remove deadline for CI
    def test_property_based_refactoring(self, bools, builder_type, has_tab):
        # Dynamically build the input C# code string
        bool_str = ", ".join(str(b).lower() for b in bools)
        tab_str = ", tab: OptionTab.CombinationTab" if has_tab else ""

        input_code = f"""
        public MyRole() : base(
            {builder_type}.BuildCrewmate(ExtremeRoleId.MyRole),
            {bool_str}{tab_str})
        {{ }}
        """

        result_content, changes = refactor_constructor(input_code)

        self.assertEqual(changes, 1)
        self.assertIn("RoleArgs.BuildCrewmate", result_content)

        # Verify that the correct props are present
        for i, prop in enumerate(PROPS):
            # The prop should be present if its corresponding boolean was true
            is_prop_present = prop in result_content

            if i < len(bools):
                # This prop was explicitly specified
                self.assertEqual(is_prop_present, bools[i])
            else:
                # Props not specified in a partial list are defaulted to true
                self.assertTrue(is_prop_present)

        if has_tab:
            self.assertIn("tab: OptionTab.CombinationTab", result_content)
        else:
            self.assertNotIn("tab: OptionTab.CombinationTab", result_content)


if __name__ == '__main__':
    unittest.main()
