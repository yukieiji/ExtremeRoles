using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;

using System;

namespace ExtremeRoles.Module.Interface;

public interface ISpawnLimiter
{
	public int Get(ExtremeRoleType Team);
	public bool CanSpawn(ExtremeRoleType roleType, int spawnNum = 1);
	public void Reduce(ExtremeRoleType Team, int num = 1);

	public static int ComputeSpawnNum(
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
