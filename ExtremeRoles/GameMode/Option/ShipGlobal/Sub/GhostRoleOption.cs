
using AmongUs.GameOptions;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub;

public readonly struct GhostRoleOption(in IOptionLoader loader)
{
	public readonly float HauntMinigameMaxSpeed = loader.GetValue<GhostRoleGlobalOption, float>(GhostRoleGlobalOption.HauntMinigameMaxSpeed);
	public readonly bool IsAssignNeutralToVanillaCrewGhostRole = loader.GetValue<GhostRoleGlobalOption, bool>(GhostRoleGlobalOption.IsAssignNeutralToVanillaCrewGhostRole);
	public readonly bool IsRemoveAngleIcon = loader.GetValue<GhostRoleGlobalOption, bool>(GhostRoleGlobalOption.IsRemoveAngleIcon);
	public readonly bool IsBlockGAAbilityReport = loader.GetValue<GhostRoleGlobalOption, bool>(GhostRoleGlobalOption.IsBlockGAAbilityReport);

	public static void Create(in DefaultBuilder factory)
	{
		factory.CreateFloatOption(
			GhostRoleGlobalOption.HauntMinigameMaxSpeed,
			4.0f, 0.75f, 4.0f, 0.05f);
		factory.CreateBoolOption(GhostRoleGlobalOption.IsAssignNeutralToVanillaCrewGhostRole, true);
		factory.CreateBoolOption(GhostRoleGlobalOption.IsRemoveAngleIcon, false);
		factory.CreateBoolOption(
			GhostRoleGlobalOption.IsBlockGAAbilityReport, false,
			hook: () =>
			{
				var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
				return
					currentGameOptions.IsTryCast<NormalGameOption>(out var normal) &&
					normal.RoleOptions.GetChancePerGame(RoleTypes.GuardianAngel) > 0;
			});
	}
}
