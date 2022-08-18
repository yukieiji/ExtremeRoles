namespace ExtremeRoles.Roles.API.Interface
{
    public interface IRoleHasParent
    {
        public byte Parent { get; }

        public void RemoveParent(byte rolePlayerId);

        public static void PurgeParent(byte rolePlayerId)
        {
            var (role, anotherRole) = ExtremeRoleManager.GetInterfaceCastedRole<IRoleHasParent>(rolePlayerId);
            role?.RemoveParent(rolePlayerId);
            anotherRole?.RemoveParent(rolePlayerId);
        }

    }
}
