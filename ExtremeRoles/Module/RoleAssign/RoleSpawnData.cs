using System.Collections.Generic;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.RoleAssign;

public class SingleRoleSpawnData(int spawnSetNum, int spawnRate, int weight)
{
	public int SpawnSetNum { get; private set; } = spawnSetNum;

	public int Weight { get; } = weight;
	public int SpawnRate { get; } = spawnRate;

	public const int MaxSpawnRate = 100;

	public void ReduceSpawnNum(int reduceNum = 1)
	{
		this.SpawnSetNum = this.SpawnSetNum - reduceNum;
	}
	public bool IsSpawn()
	{
		return
			this.SpawnSetNum > 0 &&
			this.SpawnRate >= RandomGenerator.Instance.Next(1, MaxSpawnRate + 1);
	}
}

public sealed class CombinationRoleSpawnData(
	CombinationRoleManagerBase role,
	int spawnSetNum, int spawnRate, int weight, bool isMultiAssign) :
	SingleRoleSpawnData(spawnSetNum, spawnRate, weight)
{
	public CombinationRoleManagerBase Role { get; } = role;
	public bool IsMultiAssign { get; } = isMultiAssign;
}

public sealed class GhostRoleSpawnData(
	ExtremeGhostRoleId id, int spawnSetNum,
	int spawnRate, int weight,
	IReadOnlySet<ExtremeRoleId> filter) :
	SingleRoleSpawnData(spawnSetNum, spawnRate, weight)
{
	public ExtremeGhostRoleId Id { get; } = id;

	private readonly IReadOnlySet<ExtremeRoleId> filter = filter;

	public bool IsFilterContain(SingleRoleBase role)
	{
		if (this.filter.Count == 0)
		{
			return true;
		}

		var id = role.Id;

		if (role is MultiAssignRoleBase multiRole &&
			multiRole.AnotherRole is not null)
		{
			return
				this.filter.Contains(id) ||
				this.filter.Contains(multiRole.AnotherRole.Id);
		}
		else
		{
			return this.filter.Contains(id);
		}
	}

}
