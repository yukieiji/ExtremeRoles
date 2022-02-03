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

            if (__instance.AmOwner && __instance.myPlayer.CanMove && GameData.Instance)
            {
                var overLoader = ExtremeRoleManager.GameRole[
                    __instance.myPlayer.PlayerId] as Roles.Solo.Impostor.OverLoader;
                if (overLoader == null) { return; }

                if (overLoader.IsOverLoad)
                {
                    __instance.body.velocity *= overLoader.Speed;
                }
            }
        }
    }
}
