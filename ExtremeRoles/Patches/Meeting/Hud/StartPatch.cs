using System.Text;

using HarmonyLib;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Performance;

using PlayerHeler = ExtremeRoles.Helper.Player;
using SoundHelper = ExtremeRoles.Helper.Sound;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class MeetingHudStartPatch
{
	public static void Postfix(MeetingHud __instance)
	{

		var state = ExtremeRolesPlugin.ShipState;
		bool trigger = OnemanMeetingSystemManager.TryGetActiveOnemanMeeting(out var onemanMeeting);
		var builder = new StringBuilder();

		builder
			.AppendLine("------ MeetingHud Start!! -----")
			.AppendLine(" - Meeting info:");

		if (GameManager.Instance.LogicOptions.IsTryCast<LogicOptionsNormal>(out var opt))
		{
			builder
				.Append("   - Discussion Time:")
				.Append(opt.GetDiscussionTime())
				.AppendLine()

				.Append("   - Voting Time:")
				.Append(opt.GetVotingTime())
				.AppendLine();
		}

		builder
			.Append("   - Oneman Meeting:")
			.Append(trigger);
		if (onemanMeeting != null)
		{
			builder.Append($" {nameof(onemanMeeting)}");
		}
		else
		{
			builder.Append($" UNKOWN");
		}
		builder.AppendLine();

		if (!trigger &&
			ExtremeSystemTypeManager.Instance.TryGet<ModdedMeetingTimeSystem>(
				ExtremeSystemType.ModdedMeetingTimeSystem, out var system))
		{
			__instance.VoteEndingSound = SoundHelper.GetAudio(SoundHelper.Type.NullSound);
			__instance.discussionTimer -= system.HudTimerStartOffset;
			if (!system.IsShowTimer)
			{
				var textObj = __instance.TimerText.gameObject;
				textObj.SetActive(false);
				textObj.AddComponent<AutoDisabler>();
			}

			builder
				.AppendLine("   - TimeOffset System: Enable")
				.Append("     - DiscussionTimer start at:").Append(__instance.discussionTimer);

		}
		else
		{
			builder.Append("   - TimeOffset System: Disable");
		}

		var logger = ExtremeRolesPlugin.Logger;

		logger.LogInfo(builder.ToString());

		logger.LogInfo(" --- Start Meeting Start Reseting --- ");

		logger.LogInfo("Resetting Start: ShipStatus Systems");
		ExtremeSystemTypeManager.Instance.Reset(null, (byte)ResetTiming.MeetingStart);
		logger.LogInfo("Resetting End: ShipStatus Systems");

		logger.LogInfo("Resetting Start: PlayerControl");
		PlayerHeler.ResetTarget();
		logger.LogInfo("Resetting End: PlayerControl");

		logger.LogInfo("Resetting Start: Modding MeetingHud system");
		MeetingHudSelectPatch.SetSelectBlock(false);
		logger.LogInfo("Resetting End: Modding MeetingHud system");

		if (ExtremeRoleManager.GameRole.Count == 0) { return; }

		logger.LogInfo("Resetting Start: ExR Normal and Combination Roles");
		var role = ExtremeRoleManager.GetLocalPlayerRole();

		if (role is IRoleAbility abilityRole)
		{
			abilityRole.Button.OnMeetingStart();
		}
		if (role is IRoleResetMeeting resetRole)
		{
			resetRole.ResetOnMeetingStart();
		}
		if (role is MultiAssignRoleBase multiAssignRole)
		{
			if (multiAssignRole.AnotherRole is IRoleAbility multiAssignAbilityRole)
			{
				multiAssignAbilityRole.Button.OnMeetingStart();
			}
			if (multiAssignRole.AnotherRole is IRoleResetMeeting multiAssignResetRole)
			{
				multiAssignResetRole.ResetOnMeetingStart();
			}
		}
		logger.LogInfo("Resetting End: ExR Normal and Combination Roles");

		logger.LogInfo("Resetting Start: ExR Ghost Roles");
		var ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
		if (ghostRole != null)
		{
			ghostRole.ResetOnMeetingStart();
		}
		logger.LogInfo("Resetting End: ExR Ghost Roles");

		if (!trigger) { return; }

		FastDestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(false);
	}
}
