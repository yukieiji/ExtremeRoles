using AmongUs.GameOptions;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Visual;
using System.Text;
using TMPro;
using UnityEngine;

using CommomSystem = ExtremeRoles.Roles.API.Systems.Common;

namespace ExtremeRoles.Module.Event;

#nullable enable

public sealed class MeetingStatus(PlayerVoteArea pva, bool isCommActive)
{
	public NetworkedPlayerInfo Local { get; } = PlayerControl.LocalPlayer.Data;
	public bool IsCommActive { get; } = isCommActive;
	public TextMeshPro? Name { get; } = pva.NameText;
	public TextMeshPro? Info { get; } = createInfo(pva.NameText);

	private static TextMeshPro createInfo(TextMeshPro name)
	{
		var text = Object.Instantiate(
			name, name.transform);
		text.transform.localPosition += Vector3.down * 0.20f + Vector3.left * 0.30f;
		text.fontSize *= 0.63f;
		text.autoSizeTextContainer = false;
		text.gameObject.name = "VoteAreaInfo";

		return text;
	}
}

public sealed class LocalPlayerMeetingVisualUpdateEvent(
	MeetingStatus status) : ISubscriber
{
	private readonly MeetingStatus status = status;

	public bool Invoke()
	{
		if (MeetingHud.Instance == null ||
			status.Info == null ||
			status.Name == null)
		{
			return false;
		}


		SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
		GhostRoleBase? ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();

		resetInfo(status.Name, status.Local);
		setTag(status.Name, status.Local, role);
		setColor(status.Name, status.Local, role, ghostRole);
		setInfo(status.Info, status.Local, status.IsCommActive, role, ghostRole);
		return true;
	}

	private static void resetInfo(TextMeshPro name, NetworkedPlayerInfo local)
	{
		name.text = local.PlayerName;
		name.color = local.Role.IsImpostor ? Palette.ImpostorRed : Palette.White;
	}

	private static void setColor(
		TextMeshPro name,
		NetworkedPlayerInfo local,
		SingleRoleBase role,
		GhostRoleBase? ghostRole)
	{
		Color paintColor = role.GetNameColor(
			local.IsDead);
		if (ghostRole is not null)
		{
			Color ghostRoleColor = ghostRole.Color;
			paintColor = (paintColor / 2.0f) + (ghostRoleColor / 2.0f);
		}
		if (paintColor == Palette.ClearWhite)
		{
			return;
		}

		name.color = paintColor;
	}

	private static void setInfo(
		TextMeshPro info,
		NetworkedPlayerInfo local,
		bool isCommActive,
		SingleRoleBase role,
		GhostRoleBase? ghostRole)
	{
		info.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ?
			"" : getMeetingInfo(local, isCommActive, role, ghostRole);
		info.gameObject.SetActive(true);
	}

	private static string getMeetingInfo(
		NetworkedPlayerInfo local,
		bool isCommActive,
		SingleRoleBase role,
		GhostRoleBase? ghostRole)
	{
		var (tasksCompleted, tasksTotal) = GameSystem.GetTaskInfo(local);
		string roleNames = role.GetColoredRoleName(local.IsDead);

		if (ghostRole is not null)
		{
			string ghostRoleName = ghostRole.GetColoredRoleName();
			roleNames = $"{ghostRoleName}({roleNames})";
		}

		string completedStr = isCommActive ? "?" : tasksCompleted.ToString();
		string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({completedStr}/{tasksTotal})</color>" : "";

		return $"{roleNames} {taskInfo}".Trim();
	}

	private static void setTag(
		TextMeshPro name,
		NetworkedPlayerInfo local,
		SingleRoleBase role)
	{
		string tag = role.GetRolePlayerNameTag(
			role, local.PlayerId);
		if (tag == string.Empty)
		{
			return;
		}
		name.text += tag;
	}
}


