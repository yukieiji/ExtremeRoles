using System.Linq;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Liberal;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Liberal;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class SpecialDoveAndMilitantTests
{
    public SpecialDoveAndMilitantTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupExtremeSystemTypeManagerMock();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();
        MockSetupHelper.SetupMockConfig(plugin);

        MockSetupHelper.SetupOptionManager();
    }

    [Fact]
    public void SpecialDove_ConstructorAndProperties_AreCorrect()
    {
        var dove = new SpecialDove();
        Assert.Equal(ExtremeRoleId.SpecialDove, dove.Core.Id);
        Assert.Equal(ExtremeRoleType.Liberal, dove.Core.Team);
        Assert.True(dove.HasTask);
        Assert.False(dove.CanKill);
    }

    [Fact]
    public void SpecialMilitant_ConstructorAndProperties_AreCorrect()
    {
        var militant = new SpecialMilitant();
        Assert.Equal(ExtremeRoleId.SpecialMilitant, militant.Core.Id);
        Assert.Equal(ExtremeRoleType.Liberal, militant.Core.Team);
        Assert.False(militant.HasTask);
        Assert.True(militant.CanKill);
    }

    [Fact]
    public void NormalRole_ContainsSpecialDoveAndSpecialMilitant()
    {
        Assert.True(ExtremeRoleManager.NormalRole.ContainsKey((int)ExtremeRoleId.SpecialDove));
        Assert.True(ExtremeRoleManager.NormalRole.ContainsKey((int)ExtremeRoleId.SpecialMilitant));

        var dove = ExtremeRoleManager.NormalRole[(int)ExtremeRoleId.SpecialDove];
        var militant = ExtremeRoleManager.NormalRole[(int)ExtremeRoleId.SpecialMilitant];

        Assert.IsType<SpecialDove>(dove);
        Assert.IsType<SpecialMilitant>(militant);
    }

    [Fact]
    public void ClassicGameModeRoleSelector_ContainsSpecialDoveAndSpecialMilitant()
    {
        var selector = new ClassicGameModeRoleSelector();
        var normalRoles = selector.UseNormalRoleId.ToList();

        Assert.Contains(ExtremeRoleId.SpecialDove, normalRoles);
        Assert.Contains(ExtremeRoleId.SpecialMilitant, normalRoles);
    }
}
