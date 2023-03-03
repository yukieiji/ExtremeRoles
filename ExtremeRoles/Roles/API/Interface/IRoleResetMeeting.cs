namespace ExtremeRoles.Roles.API.Interface
{
    public interface IRoleResetMeeting
    {
        public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null);

        public void ResetOnMeetingStart();
    }
}
