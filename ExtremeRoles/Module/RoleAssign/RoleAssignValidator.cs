using System;
using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Helper;

using Microsoft.Extensions.DependencyInjection;

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

		foreach (var checker in this.checkers)
		{
			Logging.Debug($"Running checker: {checker.GetType().Name}");
			var ngRoleIds = checker.GetNgData(prepareData);

			if (ngRoleIds == null || !ngRoleIds.Any())
			{
				Logging.Debug($"No NG data found by {checker.GetType().Name}.");
				continue;
			}

			Logging.Info($"NG data found by {checker.GetType().Name}. IDs: {string.Join(", ", ngRoleIds.Select(id => id.ToString()))}");
			foreach (var ngRoleIdEnum in ngRoleIds) // ngRoleIdEnum is ExtremeRoleId
			{
				if (ProcessNgRole(ngRoleIdEnum, prepareData))
				{
					isUpdate = true; // If any role processing leads to an update, the overall method result is an update.
				}
			}
		}

		Logging.Debug($"------ RoleAssignValidator.IsReBuild END (isUpdate: {isUpdate}) ------");
		return isUpdate;
	}

	private bool ProcessNgRole(ExtremeRoleId ngRoleIdEnum, PreparationData prepareData)
	{
		bool dataUpdatedInThisCall = false;
		int ngRoleId = (int)ngRoleIdEnum;
		Logging.Debug($"Processing NG RoleId: {ngRoleIdEnum}");

		var assignmentsSnapshot = prepareData.Assign.Data.ToList();
		foreach (var assignment in assignmentsSnapshot)
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

			Logging.Info($"Player {playerId} has NG RoleId: {ngRoleIdEnum}. Attempting to remove.");
			bool removed = prepareData.Assign.RemoveAssignment(playerId, ngRoleId);

			if (!removed)
			{
				Logging.Warning($"Failed to remove NG RoleId: {ngRoleIdEnum} from Player {playerId}. It might have been removed by another process or rule.");
				continue; // Skip to next assignment if removal failed
			}

			dataUpdatedInThisCall = true; // Mark that data was updated
			Logging.Info($"Successfully removed NG RoleId: {ngRoleIdEnum} from Player {playerId}.");

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
				Logging.Info($"Spawn limit for Team: {teamToAdjust} increased by 1 due to removal of RoleId: {ngRoleIdEnum}.");
			}
			else
			{
				Logging.Warning($"Could not determine team for NG RoleId: {ngRoleIdEnum}. Spawn limit not adjusted.");
			}
		}
		return dataUpdatedInThisCall;
	}
}
