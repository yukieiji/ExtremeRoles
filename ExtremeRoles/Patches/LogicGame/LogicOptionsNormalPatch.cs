using HarmonyLib;

using ExtremeRoles.Module.SystemType;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Patches.LogicGame;


[HarmonyPatch]
public static class MeetingHudTimerOffsetPatch
{
	public static int NoModDiscussionTime
	{
		get
		{
			var gm = GameManager.Instance;
			if (gm == null)
			{
				return 0;
			}
			var normalOption = GameManager.Instance.LogicOptions.TryCast<LogicOptionsNormal>();
			if (normalOption == null)
			{
				return 0;
			}
			return normalOption.GameOptions.DiscussionTime;
		}
	}
	public static int NoModVotingTime
	{
		get
		{
			var gm = GameManager.Instance;
			if (gm == null)
			{
				return 0;
			}
			var normalOption = GameManager.Instance.LogicOptions.TryCast<LogicOptionsNormal>();
			if (normalOption == null)
			{
				return 0;
			}
			return normalOption.GameOptions.VotingTime;
		}
	}

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
		__result = Mathf.Clamp(
			__result + system.HudTimerOffset.Discussion,
			0, int.MaxValue);
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
		__result = Mathf.Clamp(
			__result + system.HudTimerOffset.Voting,
			0, int.MaxValue);
	}
}