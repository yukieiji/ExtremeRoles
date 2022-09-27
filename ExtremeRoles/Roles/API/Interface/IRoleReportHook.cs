namespace ExtremeRoles.Roles.API.Interface
{
    public interface IRoleReportHook
    {
        public void HookReportButton(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter);
        public void HookBodyReport(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter,
            GameData.PlayerInfo reportBody);
    }
}
