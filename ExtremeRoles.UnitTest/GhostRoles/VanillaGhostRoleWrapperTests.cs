using System;
using AmongUs.GameOptions;
using Moq;
using Xunit;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.UnitTest.GhostRoles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class VanillaGhostRoleWrapperTests
{
    private readonly Mock<TranslationController> mockTranslation;

    public VanillaGhostRoleWrapperTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        this.mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
    }

    [Fact]
    public void Constructor_GuardianAngel_SetsCorrectProperties()
    {
        var wrapper = new VanillaGhostRoleWrapper(RoleTypes.GuardianAngel);

        Assert.True(wrapper.HasTask);
        Assert.Equal(ExtremeRoleType.Crewmate, wrapper.Team);
        Assert.True(wrapper.IsCrewmate());
        Assert.False(wrapper.IsImpostor());
        Assert.False(wrapper.IsNeutral());
        Assert.True(wrapper.IsVanillaRole());
        Assert.Equal(RoleTypes.GuardianAngel.ToString(), wrapper.Name);
    }

    [Fact]
    public void Constructor_ImpostorGhost_SetsCorrectProperties()
    {
        var wrapper = new VanillaGhostRoleWrapper(RoleTypes.ImpostorGhost);

        Assert.False(wrapper.HasTask);
        Assert.Equal(ExtremeRoleType.Impostor, wrapper.Team);
        Assert.False(wrapper.IsCrewmate());
        Assert.True(wrapper.IsImpostor());
        Assert.False(wrapper.IsNeutral());
        Assert.True(wrapper.IsVanillaRole());
        Assert.Equal(RoleTypes.ImpostorGhost.ToString(), wrapper.Name);
    }

    [Fact]
    public void Constructor_CrewmateGhost_SetsCorrectProperties()
    {
        var wrapper = new VanillaGhostRoleWrapper(RoleTypes.CrewmateGhost);

        Assert.True(wrapper.HasTask);
        Assert.Equal(ExtremeRoleType.Crewmate, wrapper.Team);
        Assert.True(wrapper.IsCrewmate());
        Assert.False(wrapper.IsImpostor());
        Assert.False(wrapper.IsNeutral());
        Assert.True(wrapper.IsVanillaRole());
        Assert.Equal(RoleTypes.CrewmateGhost.ToString(), wrapper.Name);
    }

    [Fact]
    public void InitializeAndMeetingHooks_ExecuteWithoutException()
    {
        var wrapper = new VanillaGhostRoleWrapper(RoleTypes.GuardianAngel);

        wrapper.Initialize();
        wrapper.ResetOnMeetingStart();
        wrapper.ResetOnMeetingEnd();

        Assert.True(wrapper.IsVanillaRole());
    }

    [Fact]
    public void GetRoleFilter_ReturnsEmptyHashSet()
    {
        var wrapper = new VanillaGhostRoleWrapper(RoleTypes.GuardianAngel);

        var filter = wrapper.GetRoleFilter();

        Assert.Empty(filter);
    }

    [Fact]
    public void CreateAbility_ThrowsException()
    {
        var wrapper = new VanillaGhostRoleWrapper(RoleTypes.GuardianAngel);

        Assert.Throws<Exception>(() => wrapper.CreateAbility());
    }
}
