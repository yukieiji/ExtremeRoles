namespace ExtremeRoles.Roles.API.Interface
{
    public interface IRoleExilHook
    {
        public void HookWrapUp(GameData.PlayerInfo exiledPlayer);
    }
}
