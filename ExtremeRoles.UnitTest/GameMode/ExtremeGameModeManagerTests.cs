using AmongUs.GameOptions;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.IntroRunner;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
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
    public void Create_NormalGameMode_InitializesInstanceAndComponents()
    {
        // Act
        ExtremeGameModeManager.Create(GameModes.Normal);

        // Assert
        var instance = ExtremeGameModeManager.Instance;
        Assert.NotNull(instance);
        Assert.Equal(GameModes.Normal, instance.CurrentGameMode);
    }

    [Fact]
    public void Create_NormalFoolsGameMode_InitializesInstanceAndComponents()
    {
        // Act
        ExtremeGameModeManager.Create(GameModes.NormalFools);

        // Assert
        var instance = ExtremeGameModeManager.Instance;
        Assert.NotNull(instance);
        Assert.Equal(GameModes.NormalFools, instance.CurrentGameMode);
    }

    [Fact]
    public void Create_HideNSeekGameMode_InitializesInstanceAndComponents()
    {
        // Act
        ExtremeGameModeManager.Create(GameModes.HideNSeek);

        // Assert
        var instance = ExtremeGameModeManager.Instance;
        Assert.NotNull(instance);
        Assert.Equal(GameModes.HideNSeek, instance.CurrentGameMode);
    }

    [Fact]
    public void Create_SeekFoolsGameMode_InitializesInstanceAndComponents()
    {
        // Act
        ExtremeGameModeManager.Create(GameModes.SeekFools);

        // Assert
        var instance = ExtremeGameModeManager.Instance;
        Assert.NotNull(instance);
        Assert.Equal(GameModes.SeekFools, instance.CurrentGameMode);
	}

    [Fact]
    public void Create_SameGameMode_DoesNotRecreate()
    {
        // Arrange
        ExtremeGameModeManager.Create(GameModes.Normal);
        var initialInstance = ExtremeGameModeManager.Instance;

        // Act
        ExtremeGameModeManager.Create(GameModes.Normal);

        // Assert
        var currentInstance = ExtremeGameModeManager.Instance;
        Assert.Same(initialInstance, currentInstance);
    }

    [Fact]
    public void Create_UnsupportedGameMode_NullFactoryComponents()
    {
        // Act
        ExtremeGameModeManager.Create(GameModes.None);

        // Assert
        var instance = ExtremeGameModeManager.Instance;
        Assert.NotNull(instance);
        Assert.Equal(GameModes.None, instance.CurrentGameMode);
        Assert.Null(instance.ShipOption);
        Assert.Null(instance.RoleSelector);
        Assert.Null(instance.Usable);
    }

    [Fact]
    public void Load_LoadsShipOptionAndSetsXionState()
    {
        // Arrange
        ExtremeGameModeManager.Create(GameModes.Normal);
        var instance = ExtremeGameModeManager.Instance;

        // Act
        instance.Load();
    }
}
