using HarmonyLib;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Performance;
using ExtremeRoles.Extension.Ship;

namespace ExtremeRoles.Patches.MapModule
{
    [HarmonyPatch(typeof(Vent), "UsableDistance", MethodType.Getter)]
    public static class VentUsableDistancePatch
    {
        public static bool Prefix(
            ref float __result)
        {
            if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

            var underWarper = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<UnderWarper>();

            if (underWarper == null ||
                !underWarper.IsAwake) { return true; }
            
            __result = underWarper.VentUseRange;

            return false;
        }
    }

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

            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.Normal)
            {
                return true;
            }

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

            if (ExtremeRoleManager.GameRole.Count == 0)
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

            bool roleCouldUse = ExtremeRoleManager.GameRole[playerInfo.PlayerId].CanUseVent();

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
            if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

            var role = ExtremeRoleManager.GetLocalPlayerRole();

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

            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            __instance.CanUse(
                localPlayer.Data,
                out canUse, out couldUse);

            if (!canUse) { return false; }; // No need to execute the native method as using is disallowed anyways

            bool isEnter = !localPlayer.inVent;

            if (CachedShipStatus.Instance.IsCustomVent(
                __instance.Id))
            {
                __instance.SetButtons(isEnter);

                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.CustomVentUse))
                {
                    caller.WritePackedInt(__instance.Id);
                    caller.WriteByte(localPlayer.PlayerId);
                    caller.WriteByte(isEnter ? byte.MaxValue : (byte)0);
                }
                RPCOperator.CustomVentUse(
                    __instance.Id,
                    localPlayer.PlayerId,
                    isEnter ? byte.MaxValue : (byte)0);
                
                __instance.SetButtons(isEnter);

                return false;
            }

            if (ExtremeRolesPlugin.ShipState.IsRoleSetUpEnd)
            {
                var underWarper = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<UnderWarper>();
                if (underWarper != null &&
                    underWarper.IsAwake &&
                    underWarper.IsNoVentAnime)
                {
                    UnderWarper.RpcUseVentWithNoAnimation(
                        localPlayer, __instance.Id, isEnter);
                    __instance.SetButtons(isEnter);
                    return false;
                }
            }

            if (isEnter)
            {
                localPlayer.MyPhysics.RpcEnterVent(__instance.Id);
            }
            else
            {
                localPlayer.MyPhysics.RpcExitVent(__instance.Id);
            }

            __instance.SetButtons(isEnter);
            return false;
        }
    }
}
