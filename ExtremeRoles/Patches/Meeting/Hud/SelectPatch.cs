using HarmonyLib;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Extension.Il2Cpp;

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
		var shipOpt = ExtremeGameModeManager.Instance.ShipOption.Meeting;

		if (isBlock ||
			localPlayer == null ||
			(
				shipOpt.DisableSelfVote &&
				localPlayer.PlayerId == suspectStateIdx
			)
			||
			(
				shipOpt.IsBlockSkipInMeeting && suspectStateIdx == -1
			))
		{
			return false;
		}

		if (!OnemanMeetingSystemManager.TryGetActiveSystem(out var system))
		{
			if (MonikaTrashSystem.TryGet(out var monika) &&
				monika.InvalidPlayer(localPlayer.PlayerId))
			{
				return false;
			}

			return true;
		}

		var gm = GameManager.Instance;
		if (localPlayer.PlayerId != system.Caller ||
			gm == null ||
			gm.LogicOptions == null ||
			!gm.LogicOptions.IsTryCast<LogicOptionsNormal>(out var logicOptionsNormal) ||
			__instance.discussionTimer < (float)logicOptionsNormal.GetDiscussionTime())
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