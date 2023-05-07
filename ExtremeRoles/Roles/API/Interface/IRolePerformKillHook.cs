namespace ExtremeRoles.Roles.API.Interface;

public interface IRolePerformKillHook
{
    public void OnStartKill();

    public void OnEndKill();
}
