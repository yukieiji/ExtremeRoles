using System.Collections.Generic;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class RoleSpawnDataManager : ISpawnDataManager
{
    public Dictionary<ExtremeRoleType, int> MaxRoleNum { get; private set; }

    public Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>> CurrentSingleRoleSpawnData
    { get; private set; }
    
    public Dictionary<byte, CombinationRoleSpawnData> CurrentCombRoleSpawnData 
    { get; private set; }

    public List<(CombinationRoleType, GhostAndAliveCombinationRoleManagerBase)> UseGhostCombRole 
    { get; private set; }

    public Dictionary<ExtremeRoleType, int> CurrentSingleRoleUseNum 
    { get; private set; }

    public RoleSpawnDataManager()
    {
        UseGhostCombRole = new List<(CombinationRoleType, GhostAndAliveCombinationRoleManagerBase)>();
        CurrentCombRoleSpawnData = new Dictionary<byte, CombinationRoleSpawnData>();

        CurrentSingleRoleSpawnData = new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>
        {
            { ExtremeRoleType.Crewmate, new Dictionary<int, SingleRoleSpawnData>() },
            { ExtremeRoleType.Impostor, new Dictionary<int, SingleRoleSpawnData>() },
            { ExtremeRoleType.Neutral , new Dictionary<int, SingleRoleSpawnData>() },
        };

        MaxRoleNum = new Dictionary<ExtremeRoleType, int>
        {
            {
                ExtremeRoleType.Crewmate,
                ISpawnDataManager.ComputeSpawnNum(
                    RoleGlobalOption.MinCrewmateRoles,
                    RoleGlobalOption.MaxCrewmateRoles)
            },
            {
                ExtremeRoleType.Neutral,
                ISpawnDataManager.ComputeSpawnNum(
                    RoleGlobalOption.MinNeutralRoles,
                    RoleGlobalOption.MaxNeutralRoles)
            },
            {
                ExtremeRoleType.Impostor,
                ISpawnDataManager.ComputeSpawnNum(
                    RoleGlobalOption.MinImpostorRoles,
                    RoleGlobalOption.MaxImpostorRoles)
            },
        };

        CurrentSingleRoleUseNum = new Dictionary<ExtremeRoleType, int>()
        {
            { ExtremeRoleType.Crewmate, 0 },
            { ExtremeRoleType.Impostor, 0 },
            { ExtremeRoleType.Neutral , 0 },
        };

        var allOption = OptionManager.Instance;

        foreach (var roleId in ExtremeGameModeManager.Instance.RoleSelector.UseCombRoleType)
        {
            byte combType = (byte)roleId;
            var role = ExtremeRoleManager.CombRole[combType];
            int spawnRate = ISpawnDataManager.ComputePercentage(allOption.Get<int>(
                role.GetRoleOptionId(RoleCommonOption.SpawnRate),
                OptionManager.ValueType.Int));
            int roleSet = allOption.GetValue<int>(
                role.GetRoleOptionId(RoleCommonOption.RoleNum));
            int weight = allOption.GetValue<int>(
                role.GetRoleOptionId(RoleCommonOption.AssignWeight));
            bool isMultiAssign = allOption.GetValue<bool>(
                role.GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign));

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
                    weight: weight,
                    isMultiAssign: isMultiAssign));

            if (role is GhostAndAliveCombinationRoleManagerBase ghostComb)
            {
                this.UseGhostCombRole.Add(((CombinationRoleType)combType, ghostComb));
            }
        }

        foreach (var roleId in ExtremeGameModeManager.Instance.RoleSelector.UseNormalRoleId)
        {
            int intedRoleId = (int)roleId;
            SingleRoleBase role = ExtremeRoleManager.NormalRole[intedRoleId];

            int spawnRate = ISpawnDataManager.ComputePercentage(allOption.Get<int>(
                role.GetRoleOptionId(RoleCommonOption.SpawnRate),
                OptionManager.ValueType.Int));
            int weight = allOption.GetValue<int>(
                role.GetRoleOptionId(RoleCommonOption.AssignWeight));
            int roleNum = allOption.GetValue<int>(
                role.GetRoleOptionId(RoleCommonOption.RoleNum));

            Logging.Debug(
                $"Role Name:{role.RoleName}  SpawnRate:{spawnRate}   RoleNum:{roleNum}");

            if (roleNum <= 0 || spawnRate <= 0.0)
            {
                continue;
            }

            CurrentSingleRoleSpawnData[role.Team].Add(
                intedRoleId, new SingleRoleSpawnData(roleNum, spawnRate, weight));
            CurrentSingleRoleUseNum[role.Team] += roleNum;
        }
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
}
