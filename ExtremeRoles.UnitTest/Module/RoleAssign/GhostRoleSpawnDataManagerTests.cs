#nullable enable

using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class GhostRoleSpawnDataManagerTests
{
    public GhostRoleSpawnDataManagerTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        ExtremeGameModeManager.Create(GameModes.Normal);

        if (!OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, ExtremeRoles.Module.CustomOption.Implemented.PresetOption.CategoryId, out _))
        {
            OptionCreator.Create();
        }
    }

    [Fact]
    public void Create_And_QueryMethods()
    {
        var manager = GhostRoleSpawnDataManager.Instance;
        Assert.NotNull(manager);

        var list = new List<(CombinationRoleType, GhostAndAliveCombinationRoleManagerBase)>();
        manager.Create(list);

        Assert.False(manager.IsCombRole(ExtremeRoleId.Vigilante));
        Assert.Null(manager.GetUseGhostRole(ExtremeRoleType.Crewmate));
    }
}
