using ExtremeRoles.Roles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.RoleAssign.Model;

namespace ExtremeRoles.Module.RoleAssign.Update;

public static class AddRoleMenuModelUpdater
{
    public static void AddRoleData(
        AddRoleMenuModel model, ExtremeRoleId roleId)
    {
        var filter = model.Filter;
        int id = filter.Id;

        filter.Id += 1;
        filter.FilterNormalId.Add(id, roleId);
    }

    public static void AddRoleData(
        AddRoleMenuModel model, CombinationRoleType roleId)
    {
        var filter = model.Filter;
        int id = filter.Id;

        filter.Id += 1;
        filter.FilterCombinationId.Add(id, roleId);
    }

    public static void AddRoleData(
        AddRoleMenuModel model, ExtremeGhostRoleId roleId)
    {
        var filter = model.Filter;
        int id = filter.Id;

        filter.Id += 1;
        filter.FilterGhostRole.Add(id, roleId);
    }
}
