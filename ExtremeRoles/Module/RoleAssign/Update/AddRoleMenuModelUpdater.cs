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
        bool result = filter.FilterNormalId.TryAdd(id, roleId);
        if (!result)
        {
            Helper.Logging.Error("Cant Add Role");
        }
    }

    public static void AddRoleData(
        AddRoleMenuModel model, int id, CombinationRoleType roleId)
    {
        var filter = model.Filter;
        bool result = filter.FilterCombinationId.TryAdd(id, roleId);
        if (!result)
        {
            Helper.Logging.Error("Cant Add Role");
        }
    }

    public static void AddRoleData(
        AddRoleMenuModel model, int id, ExtremeGhostRoleId roleId)
    {
        var filter = model.Filter;
        bool result = filter.FilterGhostRole.TryAdd(id, roleId);
        if (!result)
        {
            Helper.Logging.Error("Cant Add Role");
        }
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
