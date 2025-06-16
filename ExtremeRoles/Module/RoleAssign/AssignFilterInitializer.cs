namespace ExtremeRoles.Module.RoleAssign;

public class AssignFilterInitializer : IAssignFilterInitializer
{
    public void Initialize(RoleAssignFilter filter, PreparationData data)
    {
		filter.Initialize();

		// アサインデータを再追加する
		foreach (var assignData in data.Assign.Data)
		{

		}
	}
}
