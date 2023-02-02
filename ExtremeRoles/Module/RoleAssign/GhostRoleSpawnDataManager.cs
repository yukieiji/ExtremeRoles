using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.RoleAssign
{
    public sealed class GhostRoleSpawnDataManager : NullableSingleton<GhostRoleSpawnDataManager>
    {
        private Dictionary<ExtremeRoleType, int> globalSpawnLimit = new Dictionary<ExtremeRoleType, int>();

        private Dictionary<ExtremeRoleId, CombinationRoleType> CombRole = new Dictionary<
            ExtremeRoleId, CombinationRoleType>();

        // フィルター、スポーン数、スポーンレート、役職ID
        private Dictionary<ExtremeRoleType, List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)>> useGhostRole = new Dictionary<
            ExtremeRoleType, List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)>>();

        public GhostRoleSpawnDataManager()
        {
            this.Clear();
        }

        public void AddCombRoleAssignData(ExtremeRoleId id, CombinationRoleType type)
        {
            this.CombRole.Add(id, type);
        }

        public void Clear()
        {
            this.globalSpawnLimit.Clear();
            this.useGhostRole.Clear();
            this.CombRole.Clear();
        }

        public CombinationRoleType GetCombRoleType(ExtremeRoleId roleId) => CombRole[roleId];

        public int GetGlobalSpawnLimit(ExtremeRoleType team)
        {
            if (this.globalSpawnLimit.ContainsKey(team))
            {
                return this.globalSpawnLimit[team];
            }
            else
            {
                return int.MinValue;
            }
        }

        public List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)> GetUseGhostRole(
            ExtremeRoleType team)
        {
            if (this.useGhostRole.ContainsKey(team))
            {
                return this.useGhostRole[team].OrderBy(
                    item => RandomGenerator.Instance.Next()).ToList();
            }
            else
            {
                return new List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)>();
            }
        }

        public bool IsCombRole(ExtremeRoleId roleId) => this.CombRole.ContainsKey(roleId);

        public bool IsGlobalSpawnLimit(ExtremeRoleType team)
        {
            bool isGhostRoleArrive = this.globalSpawnLimit.TryGetValue(
                team, out int globalSpawnLimit);

            return isGhostRoleArrive && globalSpawnLimit <= 0;
        }

        public void SetGlobalSpawnLimit(int crewNum, int impNum, int neutralNum)
        {
            this.globalSpawnLimit.Add(ExtremeRoleType.Crewmate, crewNum);
            this.globalSpawnLimit.Add(ExtremeRoleType.Impostor, impNum);
            this.globalSpawnLimit.Add(ExtremeRoleType.Neutral, neutralNum);
        }
        public void SetNormalRoleAssignData(
            ExtremeRoleType team,
            HashSet<ExtremeRoleId> filter,
            int spawnNum,
            int spawnRate,
            ExtremeGhostRoleId ghostRoleId)
        {

            (HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId) addData = (
                filter, spawnNum, spawnRate, ghostRoleId);

            if (!this.useGhostRole.ContainsKey(team))
            {
                List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)> teamGhostRole = new List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)>()
                    {
                        addData,
                    };

                this.useGhostRole.Add(team, teamGhostRole);
            }
            else
            {
                this.useGhostRole[team].Add(addData);
            }
        }

        public void ReduceGlobalSpawnLimit(ExtremeRoleType team)
        {
            this.globalSpawnLimit[team] = this.globalSpawnLimit[team] - 1;
        }

        public void ReduceRoleSpawnData(
            ExtremeRoleType team,
            HashSet<ExtremeRoleId> filter,
            int spawnNum,
            int spawnRate,
            ExtremeGhostRoleId ghostRoleId)
        {
            int index = this.useGhostRole[team].FindIndex(
                x => x == (filter, spawnNum, spawnRate, ghostRoleId));

            this.useGhostRole[team][index] = (filter, spawnNum - 1, spawnRate, ghostRoleId);

        }
    }
}
