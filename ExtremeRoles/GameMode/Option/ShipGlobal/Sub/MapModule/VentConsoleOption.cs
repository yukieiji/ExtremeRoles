
using AmongUs.GameOptions;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Module.CustomOption.Interfaces;

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

	public VentConsoleOption(in IOptionLoader loader) : this(
		loader.GetValue<bool>((int)VentOption.Disable),
		loader.GetValue<bool>((int)VentOption.EngineerUseImpostor),
		loader.GetValue<bool>((int)VentOption.CanKillInPlayer),
		(VentAnimationMode)loader.GetValue<int>((int)VentOption.AnimationModeInVison))
	{ }

	public static void Create(in DefaultBuilder factory)
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
					currentGameOptions.IsTryCast<NormalGameOption>(out var normal) &&
					normal.RoleOptions.GetChancePerGame(RoleTypes.Engineer) > 0;
			});
		factory.CreateSelectionOption<VentOption, VentAnimationMode>(VentOption.AnimationModeInVison, parent: ventOption, invert: true);
	}
}
