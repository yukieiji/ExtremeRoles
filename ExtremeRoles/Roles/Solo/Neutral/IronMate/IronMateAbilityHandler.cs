using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;

namespace ExtremeRoles.Roles.Solo.Neutral.IronMate
{
    public class IronMateAbilityHandler : IAbility, IKilledFrom
    {
        private IronMateStatusModel status;

        public IronMateAbilityHandler(IronMateStatusModel status)
        {
            this.status = status;
        }

        public bool TryKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)
        {
            if (status.system is null)
            {
                return true;
            }

            byte playerId = rolePlayer.PlayerId;

            if (!status.system.IsContains(playerId))
            {
                status.system.SetUp(playerId, status.BlockNum);
            }

            if (!status.system.TryGetShield(playerId, out int num))
            {
                return true;
            }

            if (fromPlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                fromPlayer.SetKillTimer(10.0f);
                Sound.PlaySound(Sound.Type.GuardianAngleGuard, 0.85f);
            }
            status.system.RpcUpdateNum(playerId, num - 1);
            return false;
        }
    }
}
