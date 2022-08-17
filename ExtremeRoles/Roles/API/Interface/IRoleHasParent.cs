namespace ExtremeRoles.Roles.API.Interface
{
    public interface IRoleHasParent
    {
        public byte Parent { get; }

        public void RemoveParent(byte rolePlayerId);
    }
}
