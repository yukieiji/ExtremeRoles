using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomOption.Interfaces;
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
		IOptionLoader loader,
		RoleSpawnOption minSpawnKey,
		RoleSpawnOption maxSpawnKey)
	{
		int minSpawnNum = loader.GetValue<int>((int)minSpawnKey);
		int maxSpawnNum = loader.GetValue<int>((int)maxSpawnKey);

		// 最大値が最小値より小さくならないように
		maxSpawnNum = Math.Clamp(maxSpawnNum, minSpawnNum, int.MaxValue);

		return RandomGenerator.Instance.Next(minSpawnNum, maxSpawnNum + 1);
	}

	protected static int ComputePercentage(IValueOption<int> self)
		=> (int)decimal.Multiply(self.Value, self.Range);
}
