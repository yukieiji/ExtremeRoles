using System;
using System.Collections.Generic;

using ExtremeRoles.Module;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Combination.InvestigatorOffice;

public sealed class Assistant : MultiAssignRoleBase, IRoleMurderPlayerHook, IRoleReportHook, IRoleSpecialReset
{
	private Dictionary<byte, DateTime> deadBodyInfo;
	public Assistant() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Assistant,
			ColorPalette.AssistantBluCapri),
		false, true, false, false,
		tab: OptionTab.CombinationTab)
	{ }

	public void AllReset(PlayerControl rolePlayer)
	{
		downgradeDetective();
	}

	public void HookMuderPlayer(
		PlayerControl source, PlayerControl target)
	{
		if (MeetingHud.Instance == null)
		{
			deadBodyInfo.Add(target.PlayerId, DateTime.UtcNow);
		}
	}

	public void HookReportButton(
		PlayerControl rolePlayer,
		NetworkedPlayerInfo reporter)
	{
		deadBodyInfo.Clear();
	}

	public void HookBodyReport(
		PlayerControl rolePlayer,
		NetworkedPlayerInfo reporter,
		NetworkedPlayerInfo reportBody)
	{
		if (!(
				ExtremeRoleManager.TryGetRole(rolePlayer.PlayerId, out var role) &&
				IsSameControlId(role) &&
				this.deadBodyInfo.TryGetValue(reportBody.PlayerId, out var info)
			))
		{
			return;
		}
		HudManager.Instance.Chat.AddChat(
			PlayerControl.LocalPlayer,
			Tr.GetString(
				"reportedDeadBodyInfo",
				info.ToString()));
		deadBodyInfo.Clear();
	}

	public override void RolePlayerKilledAction(
		PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		downgradeDetective();
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		downgradeDetective();
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{ }

	protected override void RoleSpecificInit()
	{
		deadBodyInfo = new Dictionary<byte, DateTime>();
	}
	private void downgradeDetective()
	{
		foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
		{
			if (role.Core.Id != ExtremeRoleId.Investigator ||
				!IsSameControlId(role))
			{
				continue;
			}

			var playerInfo = GameData.Instance.GetPlayerById(playerId);
			if (!playerInfo.IsDead && !playerInfo.Disconnected)
			{
				InvestigatorApprentice.ChangeToDetectiveApprentice(playerId);
				break;
			}
		}
	}
}
