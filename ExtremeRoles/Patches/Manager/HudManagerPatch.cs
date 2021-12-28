using HarmonyLib;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
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

            buttonCreate(role);
            roleUpdate(role);

            var multiAssignRole = role as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    buttonCreate(multiAssignRole.AnotherRole);
                    roleUpdate(multiAssignRole.AnotherRole);
                }
            }

        }
        private static void buttonCreate(SingleRoleBase checkRole)
        {
            var abilityRole = checkRole as IRoleAbility;

            if (abilityRole.Button == null)
            {
                abilityRole.CreateAbility();
                abilityRole.RoleAbilityInit();
            }
        }

        private static void roleUpdate(SingleRoleBase checkRole)
        {
            var updatableRole = checkRole as IRoleUpdate;
            if (updatableRole != null)
            {
                updatableRole.Update(PlayerControl.LocalPlayer);
            }
        }

    }
}
