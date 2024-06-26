namespace ExtremeRoles.Roles.API.Interface
{
    public interface IRoleResetMeeting
    {
        public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null);

        public void ResetOnMeetingStart();
    }
}
