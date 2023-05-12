using ExtremeRoles.Roles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.RoleAssign.Model;

namespace ExtremeRoles.Module.RoleAssign;

public static class RoleAssignFilterModelUpdater
{
	public static void AddFilter(RoleAssignFilterModel model)
	{
		model.FilterSet.Add(
            model.FilterId,
			new RoleFilterSetModel()
			{
				Id = 0,
				FilterCombinationId = new(),
				FilterGhostRole = new(),
				FilterNormalId = new(),
			});
        model.FilterId += 1;
    }

    public static void AddRoleData(
		RoleAssignFilterModel model, int targetFilter, ExtremeRoleId roleId)
	{
		var filter = model.FilterSet[targetFilter];
        int id = filter.Id;

        filter.Id += 1;
        filter.FilterNormalId.Add(id, roleId);
	}

	public static void AddRoleData(
		RoleAssignFilterModel model, int targetFilter, CombinationRoleType roleId)
	{
        var filter = model.FilterSet[targetFilter];
        int id = filter.Id;

        filter.Id += 1;
        filter.FilterCombinationId.Add(id, roleId);
    }

	public static void AddRoleData(
		RoleAssignFilterModel model, int targetFilter, ExtremeGhostRoleId roleId)
	{
        var filter = model.FilterSet[targetFilter];
        int id = filter.Id;

        filter.Id += 1;
        filter.FilterGhostRole.Add(id, roleId);
    }

    public static void ConvertModelToAssignFilter(
        RoleAssignFilterModel model, RoleAssignFilter filter)
    {
        foreach (var filterModel in model.FilterSet.Values)
        {
            var filterSet = new RoleFilterSet();

            foreach (var extremeRoleId in filterModel.FilterNormalId.Values)
            {
                filterSet.Add(extremeRoleId);
            }
            foreach (var extremeRoleId in filterModel.FilterCombinationId.Values)
            {
                filterSet.Add(extremeRoleId);
            }
            foreach (var extremeRoleId in filterModel.FilterGhostRole.Values)
            {
                filterSet.Add(extremeRoleId);
            }

            filter.AddRoleFilterSet(filterSet);
        }
    }

    public static void RemoveFilter(RoleAssignFilterModel model, int targetFilter)
    {
        model.FilterSet.Remove(targetFilter);
    }

    public static void RemoveFilterRole(
		RoleAssignFilterModel model, int targetFilter, int targetId)
	{
        var filter = model.FilterSet[targetFilter];
		filter.FilterNormalId.Remove(targetId);
        filter.FilterCombinationId.Remove(targetId);
        filter.FilterGhostRole.Remove(targetId);
    }

    public static void ResetFilter(RoleAssignFilterModel model, int targetFilter)
    {
        var filter = model.FilterSet[targetFilter];
        filter.Id = 0;
        filter.FilterNormalId.Clear();
        filter.FilterCombinationId.Clear();
        filter.FilterGhostRole.Clear();
    }
}
