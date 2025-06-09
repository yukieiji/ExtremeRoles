using System.Text;
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

		if (!OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)SpawnOptionCategory.RoleSpawnCategory, out var cate))
		{
			this.maxNum = [];
			return;
		}

		this.maxNum = new Dictionary<ExtremeRoleType, int>(3)
		{
			{
				ExtremeRoleType.Crewmate,
				ISpawnLimiter.ComputeSpawnNum(
					cate,
					RoleSpawnOption.MinCrewmate,
					RoleSpawnOption.MaxCrewmate)
			},
			{
				ExtremeRoleType.Impostor,
				ISpawnLimiter.ComputeSpawnNum(
					cate,
					RoleSpawnOption.MinImpostor,
					RoleSpawnOption.MaxImpostor)
			},
			{
				ExtremeRoleType.Neutral,
				ISpawnLimiter.ComputeSpawnNum(
					cate,
					RoleSpawnOption.MinNeutral,
					RoleSpawnOption.MaxNeutral)
			},
		};
	}

	public override string ToString()
	{
		var builder = new StringBuilder();

		builder.AppendLine("------ Spawn Limit ------");
		foreach (var (team, num) in this.maxNum)
		{
			builder.AppendLine($"Team:{team} MaxNum:{num}");
		}
		builder.Append("------------");

		return builder.ToString();
	}

	public bool CanSpawn(ExtremeRoleType roleType, int spawnNum = 1)
		=> this.maxNum.TryGetValue(roleType, out int maxNum) &&
			maxNum - spawnNum >= 0;

	public int Get(ExtremeRoleType Team)
		=> this.maxNum[Team];

	public void Reduce(ExtremeRoleType team, int num = 1)
	{
		if (!this.maxNum.TryGetValue(team, out int curNum))
		{
			curNum = 0;
		}
		this.maxNum[team] = curNum - num;
	}
}
