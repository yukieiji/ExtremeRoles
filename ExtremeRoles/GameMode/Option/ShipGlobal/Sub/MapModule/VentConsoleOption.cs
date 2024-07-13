
using AmongUs.GameOptions;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;

public enum VentAnimationMode
{
	VanillaAnimation,
	DonotWallHack,
	DonotOutVison,
}

public enum VentOption : int
{
	Disable,
	EngineerUseImpostor,
	CanKillInPlayer,
	AnimationModeInVison,
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
		category.GetValue<bool>((int)VentOption.Disable),
		category.GetValue<bool>((int)VentOption.EngineerUseImpostor),
		category.GetValue<bool>((int)VentOption.CanKillInPlayer),
		(VentAnimationMode)category.GetValue<int>((int)VentOption.AnimationModeInVison))
	{ }

	public static void Create(in OptionCategoryFactory factory)
	{
		var ventOption = factory.CreateBoolOption(VentOption.Disable, false);

		factory.CreateBoolOption(VentOption.CanKillInPlayer, false, ventOption, invert: true);
		factory.CreateBoolOption(
			VentOption.EngineerUseImpostor,
			false, ventOption, invert: true,
			hook: () =>
			{
				var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
				return
					currentGameOptions.IsTryCast<NormalGameOptionsV08>(out var normal) &&
					normal.RoleOptions.GetChancePerGame(RoleTypes.Engineer) > 0;
			});
		factory.CreateSelectionOption<VentOption, VentAnimationMode>(VentOption.AnimationModeInVison, parent: ventOption, invert: true);
	}
}
