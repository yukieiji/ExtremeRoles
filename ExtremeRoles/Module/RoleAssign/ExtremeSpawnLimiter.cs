using System;
using System.Collections.Generic;

using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class ExtremeSpawnLimiter : ISpawnLimiter
{
	private readonly Dictionary<ExtremeRoleType, int> maxNum;

	public ExtremeSpawnLimiter()
	{
		this.maxNum = OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)SpawnOptionCategory.RoleSpawnCategory, out var cate)
			? new Dictionary<ExtremeRoleType, int>
			{
				{
					ExtremeRoleType.Crewmate,
					ISpawnLimiter.ComputeSpawnNum(
						cate,
						RoleSpawnOption.MinCrewmate,
						RoleSpawnOption.MaxCrewmate)
				},
				{
					ExtremeRoleType.Neutral,
					ISpawnLimiter.ComputeSpawnNum(
						cate,
						RoleSpawnOption.MinNeutral,
						RoleSpawnOption.MaxNeutral)
				},
				{
					ExtremeRoleType.Impostor,
					ISpawnLimiter.ComputeSpawnNum(
						cate,
						RoleSpawnOption.MinImpostor,
						RoleSpawnOption.MaxImpostor)
				},
			} : new();
	}

	public bool CanSpawn(ExtremeRoleType roleType, int spawnNum = 1)
		=> this.maxNum.TryGetValue(roleType, out int maxNum) &&
			maxNum - spawnNum >= 0;

	public int Get(ExtremeRoleType Team)
		=> this.maxNum[Team];

	public void ReduceSpawnLimit(ExtremeRoleType team, int num)
	{
		if (!this.maxNum.TryGetValue(team, out int curNum))
		{
			curNum = 0;
		}
		this.maxNum[team] = curNum - num;
	}
}
