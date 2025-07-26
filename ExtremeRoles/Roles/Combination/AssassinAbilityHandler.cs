using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Roles.Combination
{
    public class AssassinAbilityHandler : IAbility, IKilledFrom
    {
        private AssassinStatusModel status;

        public AssassinAbilityHandler(AssassinStatusModel status)
        {
            this.status = status;
        }

        public bool TryKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)
        {
            if (!(status.CanKilled && ExtremeRoleManager.TryGetRole(fromPlayer.PlayerId, out var fromPlayerRole)))
            {
                return false;
            }

            if (fromPlayerRole.IsNeutral())
            {
                return status.CanKilledFromNeutral;
            }
            else if (fromPlayerRole.IsCrewmate())
            {
                return status.CanKilledFromCrew;
            }

            return false;
        }
    }
}
