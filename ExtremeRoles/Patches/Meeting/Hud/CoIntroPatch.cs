using HarmonyLib;

using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud._CoIntro_d__53), nameof(MeetingHud._CoIntro_d__53.MoveNext))]
public static class MeetingHudCoIntroPatch
{
	public const float MeetingAbilityButtonXOffset = 1.0f;
	public static void Postfix(
		MeetingHud._CoIntro_d__53 __instance, ref bool __result)
	{
		if (__result || ExtremeRoleManager.GameRole.Count == 0 || OnemanMeetingSystemManager.IsActive)
		{
			return;
		}

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
		// 会議周りで発動する能力ボタンの位置を調整
		if ((
				!MonikaTrashSystem.TryGet(out var monikaSystem) ||
				!monikaSystem.InvalidPlayer(PlayerControl.LocalPlayer.PlayerId)
			) &&
			ExtremeSystemTypeManager.Instance.ExistSystem(ExtremeSystemType.RaiseHandSystem) &&
			__instance.__4__this.MeetingAbilityButton != null)
		{
			var button = __instance.__4__this.MeetingAbilityButton;
			ExtremeRolesPlugin.Logger.LogInfo("Change: MeetingAbilityButton Pos");
			var curPos = button.transform.localPosition;
			button.transform.localPosition = curPos + new UnityEngine.Vector3(MeetingAbilityButtonXOffset, 0.0f);
		}
	}
}
