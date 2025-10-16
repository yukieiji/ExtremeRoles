using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.SystemType;

namespace ExtremeRoles;

#pragma warning disable ERA001, ERA002
public sealed class ExtremeRolePluginBehavior : MonoBehaviour
#pragma warning restore ERA001, ERA002
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
			GameProgressSystem.IsTaskPhase)
        {
            Logging.Debug($"{PlayerStatistics.Create()}");
        }
        if (Input.GetKeyDown(KeyCode.F10) &&
            ExtremeRolesPlugin.DebugMode.Value)
        {
            foreach(PetData pet in HatManager.Instance.allPets)
            {
                Logging.Debug($"Cosmic Id:{pet.ProdId}");
            }
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            if (Roles.ExtremeRoleManager.GetLocalPlayerRole() is Roles.API.Interface.IRoleAbility abilityRole)
            {
                Logging.Debug("---- Role Button Info ----");
                Logging.Debug($"Cool Time:{abilityRole.Button.Behavior.CoolTime}");
			if (abilityRole.Button.Behavior is IActivatingBehavior behavior)
			{
				Logging.Debug($"Active Time:{behavior.ActiveTime}");
			}
                Logging.Debug($"Button State:{abilityRole.Button.State}");
            }

            var ghostRole = GhostRoles.ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
            if (ghostRole != null)
            {
                Logging.Debug("---- Ghost Role Button Info ----");
                Logging.Debug($"Cool Time:{ghostRole.Button.Behavior.CoolTime}");
			if (ghostRole.Button.Behavior is IActivatingBehavior behavior)
			{
				Logging.Debug($"Active Time:{behavior.ActiveTime}");
			}
                Logging.Debug($"Button State:{ghostRole.Button.State}");
            }
        }
#endif
    }
}
