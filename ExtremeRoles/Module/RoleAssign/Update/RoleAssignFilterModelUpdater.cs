using System;

using UnityEngine;

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
