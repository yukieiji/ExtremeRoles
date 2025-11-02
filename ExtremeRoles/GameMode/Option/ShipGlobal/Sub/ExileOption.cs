using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub;

public sealed class ExiledCheckOptionActive : IOptionActivator
{
	public IOption Parent { get; } = null;

	public bool IsActive => 
		GameOptionsManager.Instance.CurrentGameOptions.IsTryCast<NormalGameOption>(out var normal) &&
		normal.ConfirmImpostor;
}

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
		var exileOptionActive = new ExiledCheckOptionActive();
		factory.CreateSelectionOption<ExiledOption, ConfirmExileMode>(ExiledOption.ConfirmExilMode, exileOptionActive);
		factory.CreateBoolOption(ExiledOption.IsConfirmRole, false, exileOptionActive);
	}
}
