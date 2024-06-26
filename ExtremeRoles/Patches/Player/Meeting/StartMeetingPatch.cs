﻿using HarmonyLib;

namespace ExtremeRoles.Patches.Player.Meeting;

#nullable enable

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
public static class PlayerControlCoStartMeetingPatch
{
	public static void Prefix([HarmonyArgument(0)] NetworkedPlayerInfo target)
	{
		InfoOverlay.Instance.IsBlock = true;

		var state = ExtremeRolesPlugin.ShipState;

		if (state.AssassinMeetingTrigger) { return; }

		// Count meetings
		if (target == null)
		{
			state.IncreaseMeetingCount();
		}
	}
}
