using System.Collections.Generic;
using System.Linq;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class GhostRoleSpawnDataManager : 
    NullableSingleton<GhostRoleSpawnDataManager>,
    ISpawnDataManager
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
                ISpawnDataManager.ComputeSpawnNum(
                    RoleGlobalOption.MinCrewmateGhostRoles,
                    RoleGlobalOption.MaxCrewmateGhostRoles)
            },
            {
                ExtremeRoleType.Neutral,
                ISpawnDataManager.ComputeSpawnNum(
                    RoleGlobalOption.MinNeutralGhostRoles,
                    RoleGlobalOption.MaxNeutralGhostRoles)
            },
            {
                ExtremeRoleType.Impostor,
                ISpawnDataManager.ComputeSpawnNum(
                    RoleGlobalOption.MinImpostorGhostRoles,
                    RoleGlobalOption.MaxImpostorGhostRoles)
            },
        };

        var allOption = OptionHolder.AllOption;
        var tmpUseData = new Dictionary<ExtremeRoleType, List<GhostRoleSpawnData>>();

        foreach (ExtremeGhostRoleId roleId in 
            ExtremeGameModeManager.Instance.RoleSelector.UseGhostRoleId)
        {
            var role = ExtremeGhostRoleManager.AllGhostRole[roleId];

            int spawnRate = ISpawnDataManager.ComputePercentage(
                allOption[role.GetRoleOptionId(RoleCommonOption.SpawnRate)]);
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

            if (!tmpUseData.ContainsKey(team))
            {
                List<GhostRoleSpawnData> teamGhostRole = new List<GhostRoleSpawnData>()
                {
                    addData,
                };

                tmpUseData.Add(team, teamGhostRole);
            }
            else
            {
                tmpUseData[team].Add(addData);
            }
        }

        foreach (var (team, spawnDataList) in tmpUseData)
        {
            Helper.Logging.Debug($"Add {team} ghost role spawn data");
            this.useGhostRole[team] = spawnDataList.OrderBy(
                x => RandomGenerator.Instance.Next()).ToList();
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
}
