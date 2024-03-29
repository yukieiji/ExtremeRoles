﻿using HarmonyLib;

using ExtremeRoles.Helper;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Player;

#nullable enable

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Awake))]
public static class PlayerControlAwakePatch
{
	public static void Postfix(PlayerControl __instance)
	{
		if (__instance.notRealPlayer) { return; }

		new CachedPlayerControl(__instance);

#if DEBUG
		foreach (var cachedPlayer in CachedPlayerControl.AllPlayerControls)
		{
			if (!cachedPlayer.PlayerControl ||
				!cachedPlayer.PlayerPhysics ||
				!cachedPlayer.NetTransform ||
				!cachedPlayer.transform)
			{
				Logging.Debug($"CachedPlayer {cachedPlayer.PlayerControl.name} has null fields");
			}
		}
#endif
	}
}
