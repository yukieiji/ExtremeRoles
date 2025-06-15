using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign;


public class RoleAssignValidator(IServiceProvider provider) : IRoleAssignValidator
{
    private readonly IEnumerable<IRoleAssignDataChecker> checkers = provider.GetServices<IRoleAssignDataChecker>();

	public bool IsReBuild(in PreparationData prepareData)
	{
		Logging.Debug("------ RoleAssignValidator.IsReBuild START ------");
		bool isUpdate = false;

		if (!this.checkers.Any())
		{
			Logging.Debug("No checkers registered. Skipping validation.");
			Logging.Debug("------ RoleAssignValidator.IsReBuild END (No checkers) ------");
			return false;
		}

		var allNgData = new HashSet<ExtremeRoleId>();

		foreach (var checker in this.checkers)
		{
			string name = checker.GetType().Name;
			Logging.Debug($"Running checker: {name}");
			var ngRoleIds = checker.GetNgData(prepareData);

			if (ngRoleIds == null || !ngRoleIds.Any())
			{
				Logging.Debug($"No NG data found by {name}.");
				continue;
			}
			Logging.Debug($"NG data found by {name}. IDs: {string.Join(", ", ngRoleIds.Select(id => id.ToString()))}");
			allNgData.UnionWith(ngRoleIds);
		}

		foreach (var ngRoleIdEnum in allNgData)
		{
			if (ProcessNgRole(ngRoleIdEnum, prepareData))
			{
				isUpdate = true; // If any role processing leads to an update, the overall method result is an update.
			}
		}

		Logging.Debug($"------ RoleAssignValidator.IsReBuild END (isUpdate: {isUpdate}) ------");
		return isUpdate;
	}

	private static bool ProcessNgRole(ExtremeRoleId ngRoleIdEnum, PreparationData prepareData)
	{
		bool dataUpdatedInThisCall = false;
		int ngRoleId = (int)ngRoleIdEnum;
		Logging.Debug($"Processing NG RoleId: {ngRoleIdEnum}");

		foreach (var assignment in prepareData.Assign.Data.ToArray())
		{
			byte playerId = 0;
			int roleId = -1;

			if (assignment is PlayerToSingleRoleAssignData singleRoleAssignment)
			{
				playerId = singleRoleAssignment.PlayerId;
				roleId = singleRoleAssignment.RoleId;
			}
			else if (assignment is PlayerToCombRoleAssignData combRoleAssignment)
			{
				playerId = combRoleAssignment.PlayerId;
				roleId = combRoleAssignment.RoleId;
			}
			else // Should not happen if types are controlled, but good for robustness
			{
				continue;
			}

			if (roleId != ngRoleId)
			{
				continue;
			}

			Logging.Debug($"Player {playerId} has NG RoleId: {ngRoleIdEnum}. Attempting to remove.");
			if (!prepareData.Assign.RemoveAssignment(playerId, ngRoleId))
			{
				Logging.Debug($"Failed to remove NG RoleId: {ngRoleIdEnum} from Player {playerId}. It might have been removed by another process or rule.");
				continue; // Skip to next assignment if removal failed
			}

			dataUpdatedInThisCall = true; // Mark that data was updated
			Logging.Debug($"Successfully removed NG RoleId: {ngRoleIdEnum} from Player {playerId}.");

			PlayerControl? playerControlToRequeue = PlayerCache.AllPlayerControl.FirstOrDefault(pc => pc.PlayerId == playerId);
			if (playerControlToRequeue != null)
			{
				prepareData.Assign.AddPlayerToReassign(playerControlToRequeue);
				Logging.Debug($"PlayerId: {playerId} added back to re-assign queue.");
			}

			ExtremeRoleType teamToAdjust = ExtremeRoleType.Null;
			bool teamFound = false;

			if (ExtremeRoleManager.NormalRole.TryGetValue(ngRoleId, out var roleDefinition))
			{
				teamToAdjust = roleDefinition.Team;
				teamFound = true;
			}
			else
			{
				foreach (var combManager in ExtremeRoleManager.CombRole.Values)
				{
					var foundCombRolePart = combManager.Roles.FirstOrDefault(r => r.Id == ngRoleIdEnum);
					if (foundCombRolePart != null)
					{
						teamToAdjust = foundCombRolePart.Team;
						teamFound = true;
						break;
					}
				}
			}

			if (teamFound && teamToAdjust != ExtremeRoleType.Null)
			{
				prepareData.Limit.Reduce(teamToAdjust, -1);
				Logging.Debug($"Spawn limit for Team: {teamToAdjust} increased by 1 due to removal of RoleId: {ngRoleIdEnum}.");
			}
			else
			{
				Logging.Debug($"Could not determine team for NG RoleId: {ngRoleIdEnum}. Spawn limit not adjusted.");
			}
		}
		return dataUpdatedInThisCall;
	}
}
