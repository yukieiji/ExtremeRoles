using HarmonyLib;
using UnityEngine;

namespace ExtremeRoles.Patches.MapModule
{
    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    public static class VentCanUsePatch
    {
        public static bool Prefix(
            Vent __instance,
            ref float __result,
            [HarmonyArgument(0)] GameData.PlayerInfo playerInfo,
            [HarmonyArgument(1)] out bool canUse,
            [HarmonyArgument(2)] out bool couldUse)
        {
            float num = float.MaxValue;
            PlayerControl player = playerInfo.Object;

            if (MapOption.DisableVents)
            {
                canUse = couldUse = false;
                __result = num;
                return false;
            }
            bool roleCouldUse = Roles.ExtremeRoleManager.GameRole[playerInfo.PlayerId].UseVent;

            var usableDistance = __instance.UsableDistance;

            couldUse = (
                !playerInfo.IsDead &&
                (player.inVent || roleCouldUse) && 
                (player.CanMove || player.inVent));
            
            canUse = couldUse;
            if (canUse)
            {
                Vector2 truePosition = player.GetTruePosition();
                Vector3 position = __instance.transform.position;
                num = Vector2.Distance(truePosition, position);

                canUse &= (
                    num <= usableDistance && 
                    !PhysicsHelpers.AnythingBetween(
                        truePosition,
                        position,
                        Constants.ShipOnlyMask, false));
            }

            __result = num;
            return false;
        }
    }
    [HarmonyPatch(typeof(Vent), nameof(Vent.Use))]
    public static class VentUsePatch
    {
        public static bool Prefix(Vent __instance)
        {
            bool canUse;
            bool couldUse;
            
            __instance.CanUse(PlayerControl.LocalPlayer.Data, out canUse, out couldUse);

            // bool canMoveInVents = PlayerControl.LocalPlayer != Spy.spy && Madmate.madmate != PlayerControl.LocalPlayer;

            if (!canUse) { return false; }; // No need to execute the native method as using is disallowed anyways

            bool isEnter = !PlayerControl.LocalPlayer.inVent;

            if (isEnter)
            {
                PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(__instance.Id);
            }
            else
            {
                PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(__instance.Id);
            }

            __instance.SetButtons(isEnter);

            Modules.Helpers.DebugLog($"VentStatus:{canUse}   {couldUse}   {isEnter}");

            return false;
        }
    }
}
