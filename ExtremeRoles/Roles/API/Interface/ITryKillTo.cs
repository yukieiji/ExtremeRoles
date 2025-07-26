namespace ExtremeRoles.Roles.API.Interface;

public interface ITryKillTo
{
    public bool TryRolePlayerKillTo(
        PlayerControl rolePlayer,
        PlayerControl targetPlayer);
}
