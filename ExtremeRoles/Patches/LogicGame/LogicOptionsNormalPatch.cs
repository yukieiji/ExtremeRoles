using HarmonyLib;

using ExtremeRoles.Module.SystemType;
using UnityEngine;
using ExtremeRoles.Extension;
using ExtremeRoles.Module.ExtremeShipStatus;

#nullable enable

namespace ExtremeRoles.Patches.LogicGame;


[HarmonyPatch]
public static class MeetingHudTimerOffsetPatch
{
	public static int NoModDiscussionTime
		=> tryGetNormalOption(out var normalOption) ?
		 normalOption!.GameOptions.DiscussionTime : 0;

	public static int NoModVotingTime
		=> tryGetNormalOption(out var normalOption) ?
		 normalOption!.GameOptions.VotingTime : 0;

	private static bool tryGetNormalOption(out LogicOptionsNormal? normalOption)
	{
		normalOption = null;

		return
			GameManager.Instance != null &&
			GameManager.Instance.LogicOptions.IsTryCast(out normalOption);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(LogicOptionsNormal), nameof(LogicOptionsNormal.GetDiscussionTime))]
	public static void GetDiscussionTimePostfix(ref int __result)
	{
		if (ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger ||
			!ExtremeSystemTypeManager.Instance.TryGet<MeetingTimeChangeSystem>(
			ExtremeSystemType.MeetingTimeOffset, out var system) ||
			system is null)
		{
			return;
		}
		__result = Mathf.Clamp(
			__result + system.HudTimerOffset.Discussion,
			0, int.MaxValue);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(LogicOptionsNormal), nameof(LogicOptionsNormal.GetVotingTime))]
	public static void GetVotingTimePostfix(ref int __result)
	{
		if (ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger ||
			!ExtremeSystemTypeManager.Instance.TryGet<MeetingTimeChangeSystem>(
			ExtremeSystemType.MeetingTimeOffset, out var system) ||
			system is null)
		{
			return;
		}
		__result = Mathf.Clamp(
			__result + system.HudTimerOffset.Voting,
			0, int.MaxValue);
	}
}