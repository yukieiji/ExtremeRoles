using ExtremeRoles.Roles.API.Interface.Ability;

#nullable enable

namespace ExtremeRoles.Roles.API;

public abstract partial class SingleRoleBase
{
	public IAbility? AbilityClass { get; protected set; }

	public virtual void ExiledAction(
        PlayerControl rolePlayer)
    {
        return;
    }

    public virtual void RolePlayerKilledAction(
        PlayerControl rolePlayer,
        PlayerControl killerPlayer)
    {
        return;
    }

    public virtual bool TryRolePlayerKillTo(
        PlayerControl rolePlayer,
        PlayerControl targetPlayer) => true;
}
