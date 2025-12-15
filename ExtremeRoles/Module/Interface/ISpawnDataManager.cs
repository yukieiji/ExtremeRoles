using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.CustomOption.OLDS;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using System;
using System.Collections.Generic;

namespace ExtremeRoles.Module.Interface;

public interface ISpawnDataManager
{
	public IReadOnlyDictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>> CurrentSingleRoleSpawnData { get; }

	public IReadOnlyDictionary<byte, CombinationRoleSpawnData> CurrentCombRoleSpawnData { get; }

	public IReadOnlyList<(CombinationRoleType, GhostAndAliveCombinationRoleManagerBase)> UseGhostCombRole { get; }

	public IReadOnlyDictionary<ExtremeRoleType, int> CurrentSingleRoleUseNum { get; }

	protected static int ComputeSpawnNum(
		OptionCategory category,
		RoleSpawnOption minSpawnKey,
		RoleSpawnOption maxSpawnKey)
	{
		int minSpawnNum = category.GetValue<int>((int)minSpawnKey);
		int maxSpawnNum = category.GetValue<int>((int)maxSpawnKey);

		// 最大値が最小値より小さくならないように
		maxSpawnNum = Math.Clamp(maxSpawnNum, minSpawnNum, int.MaxValue);

		return RandomGenerator.Instance.Next(minSpawnNum, maxSpawnNum + 1);
	}

	protected static int ComputePercentage(IOption self)
		=> (int)decimal.Multiply(self.Value<int>(), self.Range);
}