public sealed class OtherPlayerMeetingVisualUpdateEvent(
	NetworkedPlayerInfo target,
	MeetingStatus status) : ISubscriber
{
	private readonly MeetingStatus status = status;
	private readonly NetworkedPlayerInfo target = target;
	private readonly StringBuilder builder = new StringBuilder();

	public bool Invoke()
	{
		if (MeetingHud.Instance == null ||
			status.Info == null ||
			status.Name == null ||
			!ExtremeRoleManager.TryGetRole(
				this.target.PlayerId, out SingleRoleBase? targetRole))
		{
			return false;
		}


		resetInfo(status.Name, status.Local);

		SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();

		GhostRoleBase? ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
		ExtremeGhostRoleManager.GameRole.TryGetValue(
			this.target.PlayerId, out GhostRoleBase? targetGhostRole);
		bool isLocalPlayerGhostRole = ghostRole != null;

		bool blockCondition = isBlockCondition(status.Local, role) || isLocalPlayerGhostRole;
		bool meetingInfoBlock = role.IsBlockShowMeetingRoleInfo() || isLocalPlayerGhostRole;

		if (role is MultiAssignRoleBase multiRole &&
			multiRole.AnotherRole != null)
		{
			blockCondition = blockCondition || isBlockCondition(status.Local, multiRole.AnotherRole);
			meetingInfoBlock =
				meetingInfoBlock || multiRole.AnotherRole.IsBlockShowMeetingRoleInfo();
		}

		setPlayerNameTag(status.Name, role, targetRole);

		setMeetingInfo(
			targetRole,
			targetGhostRole,
			meetingInfoBlock,
			blockCondition);

		setNameColor(
			status.Local,
			status.Name,
			role,
			targetRole,
			ghostRole,
			targetGhostRole,
			meetingInfoBlock,
			blockCondition);
		return true;
	}

	private void resetInfo(
		TextMeshPro name, NetworkedPlayerInfo local)
	{
		name.text = this.target.PlayerName;
		name.color =
			local.Role.IsImpostor && this.target.Role.IsImpostor ?
			Palette.ImpostorRed : Palette.White;
	}

	private void setNameColor(
		NetworkedPlayerInfo local,
		TextMeshPro text,
		SingleRoleBase localRole,
		SingleRoleBase targetRole,
		GhostRoleBase? localGhostRole,
		GhostRoleBase? targetGhostRole,
		bool isMeetingInfoBlock,
		bool blockCondition)
	{

		byte targetPlayerId = this.target.PlayerId;

		if (!ClientOption.Instance.GhostsSeeRole.Value ||
			!local.IsDead ||
			blockCondition)
		{
			Color paintColor = localRole.GetTargetRoleSeeColor(
				targetRole, targetPlayerId);
			if (localGhostRole is not null)
			{
				Color paintGhostColor = localGhostRole.GetTargetRoleSeeColor(
					targetPlayerId, targetRole, targetGhostRole);

				if (paintGhostColor != Color.clear)
				{
					paintColor = (paintGhostColor / 2.0f) + (paintColor / 2.0f);
				}
			}

			if (paintColor == Palette.ClearWhite)
			{
				return;
			}

			text.color = paintColor;

		}
		else
		{
			Color roleColor = targetRole.GetNameColor(true);

			// インポスター同士は見える
			if (!isMeetingInfoBlock ||
				(localRole.IsImpostor() && targetRole.IsImpostor()))
			{
				text.color = roleColor;
			}
		}
	}

	private void setMeetingInfo(
		SingleRoleBase targetRole,
		GhostRoleBase? targetGhostRole,
		bool isMeetingInfoBlock,
		bool blockCondition)
	{

		var info = this.status.Info;
		if (info == null)
		{
			return;
		}

		if (!this.status.Local.IsDead ||
			blockCondition)
		{
			info.gameObject.SetActive(false);
		}
		else
		{
			info.text =
				MeetingHud.Instance.state == MeetingHud.VoteStates.Results ?
				"" : getMeetingInfo(targetRole, targetGhostRole);
			info.gameObject.SetActive(!isMeetingInfoBlock);
		}
	}

	private string getMeetingInfo(
		SingleRoleBase targetRole,
		GhostRoleBase? targetGhostRole)
	{
		var (tasksCompleted, tasksTotal) = GameSystem.GetTaskInfo(this.target);
		string roleNames = targetRole.GetColoredRoleName(this.status.Local.IsDead);

		if (targetGhostRole is not null)
		{
			string ghostRoleName = targetGhostRole.GetColoredRoleName();
			roleNames = $"{ghostRoleName}({roleNames})";
		}

		string completedStr = this.status.IsCommActive ? "?" : tasksCompleted.ToString();
		string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({completedStr}/{tasksTotal})</color>" : "";

		string meetingInfoText = "";

		var clientOption = ClientOption.Instance;
		bool isGhostSeeRole = clientOption.GhostsSeeRole.Value;
		bool isGhostSeeTask = clientOption.GhostsSeeTask.Value;

		if (isGhostSeeRole && isGhostSeeTask)
		{
			meetingInfoText = $"{roleNames} {taskInfo}".Trim();
		}
		else if (isGhostSeeTask)
		{
			meetingInfoText = $"{taskInfo}".Trim();
		}
		else if (isGhostSeeRole)
		{
			meetingInfoText = $"{roleNames}";
		}
		return meetingInfoText;
	}

	private void setPlayerNameTag(
		TextMeshPro text,
		SingleRoleBase localRole,
		SingleRoleBase targetRole)
	{
		this.builder.Clear();
		this.builder.Append(text.text);
		this.builder.Append(localRole.GetRolePlayerNameTag(targetRole, this.target.PlayerId));
		if (targetRole.Visual is ILookedTag looked)
		{
			this.builder.Append(looked.GetLookedToThisTag(this.target.PlayerId));
		}
		text.text = this.builder.ToString();
	}

	private bool isBlockCondition(
		NetworkedPlayerInfo local,
		SingleRoleBase role)
	{
		if (local.Role.Role is RoleTypes.GuardianAngel)
		{
			return true;
		}
		else if (CommomSystem.IsForceInfoBlockRole(role))
		{
			return ExtremeRolesPlugin.ShipState.IsAssassinAssign;
		}
		return false;
	}
}