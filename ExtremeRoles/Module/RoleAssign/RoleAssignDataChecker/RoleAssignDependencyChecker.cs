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
		Logging.Debug("--- RoleAssignValidator.IsReBuild START ---");
		bool dataWasActuallyRemoved = false;
		var currentAssignments = data.Assign.Data;

		if (rules.Count == 0)
		{
			Logging.Debug("No dependency rules defined. Skipping validation.");
			Logging.Debug("--- RoleAssignValidator.IsReBuild END (No rules) ---");
			return false;
		}

		// ループ中に prepareData.Assign.RemoveAssignment が呼ばれると currentAssignments の実体が変更されるため、
		// ToList() でコピーを作成してイテレーションする方が安全だが、
		// RemoveAssignment の中で currentAssignments が直接変更されることを期待している。
		// ただし、RoleIdとPlayerIdのペアで削除するので、複数の同一役職持ちプレイヤーがいるケースを考慮し、
		// 削除が発生したらループをやり直すか、慎重なイテレーションが必要。
		// 今回は、一度のIsReBuild呼び出しで複数のルールやプレイヤーにまたがる削除を許容するため、
		// 削除が発生してもループは継続する。ただし、currentAssignments の状態変更には注意。
		// 最も安全なのは、対象をリストアップし、その後まとめて削除・追加処理を行うことだが、
		// 複雑になるため、今回は逐次処理とする。

		foreach (var rule in this.dependencyRules)
		{
			Logging.Debug($"Evaluating Rule: RoleA_Id={(ExtremeRoleId)rule.RoleA_Id}, RoleB_Id={(ExtremeRoleId)rule.RoleB_Id}");
			var assignmentsOfRoleA = currentAssignments
				.Where(a => GetRoleIdFromAssignment(a) == rule.RoleA_Id)
				.Select(a => new { PlayerId = GetPlayerIdFromAssignment(a), RoleId = GetRoleIdFromAssignment(a) })
				.ToList();

			if (!assignmentsOfRoleA.Any())
			{
				Logging.Debug($"No assignments found for RoleA_Id: {(ExtremeRoleId)rule.RoleA_Id}. Skipping to next rule.");
				continue;
			}

			foreach (var itemAInfo in assignmentsOfRoleA)
			{
				Logging.Debug($"Checking PlayerId: {itemAInfo.PlayerId} for RoleA_Id: {(ExtremeRoleId)itemAInfo.RoleId}"); // itemAInfo.RoleId is correct here as it's from the assignment itself
				SingleRoleBase? roleA_Instance = null;
				if (ExtremeRoleManager.TryGetRole(itemAInfo.PlayerId, out var gameRoleInstance) && gameRoleInstance.Id == (ExtremeRoleId)rule.RoleA_Id) // Compare with rule.RoleA_Id for the instance check
				{
					roleA_Instance = gameRoleInstance;
				}

				if (roleA_Instance == null)
				{
					Logging.Debug($"RoleA instance not found or ID mismatch for PlayerId: {itemAInfo.PlayerId} (Expected RoleId: {(ExtremeRoleId)rule.RoleA_Id}).");
					continue;
				}

				bool settingC_IsValid = rule.SettingCChecker(roleA_Instance);
				Logging.Debug($"PlayerId: {itemAInfo.PlayerId}, RoleA_Id: {(ExtremeRoleId)rule.RoleA_Id}, SettingC_IsValid: {settingC_IsValid}");

				if (!settingC_IsValid)
				{
					continue;
				}

				bool roleB_ExistsInAnyPlayer = data.Assign.Data.Any(b => GetRoleIdFromAssignment(b) == rule.RoleB_Id);
				Logging.Debug($"RoleB_Id: {(ExtremeRoleId)rule.RoleB_Id} ExistsInAnyPlayer: {roleB_ExistsInAnyPlayer}");

				if (roleB_ExistsInAnyPlayer)
				{
					continue;
				}

				Logging.Debug($"Condition MET for PlayerId: {itemAInfo.PlayerId}, RoleA_Id: {(ExtremeRoleId)rule.RoleA_Id}. Attempting removal.");
				bool removedThisTime = data.Assign.RemoveAssignment(itemAInfo.PlayerId, rule.RoleA_Id);

				if (!removedThisTime)
				{
					Logging.Debug($"Failed to remove RoleA_Id: {(ExtremeRoleId)rule.RoleA_Id} for PlayerId: {itemAInfo.PlayerId}.");
					continue;
				}

				Logging.Debug($"SUCCESS: Removed RoleA_Id: {(ExtremeRoleId)rule.RoleA_Id} for PlayerId: {itemAInfo.PlayerId}");
				dataWasActuallyRemoved = true;

				PlayerControl? playerControlToRequeue = PlayerCache.AllPlayerControl.FirstOrDefault(pc => pc.PlayerId == itemAInfo.PlayerId);
				if (playerControlToRequeue != null)
				{
					prepareData.Assign.AddPlayerToReassign(playerControlToRequeue);
					Logging.Debug($"PlayerId: {itemAInfo.PlayerId} added back to re-assign queue.");
				}

				bool teamProcessedForLimit = false;
				ExtremeRoleType teamToAdjust = ExtremeRoleType.Null; // For logging

				if (ExtremeRoleManager.NormalRole.TryGetValue(rule.RoleA_Id, out var roleADefinition))
				{
					teamToAdjust = roleADefinition.Team;
					prepareData.Limit.Reduce(teamToAdjust, -1);
					teamProcessedForLimit = true;
				}
				else
				{
					ExtremeRoleId targetCombRoleId = (ExtremeRoleId)rule.RoleA_Id;
					foreach (var combManager in ExtremeRoleManager.CombRole.Values)
					{
						var foundCombRolePart = combManager.Roles.FirstOrDefault(r => r.Id == targetCombRoleId);
						if (foundCombRolePart != null)
						{
							teamToAdjust = foundCombRolePart.Team;
							prepareData.Limit.Reduce(teamToAdjust, -1);
							teamProcessedForLimit = true;
							break;
						}
					}
				}

				if (teamProcessedForLimit)
				{
					Logging.Debug($"Spawn limit for Team: {teamToAdjust} increased by 1 due to removal of RoleId: {(ExtremeRoleId)rule.RoleA_Id}.");
				}
				else
				{
					Logging.Debug($"WARNING: Could not find team for RoleId: {(ExtremeRoleId)rule.RoleA_Id} to adjust spawn limit.");
				}
			}
		}
		Logging.Debug($"--- RoleAssignValidator.IsReBuild END (dataWasActuallyRemoved: {dataWasActuallyRemoved}) ---");
		return dataWasActuallyRemoved;
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
