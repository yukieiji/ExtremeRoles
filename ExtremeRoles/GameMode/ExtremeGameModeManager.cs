using AmongUs.GameOptions;
using ExtremeRoles.GameMode.Factory;
using ExtremeRoles.GameMode.IntroRunner;
using ExtremeRoles.GameMode.Logic.Usable;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;

// TODO: setプロパティ => initにする

namespace ExtremeRoles.GameMode;

public sealed class ExtremeGameModeManager
{
    public static ExtremeGameModeManager Instance { get; private set; } = null;

    public GameModes CurrentGameMode { get; }

    public IShipGlobalOption ShipOption { get; private set; }
    public IRoleSelector RoleSelector { get; private set; }

    public ILogicUsable Usable { get; private set; }

	public bool EnableXion => this.isXionActive && this.RoleSelector.CanUseXion;

	public bool isXionActive = false;

    public ExtremeGameModeManager(GameModes mode)
    {
        CurrentGameMode = mode;
    }

    public static void Create(GameModes mode)
    {
        GameModes currentMode = Instance?.CurrentGameMode ?? GameModes.None;

        if (currentMode == mode) { return; }

        Instance = new ExtremeGameModeManager(mode);

        IModeFactory factory = mode switch
        {
            GameModes.Normal => new ClassicGameModeFactory(),
            GameModes.HideNSeek => new HideNSeekGameModeFactory(),
            _ => null,
        };

        Instance.ShipOption = factory.CreateGlobalOption();
        Instance.RoleSelector = factory.CreateRoleSelector();
        Instance.Usable = factory.CreateLogicUsable();
    }

    public void Load()
    {
        Instance.ShipOption.Load();

		isXionActive = OptionManager.Instance.GetValue<bool>(
			(int)RoleGlobalOption.UseXion);
	}

    public IIntroRunner GetIntroRunner()
        => CurrentGameMode switch
        {
            GameModes.Normal => new ClassicIntroRunner(),
            GameModes.HideNSeek => new HideNSeekIntroRunner(),
            _ => null
        };
}
