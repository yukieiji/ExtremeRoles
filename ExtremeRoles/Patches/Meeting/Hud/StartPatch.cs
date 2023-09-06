using HarmonyLib;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Performance;

using PlayerHeler = ExtremeRoles.Helper.Player;
using ExtremeRoles.Module.SystemType;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class MeetingHudStartPatch
{
	public static void Postfix()
	{
		ExtremeRolesPlugin.ShipState.ClearMeetingResetObject();
		ExtremeSystemTypeManager.Instance.RepairDamage(null, (byte)ResetTiming.MeetingStart);
		PlayerHeler.ResetTarget();
		MeetingHudSelectPatch.SetSelectBlock(false);

		if (ExtremeRoleManager.GameRole.Count == 0) { return; }

		var role = ExtremeRoleManager.GetLocalPlayerRole();

		if (role is IRoleAbility abilityRole)
		{
			abilityRole.Button.OnMeetingStart();
		}
		if (role is IRoleResetMeeting resetRole)
		{
			resetRole.ResetOnMeetingStart();
		}

		var multiAssignRole = role as MultiAssignRoleBase;
		if (multiAssignRole != null)
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

		var ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
		if (ghostRole != null)
		{
			ghostRole.ResetOnMeetingStart();
		}

		if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return; }

		FastDestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(false);
	}
}
