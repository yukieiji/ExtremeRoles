using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using HarmonyLib;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
public static class MeetingHudUpdateButtonsPatch
{
	public static bool Prefix(MeetingHud __instance)
	{
		if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return true; }

		var meeting = FastDestroyableSingleton<HudManager>.Instance.MeetingPrefab;

		__instance.amDead = false;
		__instance.Glass.sprite = meeting.Glass.sprite;
		__instance.Glass.color = meeting.Glass.color;

		for (int i = 0; i < __instance.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = __instance.playerStates[i];
			NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(
				playerVoteArea.TargetPlayerId);
			if (playerById == null)
			{
				playerVoteArea.SetDisabled();
			}
			else if (
				(playerById.Disconnected || playerById.IsDead) &&
				!playerVoteArea.XMark.gameObject.activeSelf)
			{
				playerVoteArea.XMark.gameObject.SetActive(true);
			}
		}
		return false;
	}

}
