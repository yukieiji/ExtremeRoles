using System;

using ExtremeRoles.Roles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.RoleAssign.Model;

namespace ExtremeRoles.Module.RoleAssign.Update;

public static class AddRoleMenuModelUpdater
{
    public static void AddRoleData(
        AddRoleMenuModel model, int id, ExtremeRoleId roleId)
    {
        var filter = model.Filter;
        filter.FilterNormalId.Add(id, roleId);
    }

    public static void AddRoleData(
        AddRoleMenuModel model, int id, CombinationRoleType roleId)
    {
        var filter = model.Filter;
        filter.FilterCombinationId.Add(id, roleId);
    }

    public static void AddRoleData(
        AddRoleMenuModel model, int id, ExtremeGhostRoleId roleId)
    {
        var filter = model.Filter;
        filter.FilterGhostRole.Add(id, roleId);
    }

    public static void RemoveFilterRole(
        AddRoleMenuModel model, int targetId)
    {
        var filter = model.Filter;
        filter.FilterNormalId.Remove(targetId);
        filter.FilterCombinationId.Remove(targetId);
        filter.FilterGhostRole.Remove(targetId);
    }
}
