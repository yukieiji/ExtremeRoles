using HarmonyLib;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Patches
{

    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
    public class CustomNetworkTransformPatch
    {
        public static void Postfix(CustomNetworkTransform __instance)
        {
            if (!ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd()) { return; }
            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            byte playerId = __instance.gameObject.GetComponent<PlayerControl>().PlayerId;
            var overLoader = ExtremeRoleManager.GetSafeCastedRole<Roles.Solo.Impostor.OverLoader>(playerId);

            if (overLoader == null) { return; }

            if (overLoader.IsOverLoad &&
                !__instance.AmOwner && 
                __instance.interpolateMovement != 0.0f)
            {
                __instance.body.velocity *= overLoader.Speed;
            }
        }
    }
}
