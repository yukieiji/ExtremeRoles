using System;
using System.Collections.Generic;
using System.Linq;
using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.RoleAssign
{
    public sealed class GhostRoleSpawnDataManager : NullableSingleton<GhostRoleSpawnDataManager>
    {
        private Dictionary<ExtremeRoleType, int> globalSpawnLimit = new Dictionary<ExtremeRoleType, int>();

        private Dictionary<ExtremeRoleId, CombinationRoleType> combRole = new Dictionary<
            ExtremeRoleId, CombinationRoleType>();

        private Dictionary<ExtremeRoleType, List<GhostRoleSpawnData>> useGhostRole = new Dictionary<
            ExtremeRoleType, List<GhostRoleSpawnData>>();

        public GhostRoleSpawnDataManager()
        {
            this.clear();
        }

        public void Create(
            List<(CombinationRoleType, GhostAndAliveCombinationRoleManagerBase)> useGhostCombRole)
        {
            this.clear();

            foreach (var (combRoleId, mng) in useGhostCombRole)
            {
                foreach (ExtremeRoleId roleId in mng.CombGhostRole.Keys)
                {
                    this.combRole.Add(roleId, combRoleId);
                }
            }

            this.globalSpawnLimit = new Dictionary<ExtremeRoleType, int>
            {
                {
                    ExtremeRoleType.Crewmate,
                    computeSpawnNum(
                        OptionHolder.CommonOptionKey.MinCrewmateGhostRoles,
                        OptionHolder.CommonOptionKey.MaxCrewmateGhostRoles)
                },
                {
                    ExtremeRoleType.Neutral,
                    computeSpawnNum(
                        OptionHolder.CommonOptionKey.MinNeutralGhostRoles,
                        OptionHolder.CommonOptionKey.MaxNeutralGhostRoles)
                },
                {
                    ExtremeRoleType.Impostor,
                    computeSpawnNum(
                        OptionHolder.CommonOptionKey.MinImpostorGhostRoles,
                        OptionHolder.CommonOptionKey.MaxImpostorGhostRoles)
                },
            };

            var allOption = OptionHolder.AllOption;

            foreach (ExtremeGhostRoleId roleId in 
                ExtremeGameModeManager.Instance.RoleSelector.UseGhostRoleId)
            {
                var role = ExtremeGhostRoleManager.AllGhostRole[roleId];

                int spawnRate = computePercentage(allOption[
                    role.GetRoleOptionId(RoleCommonOption.SpawnRate)]);
                int roleNum = allOption[
                    role.GetRoleOptionId(RoleCommonOption.RoleNum)].GetValue();

                Helper.Logging.Debug(
                    $"GhostRole Name:{role.Name}  SpawnRate:{spawnRate}   RoleNum:{roleNum}");

                if (roleNum <= 0 || spawnRate <= 0.0)
                {
                    continue;
                }

                var addData = new GhostRoleSpawnData(
                    roleId, roleNum, spawnRate, role.GetRoleFilter());

                ExtremeRoleType team = role.Team;

                if (!this.useGhostRole.ContainsKey(team))
                {
                    List<GhostRoleSpawnData> teamGhostRole = new List<GhostRoleSpawnData>()
                    {
                        addData,
                    };

                    this.useGhostRole.Add(team, teamGhostRole);
                }
                else
                {
                    this.useGhostRole[team].Add(addData);
                };
            }

        }

        public CombinationRoleType GetCombRoleType(ExtremeRoleId roleId) => 
            this.combRole[roleId];

        public int GetGlobalSpawnLimit(ExtremeRoleType team)
        {
            if (this.globalSpawnLimit.TryGetValue(team, out int limit))
            {
                return limit;
            }
            else
            {
                return int.MinValue;
            }
        }

        public List<GhostRoleSpawnData> GetUseGhostRole(
            ExtremeRoleType team)
        {
            this.useGhostRole.TryGetValue(team, out List<GhostRoleSpawnData> data);
            return data;
        }

        public bool IsCombRole(ExtremeRoleId roleId) => this.combRole.ContainsKey(roleId);

        public bool IsGlobalSpawnLimit(ExtremeRoleType team)
        {
            bool isGhostRoleArrive = this.globalSpawnLimit.TryGetValue(
                team, out int globalSpawnLimit);

            return isGhostRoleArrive && globalSpawnLimit <= 0;
        }

        public void ReduceGlobalSpawnLimit(ExtremeRoleType team)
        {
            this.globalSpawnLimit[team] = this.globalSpawnLimit[team] - 1;
        }

        private void clear()
        {
            this.globalSpawnLimit.Clear();
            this.useGhostRole.Clear();
            this.combRole.Clear();
        }

        private static int computeSpawnNum(
            OptionHolder.CommonOptionKey minSpawnKey,
            OptionHolder.CommonOptionKey maxSpawnKey)
        {
            var allOption = OptionHolder.AllOption;

            int minSpawnNum = allOption[(int)minSpawnKey].GetValue();
            int maxSpawnNum = allOption[(int)maxSpawnKey].GetValue();

            // 最大値が最小値より小さくならないように
            maxSpawnNum = Math.Clamp(maxSpawnNum, minSpawnNum, int.MaxValue);

            return RandomGenerator.Instance.Next(minSpawnNum, maxSpawnNum + 1);
        }

        private static int computePercentage(IOption self)
            => (int)decimal.Multiply(self.GetValue(), self.ValueCount);
    }
}
