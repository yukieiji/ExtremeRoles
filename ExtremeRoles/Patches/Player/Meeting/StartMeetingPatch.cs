using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using HarmonyLib;

namespace ExtremeRoles.Patches.Player.Meeting;

#nullable enable

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
public static class PlayerControlCoStartMeetingPatch
{
	public static void Prefix([HarmonyArgument(0)] NetworkedPlayerInfo target)
	{
		InfoOverlay.Instance.IsBlock = true;

		if (OnemanMeetingSystemManager.IsActive) { return; }

		// Count meetings
		if (target == null &&
			ExtremeSystemTypeManager.Instance.TryGet<MeetingCountSystem>(
				ExtremeSystemType.MeetingCount,
				out var countSystem))
		{
			countSystem.Increse();
		}
	}
}
