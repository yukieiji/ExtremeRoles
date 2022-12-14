using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion
    {
        public static void Purge()
        {
            if (DestroyableSingleton<HudManager>.InstanceExists)
            {
                var useButton = FastDestroyableSingleton<HudManager>.Instance.UseButton;
                GridArrange grid = useButton.transform.parent.gameObject.GetComponent<GridArrange>();
                grid.MaxColumns = 3;
            }

            PlayerId = byte.MaxValue;
            voted = false;
        }

        protected override void CommonInit()
        {
            return;
        }

        protected override void RoleSpecificInit()
        {
            return;
        }
    }
}
