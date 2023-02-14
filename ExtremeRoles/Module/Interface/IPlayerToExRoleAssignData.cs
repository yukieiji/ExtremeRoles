namespace ExtremeRoles.Module.Interface
{
    public interface IPlayerToExRoleAssignData
    {
        public enum ExRoleType : byte
        {
            Single,
            Comb,
        }

        public int ControlId { get; }
        public byte PlayerId { get; }
        public int RoleId { get; }
        public byte RoleType { get; }
    }
}
