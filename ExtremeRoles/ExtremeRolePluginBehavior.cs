using ExtremeRoles.Helper;
using UnityEngine;

namespace ExtremeRoles
{
    public sealed class ExtremeRolePluginBehavior : MonoBehaviour
    {
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                Logging.Dump();
            }
#if DEBUG
            if (Input.GetKeyDown(KeyCode.F9) &&
                ExtremeRolesPlugin.DebugMode.Value &&
                ExtremeRolesPlugin.ShipState.IsRoleSetUpEnd)
            {
                Logging.Debug($"{ExtremeRolesPlugin.ShipState.CreateStatistics()}");
            }
#endif
        }
    }
}
