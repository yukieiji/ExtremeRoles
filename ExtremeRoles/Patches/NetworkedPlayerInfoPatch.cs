using HarmonyLib;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches;

/*
[HarmonyPatch(typeof(NetworkedPlayerInfo), nameof(NetworkedPlayerInfo.Deserialize))]
public static class NetworkedPlayerInfoDeserializePatch
{
	public static void Postfix()
	{
		foreach (PlayerControl cachedPlayer in PlayerControl.AllPlayerControls)
		{
			cachedPlayer.Data = cachedPlayer.Data;
			cachedPlayer.PlayerId = cachedPlayer.PlayerId;
		}
	}
}
*/
