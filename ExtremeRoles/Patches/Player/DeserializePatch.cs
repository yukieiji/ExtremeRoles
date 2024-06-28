using HarmonyLib;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Player;

#nullable enable
/*

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Deserialize))]
public static class PlayerControlDeserializePatch
{
	public static void Postfix(PlayerControl __instance)
	{
		PlayerControl.PlayerPtrs[__instance.Pointer].PlayerId = __instance.PlayerId;
	}
}
*/
