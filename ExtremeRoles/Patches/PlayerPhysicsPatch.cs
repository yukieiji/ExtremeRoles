using HarmonyLib;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Patches
{

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PlayerPhysicsPatch
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (!ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd) { return; }
            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            var role = ExtremeRoleManager.GetLocalPlayerRole();

            if (role.IsBoost &&
                __instance.AmOwner && 
                __instance.myPlayer.CanMove && 
                GameData.Instance)
            {
                __instance.body.velocity *= role.MoveSpeed;
            }
        }
    }
}
