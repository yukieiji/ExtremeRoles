import re
import textwrap

# Define the sets of flags for each preset.
OPTIONAL_DEFAULT_FLAGS = {
    "RoleProp.CanCallMeeting",
    "RoleProp.CanRepairSabotage",
    "RoleProp.CanUseAdmin",
    "RoleProp.CanUseSecurity",
    "RoleProp.CanUseVital",
}

CREWMATE_DEFAULT_FLAGS = OPTIONAL_DEFAULT_FLAGS | {"RoleProp.HasTask"}
IMPOSTOR_DEFAULT_FLAGS = OPTIONAL_DEFAULT_FLAGS | {
    "RoleProp.CanKill",
    "RoleProp.UseVent",
    "RoleProp.UseSabotage",
}

def _normalize_flags(flag_string):
    """Splits a string of flags, strips whitespace, and returns a set."""
    return set(flag.strip() for flag in flag_string.split('|'))

def apply_role_prop_presets(csharp_code):
    """
    Applies RolePropPresets to C# constructor calls in the given code string.
    """

    def crewmate_replacer(match):
        # Group 1: Leading part of the call
        # Group 2: Whitespace
        # Group 3: The flags
        leading, whitespace, flags_str = match.groups()
        flags = _normalize_flags(flags_str)
        if flags == CREWMATE_DEFAULT_FLAGS:
            return f"{leading}{whitespace}RolePropPresets.CrewmateDefault"
        return match.group(0)  # Return original if no match

    def impostor_replacer(match):
        # Group 1: Leading part of the call
        # Group 2: Whitespace
        # Group 3: The flags
        leading, whitespace, flags_str = match.groups()
        flags = _normalize_flags(flags_str)
        if flags == IMPOSTOR_DEFAULT_FLAGS:
            return f"{leading}{whitespace}RolePropPresets.ImpostorDefault"
        return match.group(0) # Return original if no match

    def optional_default_replacer(match):
        leading, whitespace, flags_str = match.groups()
        flags = _normalize_flags(flags_str)
        if flags == OPTIONAL_DEFAULT_FLAGS:
            return f"{leading}{whitespace}RolePropPresets.OptionalDefault"
        return match.group(0)

    # Pattern for BuildCrewmate: captures up to the last comma, whitespace, and then the flags.
    crewmate_pattern = re.compile(
        r"(RoleArgs\.BuildCrewmate\([^,]+,\s*[^,]+,)(\s*)(RoleProp\..*?)(?=\s*\))",
        re.DOTALL
    )

    # Pattern for BuildImpostor: captures up to the last comma, whitespace, and then the flags.
    impostor_pattern = re.compile(
        r"(RoleArgs\.BuildImpostor\([^,]+,)(\s*)(RoleProp\..*?)(?=\s*\))",
        re.DOTALL
    )

    neutral_pattern = re.compile(
        r"(RoleArgs\.BuildNeutral\([^,]+,)(\s*)(RoleProp\..*?)(?=\s*\))",
        re.DOTALL
    )

    liberal_pattern = re.compile(
        r"(RoleArgs\.BuildLiberal\([^,]+,)(\s*)(RoleProp\..*?)(?=\s*\))",
        re.DOTALL
    )

    modified_code = crewmate_pattern.sub(crewmate_replacer, csharp_code)
    modified_code = impostor_pattern.sub(impostor_replacer, modified_code)
    modified_code = neutral_pattern.sub(optional_default_replacer, modified_code)
    modified_code = liberal_pattern.sub(optional_default_replacer, modified_code)

    return modified_code

if __name__ == '__main__':
    # Example Usage
    sample_code = textwrap.dedent("""
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

        // Mismatched Crewmate
        public Mismatched() : base(
            RoleArgs.BuildCrewmate(
                ExtremeRoleId.Mismatched,
                ColorPalette.Red,
                RoleProp.HasTask | RoleProp.CanKill
            )
        ) { }
    """)

    print("--- Original Code ---")
    print(sample_code)

    refactored_code = apply_role_prop_presets(sample_code)

    print("\\n--- Refactored Code ---")
    print(refactored_code)

    expected_output = textwrap.dedent("""
        // SingleRoleBase
        public Teleporter() : base(
            RoleArgs.BuildCrewmate(
                ExtremeRoleId.Teleporter,
                ColorPalette.TeleporterCherry,
                    RolePropPresets.CrewmateDefault))
        { }

        // MultiAssignRoleBase
        public Assassin() : base(
                RoleArgs.BuildImpostor(ExtremeRoleId.Assassin,
                    RolePropPresets.ImpostorDefault),
                 tab: OptionTab.CombinationTab)
         {
         }

        // Mismatched Crewmate
        public Mismatched() : base(
            RoleArgs.BuildCrewmate(
                ExtremeRoleId.Mismatched,
                ColorPalette.Red,
                RoleProp.HasTask | RoleProp.CanKill
            )
        ) { }
    """)
    assert refactored_code.strip() == expected_output.strip()
    print("\\nAssertion Passed!")
