using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.RoleAssign
{
    public sealed class SingleRoleSpawnData
    {
        public int SpawnSetNum { get; private set; }
        public int SpawnRate { get; private set; }

        public SingleRoleSpawnData(int spawnSetNum, int spawnRate)
        {
            SpawnSetNum = spawnSetNum;
            SpawnRate = spawnRate;
        }
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

    public sealed class CombinationRoleSpawnData
    {
        public CombinationRoleManagerBase Role { get; private set; }
        public int SpawnSetNum { get; private set; }
        public int SpawnRate { get; private set; }
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
}
