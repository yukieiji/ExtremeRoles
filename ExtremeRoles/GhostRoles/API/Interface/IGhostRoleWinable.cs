namespace ExtremeRoles.GhostRoles.API.Interface
{
    public interface IGhostRoleWinable
    {
        public bool IsWin(
            GameOverReason reason,
            NetworkedPlayerInfo ghostRolePlayer);
    }
}
