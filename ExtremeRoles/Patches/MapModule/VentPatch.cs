using HarmonyLib;
using Hazel;
using UnityEngine;

using ExtremeRoles.Performance;

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

            canUse = couldUse = false;

            if (OptionHolder.Ship.DisableVent)
            {
                __result = num;
                return false;
            }

            if (__instance.myRend.sprite == null)
            {
                return false;
            }

            bool isCustomMapVent = ExtremeRolesPlugin.Compat.IsModMap &&
                ExtremeRolesPlugin.Compat.ModMap.IsCustomVentUse(__instance);

            if (Roles.ExtremeRoleManager.GameRole.Count == 0)
            {
                if (isCustomMapVent)
                {
                    (__result, canUse, couldUse) = ExtremeRolesPlugin.Compat.ModMap.IsCustomVentUseResult(
                        __instance, playerInfo,
                        playerInfo.Role.IsImpostor || playerInfo.Role.Role == RoleTypes.Engineer);
                    return false;
                }
                return true; 
            }

            bool roleCouldUse = Roles.ExtremeRoleManager.GameRole[playerInfo.PlayerId].UseVent;

            if (isCustomMapVent)
            {
                (__result, canUse, couldUse) = ExtremeRolesPlugin.Compat.ModMap.IsCustomVentUseResult(
                    __instance, playerInfo, roleCouldUse);
                return false;
            }

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

    [HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
    public static class VentSetOutlinePatch
    {
        public static bool Prefix(
            Vent __instance,
            [HarmonyArgument(0)] bool on,
            [HarmonyArgument(1)] bool mainTarget)
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }

            var role = Roles.ExtremeRoleManager.GetLocalPlayerRole();

            if (role.IsVanillaRole() || role.IsImpostor()) { return true; }

            Color color = role.GetNameColor();
            
            __instance.myRend.material.SetFloat("_Outline", (float)(on ? 1 : 0));
            __instance.myRend.material.SetColor("_OutlineColor", color);
            __instance.myRend.material.SetColor("_AddColor", mainTarget ? color : Color.clear);

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
            
            __instance.CanUse(CachedPlayerControl.LocalPlayer.Data, out canUse, out couldUse);

            if (!canUse) { return false; }; // No need to execute the native method as using is disallowed anyways

            bool isEnter = !CachedPlayerControl.LocalPlayer.PlayerControl.inVent;

            if (ExtremeRolesPlugin.GameDataStore.CustomVent.IsCustomVent(
                __instance.Id))
            {
                __instance.SetButtons(isEnter);
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                    CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                    (byte)RPCOperator.Command.CustomVentUse,
                    Hazel.SendOption.Reliable);
                writer.WritePacked(__instance.Id);
                writer.Write(CachedPlayerControl.LocalPlayer.PlayerId);
                writer.Write(isEnter ? byte.MaxValue : (byte)0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCOperator.CustomVentUse(
                    __instance.Id,
                    PlayerControl.LocalPlayer.PlayerId,
                    isEnter ? byte.MaxValue : (byte)0);
                return false;
            }

            if (isEnter)
            {
                CachedPlayerControl.LocalPlayer.PlayerPhysics.RpcEnterVent(__instance.Id);
            }
            else
            {
                CachedPlayerControl.LocalPlayer.PlayerPhysics.RpcExitVent(__instance.Id);
            }

            __instance.SetButtons(isEnter);

            return false;
        }
    }
}
