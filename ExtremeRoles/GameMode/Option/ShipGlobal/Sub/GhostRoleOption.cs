
using AmongUs.GameOptions;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomOption.Factory.Old;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub;

public readonly struct GhostRoleOption(in OptionCategory category)
{
	public readonly float HauntMinigameMaxSpeed = category.GetValue<GhostRoleGlobalOption, float>(GhostRoleGlobalOption.HauntMinigameMaxSpeed);
	public readonly bool IsAssignNeutralToVanillaCrewGhostRole = category.GetValue<GhostRoleGlobalOption, bool>(GhostRoleGlobalOption.IsAssignNeutralToVanillaCrewGhostRole);
	public readonly bool IsRemoveAngleIcon = category.GetValue<GhostRoleGlobalOption, bool>(GhostRoleGlobalOption.IsRemoveAngleIcon);
	public readonly bool IsBlockGAAbilityReport = category.GetValue<GhostRoleGlobalOption, bool>(GhostRoleGlobalOption.IsBlockGAAbilityReport);

	public static void Create(in OldOptionCategoryFactory factory)
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
