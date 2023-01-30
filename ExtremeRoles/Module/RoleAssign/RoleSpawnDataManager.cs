using System;
using System.Collections.Generic;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Module.RoleAssign
{
    public sealed class RoleSpawnDataManager
    {
        public Dictionary<ExtremeRoleType, int> MaxRoleNum { get; private set; }

        public Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>> CurrentSingleRoleSpawnData
        { get; private set; }
        public Dictionary<byte, CombinationRoleSpawnData> CurrentCombRoleSpawnData { get; private set; }

        public RoleSpawnDataManager()
        {
            CurrentSingleRoleSpawnData = new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>
            {
                { ExtremeRoleType.Crewmate, new Dictionary<int, SingleRoleSpawnData>() },
                { ExtremeRoleType.Impostor, new Dictionary<int, SingleRoleSpawnData>() },
                { ExtremeRoleType.Neutral , new Dictionary<int, SingleRoleSpawnData>() },
            };
            CurrentCombRoleSpawnData = new Dictionary<byte, CombinationRoleSpawnData>();

            MaxRoleNum = new Dictionary<ExtremeRoleType, int>
            {
                {
                    ExtremeRoleType.Crewmate,
                    computeSpawnNum(
                        OptionHolder.CommonOptionKey.MinCrewmateRoles,
                        OptionHolder.CommonOptionKey.MaxCrewmateRoles)
                },
                {
                    ExtremeRoleType.Neutral,
                    computeSpawnNum(
                        OptionHolder.CommonOptionKey.MinNeutralRoles,
                        OptionHolder.CommonOptionKey.MaxNeutralRoles)
                },
                {
                    ExtremeRoleType.Impostor,
                    computeSpawnNum(
                        OptionHolder.CommonOptionKey.MinImpostorRoles,
                        OptionHolder.CommonOptionKey.MaxImpostorRoles)
                },
            };

            var allOption = OptionHolder.AllOption;

            foreach (var roleId in ExtremeGameModeManager.Instance.RoleSelector.UseCombRoleType)
            {
                byte combType = (byte)roleId;
                var role = ExtremeRoleManager.CombRole[combType];
                int spawnRate = computePercentage(allOption[
                    role.GetRoleOptionId(RoleCommonOption.SpawnRate)]);
                int roleSet = allOption[
                    role.GetRoleOptionId(RoleCommonOption.RoleNum)].GetValue();
                bool isMultiAssign = allOption[
                    role.GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign)].GetValue();

                Logging.Debug($"Role:{role}    SpawnRate:{spawnRate}   RoleSet:{roleSet}");

                if (roleSet <= 0 || spawnRate <= 0.0)
                {
                    continue;
                }

                CurrentCombRoleSpawnData.Add(
                    combType,
                    new CombinationRoleSpawnData(
                        role: role,
                        spawnSetNum: roleSet,
                        spawnRate: spawnRate,
                        isMultiAssign: isMultiAssign));

                var ghostComb = role as GhostAndAliveCombinationRoleManagerBase;
                if (ghostComb != null)
                {
                    ExtremeGhostRoleManager.AddCombGhostRole(
                        (CombinationRoleType)combType, ghostComb);
                }
            }

            foreach (var roleId in ExtremeGameModeManager.Instance.RoleSelector.UseNormalRoleId)
            {
                int intedRoleId = (int)roleId;
                SingleRoleBase role = ExtremeRoleManager.NormalRole[intedRoleId];

                int spawnRate = computePercentage(allOption[
                    role.GetRoleOptionId(RoleCommonOption.SpawnRate)]);
                int roleNum = allOption[
                    role.GetRoleOptionId(RoleCommonOption.RoleNum)].GetValue();

                Logging.Debug(
                    $"Role Name:{role.RoleName}  SpawnRate:{spawnRate}   RoleNum:{roleNum}");

                if (roleNum <= 0 || spawnRate <= 0.0)
                {
                    continue;
                }

                CurrentSingleRoleSpawnData[role.Team].Add(
                    intedRoleId, new SingleRoleSpawnData(roleNum, spawnRate));
            }

            ExtremeGhostRoleManager.CreateGhostRoleAssignData();
        }

        public bool IsCanSpawnTeam(ExtremeRoleType roleType, int reduceNum = 1)
        {
            return
                this.MaxRoleNum.TryGetValue(roleType, out int maxNum) &&
                maxNum - reduceNum >= 0;
        }

        public void ReduceSpawnLimit(ExtremeRoleType roleType, int reduceNum = 1)
        {
            this.MaxRoleNum[roleType] = this.MaxRoleNum[roleType] - reduceNum;
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
