using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Module;
using UnityEngine;
using ExtremeRoles.Module.RoleAssign;

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
                RoleAssignState.Instance.IsRoleSetUpEnd)
            {
                Logging.Debug($"{PlayerStatistics.Create()}");
            }
            if (Input.GetKeyDown(KeyCode.F10) &&
                ExtremeRolesPlugin.DebugMode.Value)
            {
                foreach(PetData pet in FastDestroyableSingleton<HatManager>.Instance.allPets)
                {
                    Logging.Debug($"Cosmic Id:{pet.ProdId}");
                }
            }
            if (Input.GetKeyDown(KeyCode.F12))
            {
                if (Roles.ExtremeRoleManager.GetLocalPlayerRole() is Roles.API.Interface.IRoleAbility abilityRole)
                {
                    Logging.Debug("---- Role Button Info ----");
                    Logging.Debug($"Cool Time:{abilityRole.Button.Behavior.CoolTime}");
                    Logging.Debug($"Active Time:{abilityRole.Button.Behavior.ActiveTime}");
                    Logging.Debug($"Button State:{abilityRole.Button.State}");
                }

                var ghostRole = GhostRoles.ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
                if (ghostRole != null)
                {
                    Logging.Debug("---- Ghost Role Button Info ----");
                    Logging.Debug($"Cool Time:{ghostRole.Button.Behavior.CoolTime}");
                    Logging.Debug($"Active Time:{ghostRole.Button.Behavior.ActiveTime}");
                    Logging.Debug($"Button State:{ghostRole.Button.State}");
                }
            }
#endif
        }
    }
}
