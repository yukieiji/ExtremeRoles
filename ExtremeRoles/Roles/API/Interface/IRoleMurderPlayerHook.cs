namespace ExtremeRoles.Roles.API.Interface
{
    public interface IRoleMurderPlayerHook
    {
        void HookMurderPlayer(
            PlayerControl source,
            PlayerControl target);
    }
}
