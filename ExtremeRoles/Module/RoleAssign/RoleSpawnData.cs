using System;
using System.Collections.Generic;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Module.RoleAssign
{
    public sealed class RoleSpawnData
    {
        public int CrewmateExRoleMaxNum { get; private set; }
        public int NeutralExRoleMaxNum { get; private set; }
        public int ImpostorExRoleMaxNum { get; private set; }

        public Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnSetting>> ExNormalRoleSpawnSetting
        { get; private set; }
        public Dictionary<byte, CombinationRoleSpawnSetting> ExCombRoleSpawnSetting { get; private set; }

        public RoleSpawnData()
        {
            ExNormalRoleSpawnSetting = new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnSetting>>()
            {
                { ExtremeRoleType.Crewmate, new Dictionary<int, SingleRoleSpawnSetting>() },
                { ExtremeRoleType.Impostor, new Dictionary<int, SingleRoleSpawnSetting>() },
                { ExtremeRoleType.Neutral , new Dictionary<int, SingleRoleSpawnSetting>() },
            };
            ExCombRoleSpawnSetting = new Dictionary<byte, CombinationRoleSpawnSetting>();

            var allOption = OptionHolder.AllOption;

            CrewmateExRoleMaxNum = computeSpawnNum(
                OptionHolder.CommonOptionKey.MinCrewmateRoles,
                OptionHolder.CommonOptionKey.MaxCrewmateRoles);
            NeutralExRoleMaxNum = computeSpawnNum(
                OptionHolder.CommonOptionKey.MinNeutralRoles,
                OptionHolder.CommonOptionKey.MaxNeutralRoles);
            ImpostorExRoleMaxNum = computeSpawnNum(
                OptionHolder.CommonOptionKey.MinImpostorRoles,
                OptionHolder.CommonOptionKey.MaxImpostorRoles);


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

                ExCombRoleSpawnSetting.Add(
                    combType,
                    new CombinationRoleSpawnSetting(
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

                ExNormalRoleSpawnSetting[role.Team].Add(
                    intedRoleId, new SingleRoleSpawnSetting(roleNum, spawnRate));
            }

            ExtremeGhostRoleManager.CreateGhostRoleAssignData();
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
