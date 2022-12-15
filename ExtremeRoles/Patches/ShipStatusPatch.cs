using System.Runtime.CompilerServices;

using HarmonyLib;

using UnityEngine;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.ExtremeShipStatus;
using AmongUs.GameOptions;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static class ShipStatusAwakePatch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Postfix(ShipStatus __instance)
        {
            CachedShipStatus.SetUp(__instance);
            ExtremeRolesPlugin.Compat.SetUpMap(__instance);   
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    public static class ShipStatusCalculateLightRadiusPatch
    {
        private static float crewLightVison => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
            FloatOptionNames.CrewLightMod);

        private static float impLightVison => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
            FloatOptionNames.ImpostorLightMod);

        public static bool Prefix(
            ref float __result,
            ShipStatus __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo playerInfo)
        {

            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.Normal)
            { 
                return true;
            }

            switch (ExtremeRolesPlugin.ShipState.CurVison)
            {
                case ExtremeShipStatus.ForceVisonType.LastWolfLightOff:
                    if (ExtremeRoleManager.GetSafeCastedLocalPlayerRole<
                        Roles.Solo.Impostor.LastWolf>() == null)
                    {
                        __result = 0.15f;
                        return false;
                    }
                    break;
                case ExtremeShipStatus.ForceVisonType.WispLightOff:
                    if (!Wisp.HasTorch(playerInfo.PlayerId))
                    {
                        __result = __instance.MinLightRadius * crewLightVison;
                        return false;
                    }
                    break;
                default:
                    break;
            }

            if (!ExtremeRolesPlugin.ShipState.IsRoleSetUpEnd)
            {
                return checkNormalOrCustomCalculateLightRadius(playerInfo, ref __result);
            }

            ISystemType systemType = __instance.Systems.ContainsKey(
                SystemTypes.Electrical) ? __instance.Systems[SystemTypes.Electrical] : null;
            if (systemType == null)
            {
                return checkNormalOrCustomCalculateLightRadius(playerInfo, ref __result);
            }

            SwitchSystem switchSystem = systemType.TryCast<SwitchSystem>();
            if (switchSystem == null)
            {
                return checkNormalOrCustomCalculateLightRadius(playerInfo, ref __result);
            }

            var allRole = ExtremeRoleManager.GameRole;
            if (allRole.Count == 0)
            {
                if (requireCustomCustomCalculateLightRadius())
                {
                    __result = ExtremeRolesPlugin.Compat.ModMap.CalculateLightRadius(
                        playerInfo, false, playerInfo.Role.IsImpostor);
                    return false;
                }
                return true;
            }

            SingleRoleBase role = allRole[playerInfo.PlayerId];

            if (requireCustomCustomCalculateLightRadius())
            {
                float visonMulti;
                bool applayVisonEffects = !role.IsImpostor();

                if (role.TryGetVisonMod(out float vison, out bool isApplyEnvironmentVision))
                {
                    visonMulti = vison;
                    applayVisonEffects = isApplyEnvironmentVision;
                }
                else if (playerInfo.Role.IsImpostor)
                {
                    visonMulti = impLightVison;
                }
                else
                {
                    visonMulti = crewLightVison;
                }

                __result = ExtremeRolesPlugin.Compat.ModMap.CalculateLightRadius(
                    playerInfo, visonMulti, applayVisonEffects);

                return false;
            }

            float num = (float)switchSystem.Value / 255f;
            float switchVisonMulti = Mathf.Lerp(
                __instance.MinLightRadius,
                __instance.MaxLightRadius, num);

            float baseVison = __instance.MaxLightRadius;

            if (playerInfo == null || playerInfo.IsDead) // IsDead
            {
                __result = baseVison;
            }
            else if (role.TryGetVisonMod(out float vison, out bool isApplyEnvironmentVision))
            {
                if (isApplyEnvironmentVision)
                {
                    baseVison = switchVisonMulti;
                }
                __result = baseVison * vison;
            }
            else if (playerInfo.Role.IsImpostor)
            {
                __result = baseVison * impLightVison;
            }
            else
            {
                __result = switchVisonMulti * crewLightVison;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool requireCustomCustomCalculateLightRadius() =>
            ExtremeRolesPlugin.Compat.IsModMap &&
            ExtremeRolesPlugin.Compat.ModMap.IsCustomCalculateLightRadius;

        private static bool checkNormalOrCustomCalculateLightRadius(
            GameData.PlayerInfo player, ref float result)
        {
            if (requireCustomCustomCalculateLightRadius())
            {
                result = ExtremeRolesPlugin.Compat.ModMap.CalculateLightRadius(
                    player, false, player.Role.IsImpostor);
                return false;
            }
            return true;
        }

    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnDestroy))]
    public static class ShipStatusOnDestroyPatch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Postfix()
        {
            CachedShipStatus.Destroy();
            ExtremeRolesPlugin.Compat.RemoveMap();
        }
    }
}
