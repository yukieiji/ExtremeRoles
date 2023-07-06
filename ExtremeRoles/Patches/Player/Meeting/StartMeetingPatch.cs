using HarmonyLib;

namespace ExtremeRoles.Patches.Player.Meeting;

#nullable enable

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
public static class PlayerControlCoStartMeetingPatch
{
				public static void Prefix([HarmonyArgument(0)] GameData.PlayerInfo target)
				{
								InfoOverlay.Instance.BlockShow(true);

								var state = ExtremeRolesPlugin.ShipState;

								if (state.AssassinMeetingTrigger) { return; }

								// Count meetings
								if (target == null)
								{
								state.IncreaseMeetingCount();
								}
				}
}
