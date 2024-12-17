using HarmonyLib;

using ExtremeRoles.GameMode;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Select))]
public static class MeetingHudSelectPatch
{
	private static bool isBlock = false;

	public static void SetSelectBlock(bool isBlockActive)
	{
		isBlock = isBlockActive;
	}

	public static bool Prefix(
		MeetingHud __instance,
		ref bool __result,
		[HarmonyArgument(0)] int suspectStateIdx)
	{
		__result = false;

		if (isBlock) { return false; }

		var shipOpt = ExtremeGameModeManager.Instance.ShipOption.Meeting;

		if (shipOpt.DisableSelfVote &&
			PlayerControl.LocalPlayer.PlayerId == suspectStateIdx)
		{
			return false;
		}
		if (shipOpt.IsBlockSkipInMeeting && suspectStateIdx == -1)
		{
			return false;
		}

		if (!OnemanMeetingSystemManager.TryGetActiveSystem(out var system)) { return true; }

		LogicOptionsNormal logicOptionsNormal = GameManager.Instance.LogicOptions.Cast<
			LogicOptionsNormal>();

		if (__instance.discussionTimer < (float)logicOptionsNormal.GetDiscussionTime())
		{
			return __result;
		}
		if (PlayerControl.LocalPlayer.PlayerId != system.Caller)
		{
			return __result;
		}

		SoundManager.Instance.PlaySound(
			__instance.VoteSound, false, 1f, null).volume = 0.8f;
		for (int i = 0; i < __instance.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = __instance.playerStates[i];
			if (suspectStateIdx != (int)playerVoteArea.TargetPlayerId)
			{
				playerVoteArea.ClearButtons();
			}
		}
		if (suspectStateIdx != -1)
		{
			__instance.SkipVoteButton.ClearButtons();
		}

		__result = true;
		return false;

	}
}