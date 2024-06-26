using HarmonyLib;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CoIntro))]
public static class MeetingHudCoIntroPatch
{
	public static void Postfix(
		MeetingHud __instance,
		[HarmonyArgument(0)] NetworkedPlayerInfo reporter,
		[HarmonyArgument(1)] NetworkedPlayerInfo reportedBody)
	{
		if (ExtremeRoleManager.GameRole.Count == 0) { return; }

		if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger)
		{
			var player = CachedPlayerControl.LocalPlayer;
			var hookRole = ExtremeRoleManager.GetLocalPlayerRole() as IRoleReportHook;
			var multiAssignRole = ExtremeRoleManager.GetLocalPlayerRole() as MultiAssignRoleBase;

			if (hookRole != null)
			{
				if (reportedBody == null)
				{
					hookRole.HookReportButton(
						player, reporter);
				}
				else
				{
					hookRole.HookBodyReport(
						player, reporter, reportedBody);
				}
			}
			if (multiAssignRole != null)
			{
				hookRole = multiAssignRole.AnotherRole as IRoleReportHook;
				if (hookRole != null)
				{

					if (reportedBody == null)
					{
						hookRole.HookReportButton(
							player, reporter);
					}
					else
					{
						hookRole.HookBodyReport(
							player, reporter, reportedBody);
					}
				}
			}

		}
		else
		{
			__instance.TitleText.text = Helper.Translation.GetString("whoIsMarine");
		}
	}
}
