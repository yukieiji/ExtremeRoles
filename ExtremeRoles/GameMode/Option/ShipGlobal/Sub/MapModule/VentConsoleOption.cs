
using ExtremeRoles.Module.NewOption.Factory;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;

public enum VentAnimationMode
{
	VanillaAnimation,
	DonotWallHack,
	DonotOutVison,
}

public enum VentOption : int
{
	DisableVent,
	EngineerUseImpostorVent,
	CanKillVentInPlayer,
	VentAnimationModeInVison,
}

public readonly struct VentConsoleOption(
	bool disable,
	bool engineerUseImpostorVent,
	bool canKillVentInPlayer,
	VentAnimationMode ventAnimation)
{
	public readonly bool Disable = disable;
	public readonly bool EngineerUseImpostorVent = engineerUseImpostorVent;
	public readonly bool CanKillVentInPlayer = canKillVentInPlayer;
	public readonly VentAnimationMode AnimationMode = ventAnimation;

	public VentConsoleOption(in OptionCategory category) : this(
		category.GetValue<bool>((int)VentOption.DisableVent),
		category.GetValue<bool>((int)VentOption.EngineerUseImpostorVent),
		category.GetValue<bool>((int)VentOption.CanKillVentInPlayer),
		(VentAnimationMode)category.GetValue<int>((int)VentOption.VentAnimationModeInVison))
	{ }

	public static void Create(in OptionCategoryFactory factory)
	{
		var ventOption = factory.CreateBoolOption(VentOption.DisableVent, false);

		factory.CreateBoolOption(VentOption.CanKillVentInPlayer, false, ventOption, invert: true);
		factory.CreateBoolOption(VentOption.EngineerUseImpostorVent, false, ventOption, invert: true);
		factory.CreateSelectionOption<VentOption, VentAnimationMode>(VentOption.VentAnimationModeInVison, parent: ventOption, invert: true);
	}
}
