using HarmonyLib;

using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud._CoIntro_d__52), nameof(MeetingHud._CoIntro_d__52.MoveNext))]
public static class MeetingHudCoIntroPatch
{
	public static void Postfix(
		MeetingHud._CoIntro_d__52 __instance, ref bool __result)
	{
		if (__result || ExtremeRoleManager.GameRole.Count == 0)
		{
			return;
		}

		if (!OnemanMeetingSystemManager.IsActive)
		{
			var reportedBody = __instance.reportedBody;
			var reporter = __instance.reporter;

			var player = PlayerControl.LocalPlayer;
			var localRole = ExtremeRoleManager.GetLocalPlayerRole();
			var hookRole = localRole as IRoleReportHook;

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
			if (localRole is MultiAssignRoleBase multiAssignRole)
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
			__instance.__4__this.TitleText.text = Tr.GetString("whoIsMarine");
		}
	}
}
