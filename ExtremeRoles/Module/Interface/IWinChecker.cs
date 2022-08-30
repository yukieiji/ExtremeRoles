namespace ExtremeRoles.Module.SpecialWinChecker
{
    public interface IWinChecker
    {
        public Roles.RoleGameOverReason Reason { get; }
        public bool IsWin(ExtremeShipStatus.PlayerStatistics statistics);
        public void AddAliveRole(byte playerId, Roles.API.SingleRoleBase role);
    }
}
