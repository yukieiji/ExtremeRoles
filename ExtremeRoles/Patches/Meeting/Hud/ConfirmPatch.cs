using HarmonyLib;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Confirm))]
public static class MeetingHudConfirmPatch
{
	public static bool Prefix(
		MeetingHud __instance,
		[HarmonyArgument(0)] byte suspectStateIdx)
	{
		if (!OnemanMeetingSystemManager.TryGetActiveSystem(out var system))
		{
			return true;
		}

		if (PlayerControl.LocalPlayer.PlayerId != system.Caller)
		{
			return false;
		}

		for (int i = 0; i < __instance.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = __instance.playerStates[i];
			playerVoteArea.ClearButtons();
			playerVoteArea.VoteComplete = true;
		}
		__instance.SkipVoteButton.ClearButtons();
		__instance.SkipVoteButton.VoteComplete = true;
		__instance.SkipVoteButton.gameObject.SetActive(false);

		MeetingHud.MeetingStates voteStates = __instance.state;
		if (voteStates != MeetingHud.MeetingStates.NotVoted)
		{
			return false;
		}
		__instance.state = MeetingHud.MeetingStates.Voted;
		__instance.CmdCastVote(
			PlayerControl.LocalPlayer.PlayerId, suspectStateIdx);

		return false;
	}
	public static void Postfix(
		MeetingHud __instance,
		[HarmonyArgument(0)] byte suspectStateIdx)
	{
		if (__instance.state != MeetingHud.MeetingStates.Voted ||
			OnemanMeetingSystemManager.IsActive)
		{
			return;
		}

		var (voteCheckRole, anotherVoteCheckRole) = ExtremeRoleManager.GetLocalRoleAbility<
			IVoteCheck>();
		voteCheckRole?.VoteTo(suspectStateIdx);
		anotherVoteCheckRole?.VoteTo(suspectStateIdx);
	}
}
