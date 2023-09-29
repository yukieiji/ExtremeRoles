using HarmonyLib;

using ExtremeRoles.Module.SystemType;
using UnityEngine;
using ExtremeRoles.Extension;

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

	[HarmonyPrefix]
	[HarmonyPatch(typeof(LogicOptionsNormal), nameof(LogicOptionsNormal.GetDiscussionTime))]
	public static bool GetDiscussionTimePostfix(LogicOptionsNormal __instance, ref int __result)
	{
		__result = __instance.GameOptions.DiscussionTime;

		if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger &&
			ExtremeSystemTypeManager.Instance.TryGet<MeetingTimeChangeSystem>(
				ExtremeSystemType.MeetingTimeOffset, out var system) &&
			system is not null)
		{
			__result = Mathf.Clamp(
				__result + system.HudTimerOffset.Discussion,
				0, int.MaxValue);
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(LogicOptionsNormal), nameof(LogicOptionsNormal.GetVotingTime))]
	public static bool GetVotingTimePostfix(LogicOptionsNormal __instance, ref int __result)
	{
		__result = __instance.GameOptions.VotingTime;

		if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger &&
			ExtremeSystemTypeManager.Instance.TryGet<MeetingTimeChangeSystem>(
				ExtremeSystemType.MeetingTimeOffset, out var system) &&
			system is not null)
		{
			__result = Mathf.Clamp(
				__result + system.HudTimerOffset.Voting,
				0, int.MaxValue);
		}

		return false;
	}
}