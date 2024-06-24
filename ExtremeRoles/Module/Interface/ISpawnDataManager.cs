using System;

using ExtremeRoles.GameMode.RoleSelector;


using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.Interface;

public interface ISpawnDataManager
{
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

	protected static int ComputePercentage(IValueOption<int> self)
		=> (int)decimal.Multiply(self.Value, self.Range);
}
