using System;

using ExtremeRoles.Module.RoleAssign.Model;

namespace ExtremeRoles.Module.RoleAssign.Update;

public static class RoleAssignFilterModelUpdater
{
    public static void AddFilter(RoleAssignFilterModel model, Guid id)
    {
        model.FilterSet.Add(
            id,
            new RoleFilterSetModel()
            {
                FilterCombinationId = new(),
                FilterGhostRole = new(),
                FilterNormalId = new(),
            });
    }

    public static void RemoveFilter(RoleAssignFilterModel model, Guid targetFilter)
    {
        model.FilterSet.Remove(targetFilter);
    }

    public static void ResetFilter(RoleAssignFilterModel model, Guid targetFilter)
    {
        var filter = model.FilterSet[targetFilter];
        filter.FilterNormalId.Clear();
        filter.FilterCombinationId.Clear();
        filter.FilterGhostRole.Clear();
    }
}
