using HarmonyLib;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
public static class MeetingHudUpdateButtonsPatch
{
	public static bool Prefix(MeetingHud __instance)
	{
		if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return true; }

		if (AmongUsClient.Instance.AmHost)
		{
			for (int i = 0; i < __instance.playerStates.Length; i++)
			{
				PlayerVoteArea playerVoteArea = __instance.playerStates[i];
				GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(
					playerVoteArea.TargetPlayerId);
				if (playerById == null)
				{
					playerVoteArea.SetDisabled();
				}
				else
				{
					playerVoteArea.SetDead(
						__instance.reporterId == playerById.PlayerId, false, false);
					__instance.SetDirtyBit(1U);
				}
			}
		}

		return false;
	}

}
