using HarmonyLib;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Player;

#nullable enable

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnDestroy))]
public static class PlayerControlOnDestroyPatch
{
	public static void Postfix(PlayerControl __instance)
	{
		if (__instance.notRealPlayer) { return; }
		PlayerCache.RemovePlayerControl(__instance);
	}
}
