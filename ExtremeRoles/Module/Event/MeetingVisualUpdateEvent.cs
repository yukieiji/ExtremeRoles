using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

using TMPro;
using UnityEngine;

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

public sealed class MeetingVisualUpdateEvent(MeetingStatus status) : ISubscriber
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

		return $"{roleNames} {taskInfo}".Trim(); ;
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
