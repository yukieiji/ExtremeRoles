namespace ExtremeRoles.Roles.API.Interface;

#nullable enable

public interface IParentChainStatus
{
    public byte Parent { get; }

    public void RemoveParent(byte rolePlayerId);

    public static void PurgeParent(byte rolePlayerId)
    {
		var (status, anotherStatus) = ExtremeRoleManager.GetRoleStatus<IParentChainStatus>(rolePlayerId);

		status?.RemoveParent(rolePlayerId);
		anotherStatus?.RemoveParent(rolePlayerId);
    }

}
