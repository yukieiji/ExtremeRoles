using HarmonyLib;

using UnityEngine;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomMonoBehaviour;

namespace ExtremeRoles.Patches.Meeting.Hud;

[HarmonyPatch(typeof(VoteSpreader), nameof(VoteSpreader.AddVote))]
public static class VoteSpreaderPatch
{
	public static bool Prefix(
		VoteSpreader __instance,
		[HarmonyArgument(0)] SpriteRenderer newVote)
	{
		if (true)
		{
			return true;
		}
		/* スワッパー用のシステムから取得する
		var swapper = __instance.gameObject.TryAddComponent<VoteSwapper>();
		swapper.Add(newVote, );
		*/
		return false;
	}
}
