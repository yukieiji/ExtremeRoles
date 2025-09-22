using AmongUs.GameOptions;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;

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

public readonly struct ExileOption(in IOptionLoader loader)
{
	public readonly ConfirmExileMode Mode = (ConfirmExileMode)loader.GetValue<int>((int)ExiledOption.ConfirmExilMode);
	public readonly bool IsConfirmRole = loader.GetValue<bool>((int)ExiledOption.IsConfirmRole);

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
