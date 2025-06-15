using System;
using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;


namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataChecker;

public class RoleDependencyRule
{
	public int RoleA_Id { get; }
	public int RoleB_Id { get; }
	public bool SettingCChecker { get; }

	public RoleDependencyRule(ExtremeRoleId roleA, ExtremeRoleId roleB, bool settingCChecker)
	{
		RoleA_Id = (int)roleA;
		RoleB_Id = (int)roleB;
		SettingCChecker = settingCChecker;
	}
}

public sealed class RoleDependencyRuleFactory : IRoleDependencyRuleFactory
{
	public IReadOnlyList<RoleDependencyRule> Rules { get; } = new List<RoleDependencyRule>();
}

public sealed class RoleAssignDependencyChecker(IRoleDependencyRuleFactory factory) : IRoleAssignDataChecker
{
	private readonly IReadOnlyList<RoleDependencyRule> rules = factory.Rules;

	public IReadOnlySet<ExtremeRoleId> GetNgData(in PreparationData data)
	{
		var ngRoleIds = new HashSet<ExtremeRoleId>();
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
				.Where(a => GetRoleIdFromAssignment(a) == rule.RoleA_Id); // ToList is important if currentAssignments can change, but here we only read.

			if (assignmentsOfRoleA.Any())
			{
				Logging.Debug($"No assignments found for RoleA_Id: {(ExtremeRoleId)rule.RoleA_Id}.");
				continue;
			}

			bool roleAExistsAndSettingCIsValid = false;
			foreach (var assignmentA in assignmentsOfRoleA.ToArray())
			{
				bool settingC_IsValid = rule.SettingCChecker;
				Logging.Debug($"RoleA_Id: {(ExtremeRoleId)rule.RoleA_Id}, SettingC_IsValid: {settingC_IsValid}");

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
				ngRoleIds.Add(ngRoleId);
				Logging.Debug($"NG Condition MET. RoleA_Id: {ngRoleId} added to NG list because its SettingC is valid and RoleB_Id: {(ExtremeRoleId)rule.RoleB_Id} is not assigned to anyone.");
			}
		}
		Logging.Debug($"--- RoleAssignDependencyChecker.GetNgData END ---");
		return ngRoleIds;
	}

	private static int GetRoleIdFromAssignment(IPlayerToExRoleAssignData assignment)
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
}
