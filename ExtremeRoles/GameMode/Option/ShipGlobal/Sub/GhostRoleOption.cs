
using AmongUs.GameOptions;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub;

public readonly struct GhostRoleOption(in OptionCategory category)
{
	public readonly bool IsAssignNeutralToVanillaCrewGhostRole = category.GetValue<bool>((int)GhostRoleGlobalOption.IsAssignNeutralToVanillaCrewGhostRole);
	public readonly bool IsRemoveAngleIcon = category.GetValue<bool>((int)GhostRoleGlobalOption.IsRemoveAngleIcon);
	public readonly bool IsBlockGAAbilityReport = category.GetValue<bool>((int)GhostRoleGlobalOption.IsBlockGAAbilityReport);

	public static void Create(in OptionCategoryFactory factory)
	{
		factory.CreateBoolOption(GhostRoleGlobalOption.IsAssignNeutralToVanillaCrewGhostRole, true);
		factory.CreateBoolOption(GhostRoleGlobalOption.IsRemoveAngleIcon, false);
		factory.CreateBoolOption(
			GhostRoleGlobalOption.IsBlockGAAbilityReport, false,
			hook: () =>
			{
				var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
				return
					currentGameOptions.IsTryCast<NormalGameOptionsV08>(out var normal) &&
					normal.RoleOptions.GetChancePerGame(RoleTypes.GuardianAngel) > 0;
			});
	}
}
