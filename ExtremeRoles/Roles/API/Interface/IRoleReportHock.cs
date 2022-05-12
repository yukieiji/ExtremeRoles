namespace ExtremeRoles.Roles.API.Interface
{
    public interface IRoleReportHock
    {
        public void HockReportButton(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter);
        public void HockBodyReport(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter,
            GameData.PlayerInfo reportBody);
    }
}
