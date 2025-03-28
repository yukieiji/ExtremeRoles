using HarmonyLib;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.ApiHandler;
using UnityEngine;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(LobbyInfoPane), nameof(LobbyInfoPane.CopyGameCode))]
public static class LobbyInfoPaneCopyGameCodePatch
{
	public static bool Prefix(LobbyInfoPane __instance)
	{
		if (Key.IsShiftDown())
		{
			return true;
		}

		GUIUtility.systemCopyBuffer = ConectGame.CreateDirectConectUrl(AmongUsClient.Instance.GameId);
		SoundManager.Instance.PlaySoundImmediate(__instance.CopyCodeSound, false, 1f, 1f, null);
		if (__instance.copyGameCodeCoroutine != null)
		{
			__instance.StopCoroutine(__instance.copyGameCodeCoroutine);
		}
		__instance.copyGameCodeCoroutine = __instance.CoCopyGameCode();
		__instance.StartCoroutine(__instance.copyGameCodeCoroutine);
		return false;
	}
}
