namespace ExtremeRoles.Roles.API.Interface
{
    public interface IRoleMurderPlayerHook
    {
        void HookMuderPlayer(
            PlayerControl source,
            PlayerControl target);
    }
}
