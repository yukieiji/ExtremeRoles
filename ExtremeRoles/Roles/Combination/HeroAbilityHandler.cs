using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Roles.Combination
{
    public class HeroAbilityHandler : IAbility, IKilledFrom
    {
        private HeroStatusModel status;

        public HeroAbilityHandler(HeroStatusModel status)
        {
            this.status = status;
        }

        public bool TryKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)
        {
            var fromRole = ExtremeRoleManager.GameRole[fromPlayer.PlayerId];

            if (fromRole.Core.Id == ExtremeRoleId.Villain)
            {
                HeroAcademia.RpcDrawHeroAndVillan(rolePlayer, fromPlayer);
                return false;
            }
            else if (fromRole.IsImpostor() && status.cond != Hero.OneForAllCondition.NoGuard)
            {
                return false;
            }
            return true;
        }
    }
}
