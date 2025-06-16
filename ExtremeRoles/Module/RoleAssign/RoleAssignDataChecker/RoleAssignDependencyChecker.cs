using System;
using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;


namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataChecker;

public sealed record RoleDependencyRule(ExtremeRoleId CheckRoleId, ExtremeRoleId DependRoleId, Func<bool> isDepend)
{
	private readonly Func<bool> isDependCheck = isDepend;
	public bool IsDepend => isDependCheck.Invoke();
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
			var checkRole = rule.CheckRoleId;
			var dependRole = rule.DependRoleId;

			if (checkRole == dependRole)
			{
				Logging.Debug("Invalid Rule: same roles");
				continue;
			}

			Logging.Debug($"Evaluating Rule: Check={checkRole}, Depend={dependRole}");

			// Find all players assigned to RoleA_Id
			var assignmentsOfRole = currentAssignments
				.Where(a => GetRoleIdFromAssignment(a) == (int)checkRole); // ToList is important if currentAssignments can change, but here we only read.

			if (assignmentsOfRole.Any())
			{
				Logging.Debug($"No assignments found for RoleId:{checkRole}.");
				continue;
			}

			bool roleAExistsAndSettingCIsValid = false;
			foreach (var assignmentA in assignmentsOfRole.ToArray())
			{
				bool isDepend = rule.IsDepend;
				Logging.Debug($"RoleId: {checkRole}, DependNow: {isDepend}");

				if (isDepend)
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
			bool dependRoleAssignedToAnyPlayer = currentAssignments.Any(b => GetRoleIdFromAssignment(b) == (int)dependRole);
			Logging.Debug($"DependRole: {dependRole} ExistsInAnyPlayer: {dependRoleAssignedToAnyPlayer}");

			if (!dependRoleAssignedToAnyPlayer) // RoleB does NOT exist
			{
				// Condition MET: RoleA exists, SettingC is valid, AND RoleB does not exist.
				// This means RoleA is an NG role in this context.
				ngRoleIds.Add(checkRole);
				Logging.Debug($"NG Condition MET. CheckRole: {checkRole} added to NG list because its SettingC is valid and RoleB_Id: {dependRole} is not assigned to anyone.");
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
