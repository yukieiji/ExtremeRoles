using HarmonyLib;

using UnityEngine;

namespace ExtremeSkins.Patches.AmongUs;

[HarmonyPatch(typeof(PoolablePlayer), nameof(PoolablePlayer.UpdateFromPlayerOutfit))]
public static class PoolablePlayerPatch
{
    public static void Postfix(PoolablePlayer __instance)
    {
        if (!__instance.cosmetics.visor ||
            !__instance.cosmetics.visor.transform || 
            !__instance.cosmetics.hat ||
            !__instance.cosmetics.transform) { return; }

        // fixes a bug in the original where the visor will show up beneath the hat,
        // instead of on top where it's supposed to be
        __instance.cosmetics.visor.transform.localPosition = new Vector3(
            __instance.cosmetics.visor.transform.localPosition.x,
            __instance.cosmetics.visor.transform.localPosition.y,
            __instance.cosmetics.hat.transform.localPosition.z - 1);
    }
}
