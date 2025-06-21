using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.RoleAssign;

public class AssignFilterInitializer : IAssignFilterInitializer
{
    public void Initialize(RoleAssignFilter filter, PreparationData data)
    {
		filter.Initialize();

		// アサインデータを再追加する
		foreach (var assignData in data.Assign.Data)
		{
			if (assignData is PlayerToCombRoleAssignData combRoleAssignData)
			{
				filter.Update(combRoleAssignData.CombTypeId);
			}
			else
			{
				filter.Update(assignData.RoleId);
			}
		}
	}
}
