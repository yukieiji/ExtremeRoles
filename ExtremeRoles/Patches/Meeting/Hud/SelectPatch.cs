using HarmonyLib;

using ExtremeRoles.GameMode;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;

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

		var localPlayer = PlayerControl.LocalPlayer;
		if (isBlock || localPlayer == null)
		{
			return false;
		}

		var shipOpt = ExtremeGameModeManager.Instance.ShipOption.Meeting;

		if (shipOpt.DisableSelfVote &&
			localPlayer.PlayerId == suspectStateIdx)
		{
			return false;
		}
		else if (shipOpt.IsBlockSkipInMeeting && suspectStateIdx == -1)
		{
			return false;
		}

		if (MonikaTrashSystem.TryGet(out var monika) &&
			monika.InvalidPlayer(localPlayer.PlayerId))
		{
			return false;
		}

		if (!OnemanMeetingSystemManager.TryGetActiveSystem(out var system))
		{
			return true;
		}

		LogicOptionsNormal logicOptionsNormal = GameManager.Instance.LogicOptions.Cast<
			LogicOptionsNormal>();

		if (__instance.discussionTimer < (float)logicOptionsNormal.GetDiscussionTime() ||
			localPlayer.PlayerId != system.Caller)
		{
			return false;
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