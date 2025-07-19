﻿using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion
    {
        public static void Purge()
        {
            if (HudManager.InstanceExists)
            {
                var useButton = HudManager.Instance.UseButton;
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
