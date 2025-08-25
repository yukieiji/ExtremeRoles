using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Roles.Combination.HeroAcademia;

public class HeroAbilityHandler(HeroStatusModel status) : IAbility, IKilledFrom
{
	private HeroStatusModel status = status;

    public bool TryKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)
    {
        if (ExtremeRoleManager.TryGetRole(fromPlayer.PlayerId, out var fromRole) &&
			fromRole.Core.Id is ExtremeRoleId.Villain)
        {
            HeroAcademiaRole.RpcDrawHeroAndVillan(rolePlayer, fromPlayer);
            return false;
        }

		return this.status.Cond is Hero.OneForAllCondition.NoGuard || !fromRole.IsImpostor();
    }
}
