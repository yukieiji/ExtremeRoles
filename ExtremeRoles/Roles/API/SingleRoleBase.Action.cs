using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.API;

public abstract partial class SingleRoleBase
{
	public IAbility? AbilityClass { get; protected set; }
	public virtual IStatusModel? Status { get; }

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
