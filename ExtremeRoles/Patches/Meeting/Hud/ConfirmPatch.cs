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

		if (!system.IsIgnoreDeadPlayer)
		{
			return true;
		}

		for (int i = 0; i < __instance.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = __instance.playerStates[i];
			playerVoteArea.ClearButtons();
			playerVoteArea.voteComplete = true;
		}
		__instance.SkipVoteButton.ClearButtons();
		__instance.SkipVoteButton.voteComplete = true;
		__instance.SkipVoteButton.gameObject.SetActive(false);

		MeetingHud.VoteStates voteStates = __instance.state;
		if (voteStates != MeetingHud.VoteStates.NotVoted)
		{
			return false;
		}
		__instance.state = MeetingHud.VoteStates.Voted;
		__instance.CmdCastVote(
			PlayerControl.LocalPlayer.PlayerId, suspectStateIdx);

		return false;
	}
	public static void Postfix(
		MeetingHud __instance,
		[HarmonyArgument(0)] byte suspectStateIdx)
	{
		if (__instance.state != MeetingHud.VoteStates.Voted ||
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
