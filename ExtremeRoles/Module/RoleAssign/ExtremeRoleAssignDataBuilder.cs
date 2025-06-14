
using System;
using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;

using Microsoft.Extensions.DependencyInjection;

namespace ExtremeRoles.Module.RoleAssign;

#nullable enable

public class ExtremeRoleAssignDataBuilder(
	IServiceProvider provider,
	IRoleAssignDataPreparer preparer
) : IRoleAssignDataBuilder
{
	public enum Priority
	{
		Combination,
		Single,

		Not = 100,
	}

	private readonly IRoleAssignDataPreparer preparer = preparer;
	private readonly IRoleAssignDataBuildBehaviour[] behaviour = provider.GetServices<IRoleAssignDataBuildBehaviour>().OrderBy(
		x => x.Priority).ToArray();

	public IReadOnlyList<IPlayerToExRoleAssignData> Build()
	{
		var prepareData = this.preparer.Prepare();

		Logging.Debug(prepareData.RoleSpawn.ToString());
		Logging.Debug(prepareData.Limit.ToString());

		if (ExtremeGameModeManager.Instance.EnableXion)
		{
			PlayerControl loaclPlayer = PlayerControl.LocalPlayer;
			var assignData = prepareData.Assign;
			assignData.AddAssignData(
				new PlayerToSingleRoleAssignData(
					loaclPlayer.PlayerId,
					(int)ExtremeRoleId.Xion,
					assignData.ControlId));
			assignData.RemveFromPlayerControl(loaclPlayer);
		}

		GhostRoleSpawnDataManager.Instance.Create(prepareData.RoleSpawn.UseGhostCombRole);

		RoleAssignFilter.Instance.Initialize();

		foreach (var beha in this.behaviour)
		{
			beha.Build(in prepareData);
		}

		return prepareData.Assign.Data;
	}
}
