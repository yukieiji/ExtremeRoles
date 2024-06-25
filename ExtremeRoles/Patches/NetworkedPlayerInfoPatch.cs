using HarmonyLib;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches;


[HarmonyPatch(typeof(NetworkedPlayerInfo), nameof(NetworkedPlayerInfo.Deserialize))]
public static class NetworkedPlayerInfoDeserializePatch
{
	public static void Postfix()
	{
		foreach (CachedPlayerControl cachedPlayer in CachedPlayerControl.AllPlayerControls)
		{
			cachedPlayer.Data = cachedPlayer.PlayerControl.Data;
			cachedPlayer.PlayerId = cachedPlayer.PlayerControl.PlayerId;
		}
	}
}
