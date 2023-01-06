namespace ExtremeRoles.Module.Interface
{
    public interface IWinChecker
    {
        public Roles.RoleGameOverReason Reason { get; }
        public bool IsWin(PlayerStatistics statistics);
        public void AddAliveRole(byte playerId, Roles.API.SingleRoleBase role);
    }
}
