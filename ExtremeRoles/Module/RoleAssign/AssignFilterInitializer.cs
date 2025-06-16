namespace ExtremeRoles.Module.RoleAssign;

public class AssignFilterInitializer : IAssignFilterInitializer
{
    public void Initialize(RoleAssignFilter filter, PreparationData data)
    {
		if (data.Assign.Data.Count == 0)
		{
			filter.Initialize();
			return;
		}
    }
}
