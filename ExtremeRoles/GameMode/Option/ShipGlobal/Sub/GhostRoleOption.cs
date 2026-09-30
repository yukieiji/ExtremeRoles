
using AmongUs.GameOptions;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.OLDS;
using System;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub;

[Flags]
public enum VanillaCrewmateGhostRoleAssign
{
	None = 0,
	NeutalOk = 1 << 0,
	LiberalOk = 1 << 1,
}

public readonly struct GhostRoleOption(in OptionCategory category)
{
	public readonly float HauntMinigameMaxSpeed = category.GetValue<GhostRoleGlobalOption, float>(GhostRoleGlobalOption.HauntMinigameMaxSpeed);
	public readonly VanillaCrewmateGhostRoleAssign AssignToVanillaCrewmateGhostRole = create(category);
	public readonly bool IsBlockGAAbilityReport = category.GetValue<GhostRoleGlobalOption, bool>(GhostRoleGlobalOption.IsBlockGAAbilityReport);

	public static void Create(in OptionCategoryFactory factory)
	{
		factory.CreateFloatOption(
			GhostRoleGlobalOption.HauntMinigameMaxSpeed,
			4.0f, 0.75f, 4.0f, 0.05f);
		factory.CreateBoolOption(GhostRoleGlobalOption.IsAssignNeutralToVanillaCrewGhostRole, true);
		factory.CreateBoolOption(GhostRoleGlobalOption.IsAssignLiberalToVanillaCrewGhostRole, true);
		factory.CreateBoolOption(
			GhostRoleGlobalOption.IsBlockGAAbilityReport, false,
			new VanillaRoleActive(RoleTypes.GuardianAngel));
	}
	private static VanillaCrewmateGhostRoleAssign create(OptionCategory category)
	{
		var result = VanillaCrewmateGhostRoleAssign.None;
		if (category.GetValue<GhostRoleGlobalOption, bool>(GhostRoleGlobalOption.IsAssignNeutralToVanillaCrewGhostRole))
		{
			result |= VanillaCrewmateGhostRoleAssign.NeutalOk;
		}
		if (category.GetValue<GhostRoleGlobalOption, bool>(GhostRoleGlobalOption.IsAssignLiberalToVanillaCrewGhostRole))
		{
			result |= VanillaCrewmateGhostRoleAssign.LiberalOk;
		}
		return result;
	}
}
