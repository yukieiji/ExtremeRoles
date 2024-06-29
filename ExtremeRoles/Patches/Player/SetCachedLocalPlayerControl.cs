using System.Linq;
using System.Reflection;

using ExtremeRoles.Performance;
using HarmonyLib;

namespace ExtremeRoles.Patches.Player;

#nullable enable


[HarmonyPatch]
public static class SetCachedLocalPlayerControl
{
	[HarmonyTargetMethod]
	public static MethodBase TargetMethod()
	{
		var type = typeof(PlayerControl).GetNestedTypes(AccessTools.all).FirstOrDefault(t => t.Name.Contains("Start"));
		return AccessTools.Method(type, nameof(Il2CppSystem.Collections.IEnumerator.MoveNext));
	}

	[HarmonyPostfix]
	public static void SetLocalPlayer()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (!localPlayer)
		{
			PlayerControl.LocalPlayer = null;
			return;
		}
		/*
		CachedPlayerControl? cached = AmongUsCache.AllPlayerControl.FirstOrDefault(
			p => p.Pointer == localPlayer.Pointer);
		if (cached != null)
		{
			PlayerControl.LocalPlayer = cached;
		}
		*/
	}
}