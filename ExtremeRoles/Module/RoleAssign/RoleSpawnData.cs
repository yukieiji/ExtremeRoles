using System.Collections.Generic;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.RoleAssign;

public abstract class SpawnData
{
    public int SpawnSetNum { get; protected set; }
    public int SpawnRate { get; protected set; }

    public void ReduceSpawnNum(int reduceNum = 1)
    {
        this.SpawnSetNum = this.SpawnSetNum - reduceNum;
    }
    public bool IsSpawn()
    {
        return
            this.SpawnSetNum > 0 &&
            this.SpawnRate >= RandomGenerator.Instance.Next(0, 110);
    }
}

public sealed class SingleRoleSpawnData : SpawnData
{
    public SingleRoleSpawnData(int spawnSetNum, int spawnRate)
    {
        SpawnSetNum = spawnSetNum;
        SpawnRate = spawnRate;
    }
}

public sealed class CombinationRoleSpawnData : SpawnData
{
    public CombinationRoleManagerBase Role { get; private set; }
    public bool IsMultiAssign { get; private set; }

    public CombinationRoleSpawnData(
        CombinationRoleManagerBase role,
        int spawnSetNum, int spawnRate, bool isMultiAssign)
    {
        Role = role;
        SpawnSetNum = spawnSetNum;
        SpawnRate = spawnRate;
        IsMultiAssign = isMultiAssign;
    }
}

public sealed class GhostRoleSpawnData : SpawnData
{
    public ExtremeGhostRoleId Id { get; private set; }

    private HashSet<ExtremeRoleId> filter;

    public GhostRoleSpawnData(
        ExtremeGhostRoleId id, int spawnSetNum,
        int spawnRate, HashSet<ExtremeRoleId> filter)
    {
        Id = id;
        SpawnSetNum = spawnSetNum;
        SpawnRate = spawnRate;
        this.filter = filter;
    }

    public bool IsBlock(SingleRoleBase role)
    {
        if (this.filter.Count == 0)
        {
            return false;
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
            return this.filter.Contains(role.Id);
        }
    }
        
}
