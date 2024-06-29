using HarmonyLib;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Player;

#nullable enable

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Awake))]
public static class PlayerControlAwakePatch
{
	public static void Postfix(PlayerControl __instance)
	{
		if (__instance.notRealPlayer) { return; }

		PlayerCache.AddPlayerControl(__instance);

#if DEBUG
		foreach (var cachedPlayer in AmongUsCache.AllPlayerControl)
		{
			if (!cachedPlayer.PlayerControl ||
				!cachedPlayer.MyPhysics ||
				!cachedPlayer.NetTransform ||
				!cachedPlayer.transform)
			{
				Logging.Debug($"CachedPlayer {cachedPlayer.name} has null fields");
			}
		}
#endif
	}
}