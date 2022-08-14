using ExtremeRoles.Helper;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion
    {
        protected override void CommonInit()
        {
            return;
        }

        protected override void RoleSpecificInit()
        {
            return;
        }

        private void Init(byte xionPlayerId)
        {
            if (xionPlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
            {
                
            }
        }

    }
}
