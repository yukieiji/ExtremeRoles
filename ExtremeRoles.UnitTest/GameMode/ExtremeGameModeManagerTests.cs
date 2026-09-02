using System.Reflection;
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

        ResetInstance();
    }

    private static void ResetInstance()
    {
        var prop = typeof(ExtremeGameModeManager).GetProperty(nameof(ExtremeGameModeManager.Instance), BindingFlags.Public | BindingFlags.Static);
        prop?.SetValue(null, null);
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
        Assert.NotNull(instance.ShipOption);
        Assert.NotNull(instance.RoleSelector);
        Assert.NotNull(instance.Usable);
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
        Assert.NotNull(instance.ShipOption);
        Assert.NotNull(instance.RoleSelector);
        Assert.NotNull(instance.Usable);
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
        Assert.NotNull(instance.ShipOption);
        Assert.NotNull(instance.RoleSelector);
        Assert.NotNull(instance.Usable);
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
        Assert.NotNull(instance.ShipOption);
        Assert.NotNull(instance.RoleSelector);
        Assert.NotNull(instance.Usable);
    }

    [Fact]
    public void Create_SameGameMode_DoesNotRecreate()
    {
        // Arrange
        ExtremeGameModeManager.Create(GameModes.Normal);
        var initialInstance = ExtremeGameModeManager.Instance;
		Assert.NotNull(initialInstance);

        // Act
        ExtremeGameModeManager.Create(GameModes.Normal);

        // Assert
        var currentInstance = ExtremeGameModeManager.Instance;
        Assert.Same(initialInstance, currentInstance);
    }

    [Fact]
    public void Create_UnsupportedGameMode_NullFactoryComponents()
    {
        // Arrange
        ExtremeGameModeManager.Create(GameModes.Normal);

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

        // Assert
        Assert.Equal(IRoleSelector.RawXionUse, instance.isXionActive);
    }


    [Theory]
    [InlineData(GameModes.Normal, typeof(ClassicIntroRunner))]
    [InlineData(GameModes.NormalFools, typeof(ClassicIntroRunner))]
    [InlineData(GameModes.HideNSeek, typeof(HideNSeekIntroRunner))]
    [InlineData(GameModes.SeekFools, typeof(HideNSeekIntroRunner))]
    public void GetIntroRunner_SupportedModes_ReturnsExpectedType(GameModes mode, System.Type expectedType)
    {
        // Arrange
        ExtremeGameModeManager.Create(mode);
        var instance = ExtremeGameModeManager.Instance;

        // Act
        var runner = instance.GetIntroRunner();

        // Assert
        Assert.NotNull(runner);
        Assert.IsType(expectedType, runner);
    }

    [Fact]
    public void GetIntroRunner_UnsupportedMode_ReturnsNull()
    {
        // Arrange
        ExtremeGameModeManager.Create(GameModes.Normal);
        ExtremeGameModeManager.Create(GameModes.None);
        var instance = ExtremeGameModeManager.Instance;

        // Act
        var runner = instance.GetIntroRunner();

        // Assert
        Assert.Null(runner);
    }
}
