using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;

namespace ExtremeRoles.Roles.Solo.Neutral.Yoko
{
    public class YokoAbilityHandler : IAbility, IKilledFrom
    {
        private YokoStatusModel status;

        public YokoAbilityHandler(YokoStatusModel status)
        {
            this.status = status;
        }

        public bool TryKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)
            => status.yashiro is null || !status.yashiro.IsNearActiveYashiro(
                rolePlayer.GetTruePosition());
    }
}
