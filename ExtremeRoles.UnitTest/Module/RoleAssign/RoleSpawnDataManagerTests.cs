#nullable enable

using AmongUs.GameOptions;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.RoleAssign;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class RoleSpawnDataManagerTests
{
    public RoleSpawnDataManagerTests()
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
    public void Constructor_InitializesData()
    {
        var manager = new RoleSpawnDataManager();

        Assert.NotNull(manager.CurrentSingleRoleSpawnData);
        Assert.NotNull(manager.CurrentCombRoleSpawnData);
        Assert.NotNull(manager.UseGhostCombRole);
        Assert.NotNull(manager.CurrentSingleRoleUseNum);

        var str = manager.ToString();
        Assert.Contains("RoleSpawnInfo", str);
    }
}
