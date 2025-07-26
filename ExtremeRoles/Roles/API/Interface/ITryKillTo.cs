namespace ExtremeRoles.Roles.API.Interface;

public interface ITryKillTo
{
    bool TryRolePlayerKillTo(
        PlayerControl rolePlayer,
        PlayerControl targetPlayer);
}
