using System.Text;
using System.Collections.Generic;

using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class ExtremeSpawnLimiter : ISpawnLimiter
{
	private readonly Dictionary<ExtremeRoleType, int> maxNum = new Dictionary<ExtremeRoleType, int>(3);

	public override string ToString()
	{
		var builder = new StringBuilder();

		builder.AppendLine("------ SpawnLimit ------");
		foreach (var (team, num) in this.maxNum)
		{
			builder.AppendLine($"Team:{team} MaxNum:{num}");
		}
		builder.AppendLine("------------");

		return builder.ToString();
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

	public void Initialize()
	{
		this.maxNum.Clear();

		if (!OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)SpawnOptionCategory.RoleSpawnCategory, out var cate))
		{
			return;
		}

		this.maxNum[ExtremeRoleType.Crewmate] = ISpawnLimiter.ComputeSpawnNum(
			cate,
			RoleSpawnOption.MinCrewmate,
			RoleSpawnOption.MaxCrewmate);
		this.maxNum[ExtremeRoleType.Impostor] = ISpawnLimiter.ComputeSpawnNum(
			cate,
			RoleSpawnOption.MinImpostor,
			RoleSpawnOption.MaxImpostor);
		this.maxNum[ExtremeRoleType.Null] = ISpawnLimiter.ComputeSpawnNum(
			cate,
			RoleSpawnOption.MinNeutral,
			RoleSpawnOption.MaxNeutral);
	}
}
