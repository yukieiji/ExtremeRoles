namespace ExtremeRoles.Roles.API.Interface
{
    public interface IRoleReportHook
    {
        public void HookReportButton(
            PlayerControl rolePlayer,
            NetworkedPlayerInfo reporter);
        public void HookBodyReport(
            PlayerControl rolePlayer,
            NetworkedPlayerInfo reporter,
            NetworkedPlayerInfo reportBody);
    }
}
