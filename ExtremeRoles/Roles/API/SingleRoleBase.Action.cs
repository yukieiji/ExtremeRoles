namespace ExtremeRoles.Roles.API
{
    public abstract partial class SingleRoleBase
    {
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

        public virtual bool TryRolePlayerKilledFrom(
            PlayerControl rolePlayer,
            PlayerControl fromPlayer) => true;
    }
}
