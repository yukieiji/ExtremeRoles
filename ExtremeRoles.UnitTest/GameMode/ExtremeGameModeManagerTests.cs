using AmongUs.GameOptions;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.Factory;
using ExtremeRoles.GameMode.IntroRunner;
using ExtremeRoles.GameMode.Logic.Usable;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomOption;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ExtremeGameModeManagerTests
{
    public ExtremeGameModeManagerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupExtremeSystemTypeManagerMock();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();
        MockSetupHelper.SetupMockConfig(plugin);

        if (ClientOption.Instance == null || !OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)OptionCreator.CommonOption.RandomOption, out _))
        {
            OptionCreator.Create();
        }
    }

    [Fact]
    public void Create_NormalMode_InitializesClassicComponents()
    {
        ExtremeGameModeManager.Create(GameModes.Normal);

        var instance = ExtremeGameModeManager.Instance;
        Assert.NotNull(instance);
        Assert.Equal(GameModes.Normal, instance.CurrentGameMode);
        Assert.IsType<ClassicGameModeShipGlobalOption>(instance.ShipOption);
        Assert.IsType<ClassicGameModeRoleSelector>(instance.RoleSelector);
        Assert.IsType<ClassicGameModeUsableLogic>(instance.Usable);
    }

    [Fact]
    public void Create_HideNSeekMode_InitializesHideNSeekComponents()
    {
        ExtremeGameModeManager.Create(GameModes.HideNSeek);

        var instance = ExtremeGameModeManager.Instance;
        Assert.NotNull(instance);
        Assert.Equal(GameModes.HideNSeek, instance.CurrentGameMode);
        Assert.IsType<HideNSeekModeShipGlobalOption>(instance.ShipOption);
        Assert.IsType<HideNSeekGameModeRoleSelector>(instance.RoleSelector);
        Assert.IsType<HideNSeekModeUsableLogic>(instance.Usable);
    }

    [Fact]
    public void Create_SameMode_DoesNotRecreateInstance()
    {
        ExtremeGameModeManager.Create(GameModes.Normal);
        var firstInstance = ExtremeGameModeManager.Instance;

        ExtremeGameModeManager.Create(GameModes.Normal);
        var secondInstance = ExtremeGameModeManager.Instance;

        Assert.Same(firstInstance, secondInstance);
    }

    [Fact]
    public void Create_UnsupportedMode_InstanceCreatedWithoutFactoryOptions()
    {
        // Recreate with a mode that has no factory (e.g., GameModes.None)
        ExtremeGameModeManager.Create(GameModes.None);

        var instance = ExtremeGameModeManager.Instance;
        Assert.NotNull(instance);
        Assert.Equal(GameModes.None, instance.CurrentGameMode);
        Assert.Null(instance.ShipOption);
        Assert.Null(instance.RoleSelector);
        Assert.Null(instance.Usable);
    }

    [Fact]
    public void GetIntroRunner_ReturnsCorrectRunnerForGameMode()
    {
        ExtremeGameModeManager.Create(GameModes.Normal);
        var normalRunner = ExtremeGameModeManager.Instance.GetIntroRunner();
        Assert.IsType<ClassicIntroRunner>(normalRunner);

        ExtremeGameModeManager.Create(GameModes.NormalFools);
        var normalFoolsRunner = ExtremeGameModeManager.Instance.GetIntroRunner();
        Assert.IsType<ClassicIntroRunner>(normalFoolsRunner);

        ExtremeGameModeManager.Create(GameModes.HideNSeek);
        var hnsRunner = ExtremeGameModeManager.Instance.GetIntroRunner();
        Assert.IsType<HideNSeekIntroRunner>(hnsRunner);

        ExtremeGameModeManager.Create(GameModes.SeekFools);
        var seekFoolsRunner = ExtremeGameModeManager.Instance.GetIntroRunner();
        Assert.IsType<HideNSeekIntroRunner>(seekFoolsRunner);

        ExtremeGameModeManager.Create(GameModes.None);
        var noneRunner = ExtremeGameModeManager.Instance.GetIntroRunner();
        Assert.Null(noneRunner);
    }

    [Fact]
    public void EnableXion_ReturnsExpectedValue()
    {
        ExtremeGameModeManager.Create(GameModes.Normal);
        var manager = ExtremeGameModeManager.Instance;

        manager.isXionActive = false;
        Assert.False(manager.EnableXion);

        manager.isXionActive = true;
        Assert.True(manager.EnableXion);
    }

    [Fact]
    public void Load_LoadsShipOptionAndUpdatesIsXionActive()
    {
        ExtremeGameModeManager.Create(GameModes.Normal);
        var manager = ExtremeGameModeManager.Instance;

        manager.Load();

        Assert.NotNull(manager.ShipOption);
        Assert.Equal(IRoleSelector.RawXionUse, manager.isXionActive);
    }
}
