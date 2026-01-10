import textwrap
from refactor.replace_to_presets_4th import (
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

def test_crewmate_multiline():
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
    assert apply_role_prop_presets(source) == expected

def test_impostor_multiline_with_extra_args():
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
    assert apply_role_prop_presets(source).strip() == expected.strip()

def test_crewmate_singleline():
    source = "base(RoleArgs.BuildCrewmate(id, color, RoleProp.HasTask | RoleProp.CanCallMeeting | RoleProp.CanRepairSabotage | RoleProp.CanUseAdmin | RoleProp.CanUseSecurity | RoleProp.CanUseVital))"
    expected = "base(RoleArgs.BuildCrewmate(id, color, RolePropPresets.CrewmateDefault))"
    assert apply_role_prop_presets(source) == expected

def test_impostor_singleline():
    source = "base(RoleArgs.BuildImpostor(id, RoleProp.CanKill | RoleProp.UseVent | RoleProp.UseSabotage | RoleProp.CanCallMeeting | RoleProp.CanRepairSabotage | RoleProp.CanUseAdmin | RoleProp.CanUseSecurity | RoleProp.CanUseVital))"
    expected = "base(RoleArgs.BuildImpostor(id, RolePropPresets.ImpostorDefault))"
    assert apply_role_prop_presets(source) == expected

def test_no_change_if_not_matching_crewmate():
    source = textwrap.dedent("""
    public MyRole() : base(
        RoleArgs.BuildCrewmate(
            RoleId,
            SomeColor,
            RoleProp.HasTask | RoleProp.CanKill))
    { }
    """)
    assert apply_role_prop_presets(source) == source

def test_no_change_if_not_matching_impostor():
    source = textwrap.dedent("""
    public MyRole() : base(
        RoleArgs.BuildImpostor(
            RoleId,
            RoleProp.CanKill | RoleProp.HasTask))
    { }
    """)
    assert apply_role_prop_presets(source) == source

def test_no_change_for_unrelated_code():
    source = "public void SomeMethod() { }"
    assert apply_role_prop_presets(source) == source

def test_flags_out_of_order():
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
    assert apply_role_prop_presets(source).strip() == expected.strip()


@given(flags=role_prop_flags_strategy())
def test_property_based_crewmate(flags):
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
        assert "RolePropPresets.CrewmateDefault" in result
    else:
        assert source == result

@given(flags=role_prop_flags_strategy())
def test_property_based_impostor(flags):
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
        assert "RolePropPresets.ImpostorDefault" in result
    else:
        assert source == result

def test_neutral_singleline():
    source = "base(RoleArgs.BuildNeutral(id, RoleProp.CanCallMeeting | RoleProp.CanRepairSabotage | RoleProp.CanUseAdmin | RoleProp.CanUseSecurity | RoleProp.CanUseVital))"
    expected = "base(RoleArgs.BuildNeutral(id, RolePropPresets.OptionalDefault))"
    assert apply_role_prop_presets(source) == expected

def test_liberal_singleline():
    source = "base(RoleArgs.BuildLiberal(id, RoleProp.CanCallMeeting | RoleProp.CanRepairSabotage | RoleProp.CanUseAdmin | RoleProp.CanUseSecurity | RoleProp.CanUseVital))"
    expected = "base(RoleArgs.BuildLiberal(id, RolePropPresets.OptionalDefault))"
    assert apply_role_prop_presets(source) == expected

@given(flags=role_prop_flags_strategy())
def test_property_based_neutral(flags):
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
        assert "RolePropPresets.OptionalDefault" in result
    else:
        assert source == result

@given(flags=role_prop_flags_strategy())
def test_property_based_liberal(flags):
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
        assert "RolePropPresets.OptionalDefault" in result
    else:
        assert source == result
