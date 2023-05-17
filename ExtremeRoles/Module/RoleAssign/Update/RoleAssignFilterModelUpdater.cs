using System;

using UnityEngine;

using ExtremeRoles.Roles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.RoleAssign.Model;

namespace ExtremeRoles.Module.RoleAssign.Update;

public static class RoleAssignFilterModelUpdater
{
    public static void AddFilter(RoleAssignFilterModel model, Guid id)
    {
        model.FilterSet.Add(
            id,
            new RoleFilterData()
            {
                FilterCombinationId = new(),
                FilterGhostRole = new(),
                FilterNormalId = new(),
            });
    }

    public static void AddRoleData(
        RoleAssignFilterModel model, Guid targetFilter, int id, ExtremeRoleId roleId)
    {
        var filter = model.FilterSet[targetFilter];
        bool result = filter.FilterNormalId.TryAdd(id, roleId);
        
        if (result)
        {
            updateConfigValue(model);
        }
        else
        {
            Helper.Logging.Error("Cant Add Role");
        }
    }

    public static void AddRoleData(
        RoleAssignFilterModel model, Guid targetFilter, int id, CombinationRoleType roleId)
    {
        var filter = model.FilterSet[targetFilter];
        bool result = filter.FilterCombinationId.TryAdd(id, roleId);
        
        if (result)
        {
            updateConfigValue(model);
        }
        else
        {
            Helper.Logging.Error("Cant Add Role");
        }
    }

    public static void AddRoleData(
        RoleAssignFilterModel model, Guid targetFilter, int id, ExtremeGhostRoleId roleId)
    {
        var filter = model.FilterSet[targetFilter];
        bool result = filter.FilterGhostRole.TryAdd(id, roleId);
        
        if (result)
        {
            updateConfigValue(model);
        }
        else
        {
            Helper.Logging.Error("Cant Add Role");
        }
    }

    public static void RemoveFilterRole(
        RoleAssignFilterModel model, Guid targetFilter, int targetId)
    {
        var filter = model.FilterSet[targetFilter];
        filter.FilterNormalId.Remove(targetId);
        filter.FilterCombinationId.Remove(targetId);
        filter.FilterGhostRole.Remove(targetId);

        updateConfigValue(model);
    }

    public static void IncreseFilterAssignNum(RoleAssignFilterModel model, Guid targetFilter)
    {
        var filter = model.FilterSet[targetFilter];
        int curNum = filter.AssignNum;
        filter.AssignNum = Mathf.Clamp(curNum + 1, 1, int.MaxValue);
        
        if (filter.FilterCombinationId.Count > 0 ||
            filter.FilterNormalId.Count > 0 ||
            filter.FilterGhostRole.Count > 0)
        {
            updateConfigValue(model);
        }
    }

    public static void DecreseFilterAssignNum(RoleAssignFilterModel model, Guid targetFilter)
    {
        var filter = model.FilterSet[targetFilter];
        int curNum = filter.AssignNum;
        filter.AssignNum = Mathf.Clamp(curNum - 1, 1, int.MaxValue);
        
        if (filter.FilterCombinationId.Count > 0 ||
            filter.FilterNormalId.Count > 0 ||
            filter.FilterGhostRole.Count > 0)
        {
            updateConfigValue(model);
        }
    }

    public static void RemoveFilter(RoleAssignFilterModel model, Guid targetFilter)
    {
        model.FilterSet.Remove(targetFilter);
        
        updateConfigValue(model);
    }

    public static void ResetFilter(RoleAssignFilterModel model, Guid targetFilter)
    {
        var filter = model.FilterSet[targetFilter];
        filter.FilterNormalId.Clear();
        filter.FilterCombinationId.Clear();
        filter.FilterGhostRole.Clear();

        updateConfigValue(model);
    }

    private static void updateConfigValue(RoleAssignFilterModel model)
    {
        model.Config.Value = model.SerializeToString();
    }
}
