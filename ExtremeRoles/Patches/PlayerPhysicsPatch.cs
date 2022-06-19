using HarmonyLib;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Patches
{

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public class PlayerPhysicsPatch
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (!ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd()) { return; }
            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            var overLoader = ExtremeRoleManager.GetSafeCastedRole<
                Roles.Solo.Impostor.OverLoader>(__instance.myPlayer.PlayerId);

            if (overLoader.IsOverLoad &&
                __instance.AmOwner && 
                __instance.myPlayer.CanMove && 
                GameData.Instance)
            {
                __instance.body.velocity *= overLoader.Speed;
            }
        }
    }
}
