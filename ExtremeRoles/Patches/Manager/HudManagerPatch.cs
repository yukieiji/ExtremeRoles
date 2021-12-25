using HarmonyLib;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Patches.Manager
{

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class HudManagerUpdatePatch
    {
        public static void Prefix(HudManager __instance)
        {
            if (__instance.GameSettings != null)
            {
                __instance.GameSettings.fontSize = 1.2f;
            }

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            
            var role = ExtremeRoleManager.GetLocalPlayerRole();
            if (role is IRoleAbility)
            {
                var abilityRole = (IRoleAbility)role;

                if (abilityRole.Button == null)
                {
                    abilityRole.CreateAbility();
                }
            }
            

        }

    }
}
