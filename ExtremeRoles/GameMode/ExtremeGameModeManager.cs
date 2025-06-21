using AmongUs.GameOptions;
using ExtremeRoles.GameMode.Factory;
using ExtremeRoles.GameMode.IntroRunner;
using ExtremeRoles.GameMode.Logic.Usable;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;



using ExtremeRoles.Roles;

// TODO: setプロパティ => initにする

#nullable enable

namespace ExtremeRoles.GameMode;

public sealed class ExtremeGameModeManager
{
#pragma warning disable CS8625 // null リテラルを null 非許容参照型に変換できません。
	public static ExtremeGameModeManager Instance { get; private set; } = null;
#pragma warning restore CS8625 // null リテラルを null 非許容参照型に変換できません。

	public GameModes CurrentGameMode { get; }

    public IShipGlobalOption ShipOption { get; private set; }
    public IRoleSelector RoleSelector { get; private set; }

    public ILogicUsable Usable { get; private set; }

	public bool EnableXion => this.isXionActive && this.RoleSelector.CanUseXion;

	public bool isXionActive = false;

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。'required' 修飾子を追加するか、Null 許容として宣言することを検討してください。
	public ExtremeGameModeManager(GameModes mode)
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。'required' 修飾子を追加するか、Null 許容として宣言することを検討してください。
	{
        CurrentGameMode = mode;
    }

    public static void Create(GameModes mode)
    {
        GameModes currentMode = Instance?.CurrentGameMode ?? GameModes.None;

        if (currentMode == mode) { return; }

        Instance = new ExtremeGameModeManager(mode);

        IModeFactory? factory = mode switch
        {
            GameModes.Normal or GameModes.NormalFools => new ClassicGameModeFactory(),
            GameModes.HideNSeek or GameModes.SeekFools => new HideNSeekGameModeFactory(),
            _ => null,
        };

		if (factory is null)
		{
			return;
		}

        Instance.ShipOption = factory.CreateGlobalOption();
        Instance.RoleSelector = factory.CreateRoleSelector();
        Instance.Usable = factory.CreateLogicUsable();
    }

    public void Load()
    {
        Instance.ShipOption.Load();

		isXionActive = IRoleSelector.RawXionUse;
	}

    public IIntroRunner? GetIntroRunner()
        => CurrentGameMode switch
        {
            GameModes.Normal or GameModes.NormalFools => new ClassicIntroRunner(),
            GameModes.HideNSeek or GameModes.SeekFools => new HideNSeekIntroRunner(),
            _ => null
        };
}
