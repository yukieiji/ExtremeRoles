using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;

public sealed class NeutralSingleRoleAssignDataBuilder(
	IVanillaRoleProvider roleProvider,
	ISingleRoleAssignHelper helper,
	IModLogger logger) : INeutralSingleRoleAssignDataBuilder
{
	private readonly IReadOnlySet<RoleTypes> vanillaCrewRoleType = roleProvider.CrewmateRole;

	public void Build(in PreparationData data)
	{
		int neutralNum = data.Limit.Get(ExtremeRoleType.Neutral);
		if (neutralNum <= 0)
		{
			return;
		}

		logger.LogTrace("------------------------- SingleRoleAssign - Neutral - Start -------------------------");
		var neutralAssignTargetPlayer = helper.GetAssignablePlayer(data.Assign, ExtremeRoleType.Neutral).ToList();

		int assignNum = Math.Clamp(
			neutralNum,
			0, Math.Min(
				neutralAssignTargetPlayer.Count,
				data.RoleSpawn.CurrentSingleRoleUseNum[ExtremeRoleType.Neutral]));

		logger.LogTrace($"Neutral assign num:{assignNum}");

		neutralAssignTargetPlayer = neutralAssignTargetPlayer.OrderBy(
			x => RandomGenerator.Instance.Next()).Take(assignNum).ToList();

		helper.AddSingleExtremeRoleAssignDataFromTeamAndPlayer(
			data,
			ExtremeRoleType.Neutral,
			neutralAssignTargetPlayer,
			vanillaCrewRoleType);
		logger.LogTrace("------------------------- SingleRoleAssign - Neutral - End -------------------------");
	}
}
