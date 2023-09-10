using HarmonyLib;


using AmongUs.GameOptions;


using ExtremeRoles.Performance;
using ExtremeRoles.Module.SystemType;

#nullable enable

namespace ExtremeRoles.Patches.LogicGame;


[HarmonyPatch]
public static class MeetingHudTimerOffsetPatch
{
	public static float NoModDiscussionTime
		=> GameManager.Instance.LogicOptions is LogicOptionsNormal normalOption ? normalOption.GameOptions.DiscussionTime : 0.0f;
	public static float NoModVotingTime
		=> GameManager.Instance.LogicOptions is LogicOptionsNormal normalOption ? normalOption.GameOptions.VotingTime : 0.0f;


	[HarmonyPostfix]
	[HarmonyPatch(typeof(LogicOptionsNormal), nameof(LogicOptionsNormal.GetDiscussionTime))]
	public static void GetDiscussionTimePostfix(ref float __result)
	{
		// 議論時間が投票時間より長いのでそちらの秒数を変更する
		if (!ExtremeSystemTypeManager.Instance.TryGet<MeetingTimeChangeSystem>(
			ExtremeSystemType.MeetingTimeOffset, out var system) ||
			system is null)
		{
			return;
		}
		__result = system.HudTimerOffset.Discussion;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(LogicOptionsNormal), nameof(LogicOptionsNormal.GetVotingTime))]
	public static void GetVotingTimePostfix(ref float __result)
	{
		// 投票時間が議論時間より長いのでそちらの秒数を変更する
		if (!ExtremeSystemTypeManager.Instance.TryGet<MeetingTimeChangeSystem>(
			ExtremeSystemType.MeetingTimeOffset, out var system) ||
			system is null)
		{
			return;
		}
		__result = system.HudTimerOffset.Voting;
	}
}