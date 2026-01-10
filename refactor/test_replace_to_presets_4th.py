import unittest
import textwrap
from replace_to_presets_4th import (
    apply_role_prop_presets,
    CREWMATE_DEFAULT_FLAGS,
    IMPOSTOR_DEFAULT_FLAGS,
    OPTIONAL_DEFAULT_FLAGS,
)
from hypothesis import given, strategies as st

# Helper to generate a set of RoleProp flags for hypothesis
ROLE_PROP_FLAGS = [
    "RoleProp.CanKill", "RoleProp.HasTask", "RoleProp.UseVent",
    "RoleProp.UseSabotage", "RoleProp.CanCallMeeting", "RoleProp.CanRepairSabotage",
    "RoleProp.CanUseAdmin", "RoleProp.CanUseSecurity", "RoleProp.CanUseVital"
]

@st.composite
def role_prop_flags_strategy(draw):
    # Create a subset of the available flags
    return draw(st.sets(st.sampled_from(ROLE_PROP_FLAGS)))

class TestApplyRolePropPresets(unittest.TestCase):

    def test_crewmate_multiline(self):
        source = textwrap.dedent("""
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
        """)
        expected = textwrap.dedent("""
        public Teleporter() : base(
            RoleArgs.BuildCrewmate(
                ExtremeRoleId.Teleporter,
                ColorPalette.TeleporterCherry,
                RolePropPresets.CrewmateDefault))
        { }
        """)
        self.assertEqual(apply_role_prop_presets(source), expected)

    def test_impostor_multiline_with_extra_args(self):
        source = textwrap.dedent("""
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
        { }
        """)
        expected = textwrap.dedent("""
        public Assassin() : base(
            RoleArgs.BuildImpostor(ExtremeRoleId.Assassin,
                RolePropPresets.ImpostorDefault),
             tab: OptionTab.CombinationTab)
        { }
        """)
        self.assertEqual(apply_role_prop_presets(source).strip(), expected.strip())

    def test_crewmate_singleline(self):
        source = "base(RoleArgs.BuildCrewmate(id, color, RoleProp.HasTask | RoleProp.CanCallMeeting | RoleProp.CanRepairSabotage | RoleProp.CanUseAdmin | RoleProp.CanUseSecurity | RoleProp.CanUseVital))"
        expected = "base(RoleArgs.BuildCrewmate(id, color, RolePropPresets.CrewmateDefault))"
        self.assertEqual(apply_role_prop_presets(source), expected)

    def test_impostor_singleline(self):
        source = "base(RoleArgs.BuildImpostor(id, RoleProp.CanKill | RoleProp.UseVent | RoleProp.UseSabotage | RoleProp.CanCallMeeting | RoleProp.CanRepairSabotage | RoleProp.CanUseAdmin | RoleProp.CanUseSecurity | RoleProp.CanUseVital))"
        expected = "base(RoleArgs.BuildImpostor(id, RolePropPresets.ImpostorDefault))"
        self.assertEqual(apply_role_prop_presets(source), expected)

    def test_no_change_if_not_matching_crewmate(self):
        source = textwrap.dedent("""
        public MyRole() : base(
            RoleArgs.BuildCrewmate(
                RoleId,
                SomeColor,
                RoleProp.HasTask | RoleProp.CanKill))
        { }
        """)
        self.assertEqual(apply_role_prop_presets(source), source)

    def test_no_change_if_not_matching_impostor(self):
        source = textwrap.dedent("""
        public MyRole() : base(
            RoleArgs.BuildImpostor(
                RoleId,
                RoleProp.CanKill | RoleProp.HasTask))
        { }
        """)
        self.assertEqual(apply_role_prop_presets(source), source)

    def test_no_change_for_unrelated_code(self):
        source = "public void SomeMethod() { }"
        self.assertEqual(apply_role_prop_presets(source), source)

    def test_flags_out_of_order(self):
        source = textwrap.dedent("""
        public Teleporter() : base(
            RoleArgs.BuildCrewmate(
                ExtremeRoleId.Teleporter,
                ColorPalette.TeleporterCherry,
                RoleProp.CanUseVital | RoleProp.HasTask |
                RoleProp.CanRepairSabotage | RoleProp.CanCallMeeting |
                RoleProp.CanUseAdmin | RoleProp.CanUseSecurity
                ))
        { }
        """)
        expected = textwrap.dedent("""
        public Teleporter() : base(
            RoleArgs.BuildCrewmate(
                ExtremeRoleId.Teleporter,
                ColorPalette.TeleporterCherry,
                RolePropPresets.CrewmateDefault
                ))
        { }
        """)
        self.assertEqual(apply_role_prop_presets(source).strip(), expected.strip())


    @given(flags=role_prop_flags_strategy())
    def test_property_based_crewmate(self, flags):
        flags_str = " | ".join(flags)
        if not flags_str: # Handle empty set
            flags_str = "RoleProp.None"

        source = f"""
        public MyRole() : base(
            RoleArgs.BuildCrewmate(RoleId, SomeColor, {flags_str}))
        {{ }}
        """

        result = apply_role_prop_presets(source)

        if set(flags) == CREWMATE_DEFAULT_FLAGS:
            self.assertIn("RolePropPresets.CrewmateDefault", result)
        else:
            self.assertEqual(source, result)

    @given(flags=role_prop_flags_strategy())
    def test_property_based_impostor(self, flags):
        flags_str = " | ".join(flags)
        if not flags_str: # Handle empty set
            flags_str = "RoleProp.None"

        source = f"""
        public MyRole() : base(
            RoleArgs.BuildImpostor(RoleId, {flags_str}))
        {{ }}
        """

        result = apply_role_prop_presets(source)

        if set(flags) == IMPOSTOR_DEFAULT_FLAGS:
            self.assertIn("RolePropPresets.ImpostorDefault", result)
        else:
            self.assertEqual(source, result)

    def test_neutral_singleline(self):
        source = "base(RoleArgs.BuildNeutral(id, RoleProp.CanCallMeeting | RoleProp.CanRepairSabotage | RoleProp.CanUseAdmin | RoleProp.CanUseSecurity | RoleProp.CanUseVital))"
        expected = "base(RoleArgs.BuildNeutral(id, RolePropPresets.OptionalDefault))"
        self.assertEqual(apply_role_prop_presets(source), expected)

    def test_liberal_singleline(self):
        source = "base(RoleArgs.BuildLiberal(id, RoleProp.CanCallMeeting | RoleProp.CanRepairSabotage | RoleProp.CanUseAdmin | RoleProp.CanUseSecurity | RoleProp.CanUseVital))"
        expected = "base(RoleArgs.BuildLiberal(id, RolePropPresets.OptionalDefault))"
        self.assertEqual(apply_role_prop_presets(source), expected)

    @given(flags=role_prop_flags_strategy())
    def test_property_based_neutral(self, flags):
        flags_str = " | ".join(flags)
        if not flags_str:
            flags_str = "RoleProp.None"

        source = f"""
        public MyRole() : base(
            RoleArgs.BuildNeutral(RoleId, {flags_str}))
        {{ }}
        """

        result = apply_role_prop_presets(source)

        if set(flags) == OPTIONAL_DEFAULT_FLAGS:
            self.assertIn("RolePropPresets.OptionalDefault", result)
        else:
            self.assertEqual(source, result)

    @given(flags=role_prop_flags_strategy())
    def test_property_based_liberal(self, flags):
        flags_str = " | ".join(flags)
        if not flags_str:
            flags_str = "RoleProp.None"

        source = f"""
        public MyRole() : base(
            RoleArgs.BuildLiberal(RoleId, {flags_str}))
        {{ }}
        """

        result = apply_role_prop_presets(source)

        if set(flags) == OPTIONAL_DEFAULT_FLAGS:
            self.assertIn("RolePropPresets.OptionalDefault", result)
        else:
            self.assertEqual(source, result)

if __name__ == '__main__':
    unittest.main()
