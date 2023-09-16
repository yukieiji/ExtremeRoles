using HarmonyLib;

using ExtremeRoles.Module.SystemType;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Patches.LogicGame;


[HarmonyPatch]
public static class MeetingHudTimerOffsetPatch
{
	public static int NoModDiscussionTime
		=> GameManager.Instance.LogicOptions is LogicOptionsNormal normalOption ? normalOption.GameOptions.DiscussionTime : 0;
	public static int NoModVotingTime
		=> GameManager.Instance.LogicOptions is LogicOptionsNormal normalOption ? normalOption.GameOptions.VotingTime : 0;


	[HarmonyPostfix]
	[HarmonyPatch(typeof(LogicOptionsNormal), nameof(LogicOptionsNormal.GetDiscussionTime))]
	public static void GetDiscussionTimePostfix(ref int __result)
	{
		if (!ExtremeSystemTypeManager.Instance.TryGet<MeetingTimeChangeSystem>(
			ExtremeSystemType.MeetingTimeOffset, out var system) ||
			system is null)
		{
			return;
		}
		__result += system.HudTimerOffset.Discussion;
		__result = Mathf.Clamp(__result, 0, int.MaxValue);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(LogicOptionsNormal), nameof(LogicOptionsNormal.GetVotingTime))]
	public static void GetVotingTimePostfix(ref int __result)
	{
		if (!ExtremeSystemTypeManager.Instance.TryGet<MeetingTimeChangeSystem>(
			ExtremeSystemType.MeetingTimeOffset, out var system) ||
			system is null)
		{
			return;
		}
		__result += system.HudTimerOffset.Voting;
		__result = Mathf.Clamp(__result, 0, int.MaxValue);
	}
}