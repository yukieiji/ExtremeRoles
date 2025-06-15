using System;
using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;


namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataChecker;

public class RoleDependencyRule
{
	public int RoleA_Id { get; }
	public int RoleB_Id { get; }
	public Func<SingleRoleBase, bool> SettingCChecker { get; }

	public RoleDependencyRule(ExtremeRoleId roleA, ExtremeRoleId roleB, Func<SingleRoleBase, bool> settingCChecker)
	{
		RoleA_Id = (int)roleA;
		RoleB_Id = (int)roleB;
		SettingCChecker = settingCChecker;
	}
}

public sealed class RoleDependencyRuleFactory
{
	public IReadOnlyList<RoleDependencyRule> Rules { get; } = new List<RoleDependencyRule>();
}

public sealed class RoleAssignDependencyChecker(RoleDependencyRuleFactory factory) : IRoleAssignDataChecker
{
	private readonly IReadOnlyList<RoleDependencyRule> rules = factory.Rules;

	public IReadOnlyList<ExtremeRoleId> GetNgData(in PreparationData data)
	{
		var ngRoleIds = new List<ExtremeRoleId>();
		var currentAssignments = data.Assign.Data;

		if (rules.Count == 0)
		{
			Logging.Debug("No dependency rules defined. Skipping validation.");
			return ngRoleIds; // Return empty list
		}

		foreach (var rule in rules) // Corrected: use 'rules' field
		{
			Logging.Debug($"Evaluating Rule: RoleA_Id={(ExtremeRoleId)rule.RoleA_Id}, RoleB_Id={(ExtremeRoleId)rule.RoleB_Id}");

			// Find all players assigned to RoleA_Id
			var assignmentsOfRoleA = currentAssignments
				.Where(a => GetRoleIdFromAssignment(a) == rule.RoleA_Id)
				.ToList(); // ToList is important if currentAssignments can change, but here we only read.

			if (!assignmentsOfRoleA.Any())
			{
				Logging.Debug($"No assignments found for RoleA_Id: {(ExtremeRoleId)rule.RoleA_Id}.");
				continue;
			}

			bool roleAExistsAndSettingCIsValid = false;
			foreach (var assignmentA in assignmentsOfRoleA)
			{
				byte playerIdForRoleA = GetPlayerIdFromAssignment(assignmentA);
				//SingleRoleBase? roleA_Instance = ExtremeRoleManager.GetRole(playerIdForRoleA); // This might get any role of the player
                // We need to ensure it's the specific RoleA_Id instance.
                // However, the SettingCChecker likely operates on the properties of RoleA_Id,
                // and we need an instance of that role to check its settings.
                // The original code fetched the gameRoleInstance and checked its Id.

				SingleRoleBase? roleA_Instance = null;
				// Attempt to get the specific role instance for the player that matches RoleA_Id
				// This assumes ExtremeRoleManager can provide an instance of a role by player ID and role ID,
				// or that we can retrieve all roles for a player and find the matching one.
				// For simplicity, let's assume a player can only have one instance of a specific role type,
				// or that GetRole returns the relevant one if multiple are possible (e.g. comb roles).
				// The original logic: ExtremeRoleManager.TryGetRole(playerId, out var gameRoleInstance) && gameRoleInstance.Id == rule.RoleA_Id
				// This implies TryGetRole gets *the* role for a player, which might be a SingleRole or a part of a CombRole.
				// Let's refine this to correctly get the instance of RoleA.

				if (ExtremeRoleManager.TryGetRole(playerIdForRoleA, out var gameRoleInstance) &&
				    gameRoleInstance.Id == (ExtremeRoleId)rule.RoleA_Id)
				{
					roleA_Instance = gameRoleInstance as SingleRoleBase; // Assuming SettingCChecker expects SingleRoleBase
                                                                // If RoleA can be a CombRolePart, this needs adjustment or SettingCChecker needs to be more generic.
                                                                // Given SettingCChecker is Func<SingleRoleBase, bool>, this cast is appropriate.
				}


				if (roleA_Instance == null)
				{
					// This case might happen if RoleA_Id is part of a comb-role and TryGetRole returns the comb-role container
					// or if the player somehow has the ID assigned but the instance cannot be fetched as SingleRoleBase.
					// For now, follow original logic's strict check.
					Logging.Debug($"RoleA instance (as SingleRoleBase) not found or ID mismatch for PlayerId: {playerIdForRoleA} (Expected RoleId: {(ExtremeRoleId)rule.RoleA_Id}).");
					continue;
				}

				bool settingC_IsValid = rule.SettingCChecker(roleA_Instance);
				Logging.Debug($"PlayerId: {playerIdForRoleA}, RoleA_Id: {(ExtremeRoleId)rule.RoleA_Id}, SettingC_IsValid: {settingC_IsValid}");

				if (settingC_IsValid)
				{
					roleAExistsAndSettingCIsValid = true;
					break; // Found at least one instance of RoleA with SettingC valid
				}
			}

			if (!roleAExistsAndSettingCIsValid)
			{
				// No instance of RoleA (with SettingC valid) found for any player.
				continue;
			}

			// Condition: RoleA exists and its SettingC is valid.
			// Now, check if RoleB is NOT assigned to ANY player.
			bool roleB_ExistsInAnyPlayer = currentAssignments.Any(b => GetRoleIdFromAssignment(b) == rule.RoleB_Id);
			Logging.Debug($"RoleB_Id: {(ExtremeRoleId)rule.RoleB_Id} ExistsInAnyPlayer: {roleB_ExistsInAnyPlayer}");

			if (!roleB_ExistsInAnyPlayer) // RoleB does NOT exist
			{
				// Condition MET: RoleA exists, SettingC is valid, AND RoleB does not exist.
				// This means RoleA is an NG role in this context.
				ExtremeRoleId ngRoleId = (ExtremeRoleId)rule.RoleA_Id;
				if (!ngRoleIds.Contains(ngRoleId))
				{
					ngRoleIds.Add(ngRoleId);
					Logging.Debug($"NG Condition MET. RoleA_Id: {ngRoleId} added to NG list because its SettingC is valid and RoleB_Id: {(ExtremeRoleId)rule.RoleB_Id} is not assigned to anyone.");
				}
			}
		}
		Logging.Debug($"--- RoleAssignDependencyChecker.GetNgData END ---");
		return ngRoleIds;
	}

	private int GetRoleIdFromAssignment(IPlayerToExRoleAssignData assignment)
	{
		if (assignment is PlayerToSingleRoleAssignData single)
		{
			return single.RoleId;
		}
		if (assignment is PlayerToCombRoleAssignData comb)
		{
			return comb.RoleId;
		}
		return -1;
	}

	private byte GetPlayerIdFromAssignment(IPlayerToExRoleAssignData assignment)
	{
		if (assignment is PlayerToSingleRoleAssignData single)
		{
			return single.PlayerId;
		}
		if (assignment is PlayerToCombRoleAssignData comb)
		{
			return comb.PlayerId;
		}
		return 0;
	}
}
