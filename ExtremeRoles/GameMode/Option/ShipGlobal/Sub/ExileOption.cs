using AmongUs.GameOptions;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomOption.Factory.Old;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub;

public enum ExiledOption : int
{
	ConfirmExilMode,
	IsConfirmRole,
}

public enum ConfirmExileMode : byte
{
	Impostor,
	Crewmate,
	Neutral,
	AllTeam
}

public readonly struct ExileOption(in OptionCategory category)
{
	public readonly ConfirmExileMode Mode = (ConfirmExileMode)category.GetValue<int>((int)ExiledOption.ConfirmExilMode);
	public readonly bool IsConfirmRole = category.GetValue<bool>((int)ExiledOption.IsConfirmRole);

	public static void Create(in OptionCategoryFactory factory)
	{
		var hook = () =>
		{
			var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
			return
				currentGameOptions.IsTryCast<NormalGameOption>(out var normal) &&
				normal.ConfirmImpostor;
		};
		factory.CreateSelectionOption<ExiledOption, ConfirmExileMode>(ExiledOption.ConfirmExilMode, hook: hook);
		factory.CreateBoolOption(ExiledOption.IsConfirmRole, false, hook: hook);
	}
}
