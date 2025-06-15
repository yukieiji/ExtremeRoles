namespace ExtremeRoles.Module.RoleAssign;

public class AssignFilterInitializer : IAssignFilterInitializer
{
    public void Initialize(RoleAssignFilter filter, PreparationData data)
    {
        // PreparationData は現在の実装では使用しないが、インターフェースに合わせて引数として受け取る
        filter.Initialize();
    }
}
