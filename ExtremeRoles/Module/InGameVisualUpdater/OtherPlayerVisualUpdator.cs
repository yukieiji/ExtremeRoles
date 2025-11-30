using System.Text;

using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Visual;

using CommomSystem = ExtremeRoles.Roles.API.Systems.Common;

#nullable enable

namespace ExtremeRoles.Module.InGameVisualUpdater;

public sealed class OtherPlayerVisualUpdator(
	PlayerControl local,
	PlayerControl target) :
	InGameVisualUpdatorBase(target)
{
	private readonly PlayerControl local = local;
	private readonly StringBuilder builder = new StringBuilder();

	public class BlockCondition(PlayerControl localPlayer)
	{
		private readonly NetworkedPlayerInfo data = localPlayer.Data;

		public bool IsBlockShowName { get; private set; }
		public bool IsBlockShowPlayerInfo { get; private set; }

		public void UpdateCondition(
			SingleRoleBase localRole,
			GhostRoleBase? localGhostRole)
		{
			this.IsBlockShowName = isBlockCondition(localRole) || localGhostRole != null;
			this.IsBlockShowPlayerInfo = localRole.IsBlockShowPlayingRoleInfo() || localGhostRole != null;

			if (localRole is MultiAssignRoleBase multiRole &&
				multiRole.AnotherRole != null)
			{
				this.IsBlockShowName = this.IsBlockShowName || isBlockCondition(multiRole.AnotherRole);
				this.IsBlockShowPlayerInfo =
				   this.IsBlockShowPlayerInfo || multiRole.AnotherRole.IsBlockShowPlayingRoleInfo();
			}
		}

		private bool isBlockCondition(SingleRoleBase role)
		{
			if (this.data.Role.Role == RoleTypes.GuardianAngel)
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

	private readonly BlockCondition condition = new BlockCondition(local);

	public override void Update()
	{
		SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
		GhostRoleBase ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();

		this.condition.UpdateCondition(role, ghostRole);

		byte id = this.PlayerId;
		if (!ExtremeRoleManager.TryGetRole(id, out var targetRole))
		{
			ExtremeRolesPlugin.Logger.LogError($"Role not found for PlayerId: {id} in HudManagerPatch.setPlayerNameColor");
			return;
		}
		GhostRoleBase? targetGhostRole;
		ExtremeGhostRoleManager.GameRole.TryGetValue(id, out targetGhostRole);

		reset();
		setNameColor(targetRole, targetGhostRole, role, ghostRole);
		setPlayerNameTag(targetRole, role);
		updateRoleInfo(targetRole, targetGhostRole, role, ghostRole);
	}

	private void reset()
	{
		SetNameText(this.PlayerName);
		SetNameColor(
			this.local.Data.Role.IsImpostor && this.Data.Role.IsImpostor ?
			Palette.ImpostorRed : Palette.White);
	}

	private void setNameColor(
		SingleRoleBase targetRole,
		GhostRoleBase? targetGhostRole,
		SingleRoleBase localRole,
		GhostRoleBase? localGhostRole)
	{
		if (!ClientOption.Instance.GhostsSeeRole.Value ||
			!this.local.Data.IsDead ||
			this.condition.IsBlockShowName)
		{
			var paintColor = localRole.GetTargetRoleSeeColor(targetRole, this.PlayerId);

			if (localGhostRole != null)
			{
				var paintGhostColor = localGhostRole.GetTargetRoleSeeColor(
					this.PlayerId, targetRole, targetGhostRole);

				if (paintGhostColor != Color.clear)
				{
					paintColor = (paintGhostColor / 2.0f) + (paintColor / 2.0f);
				}
			}

			if (paintColor == Palette.ClearWhite)
			{
				return;
			}

			SetNameColor(paintColor);
		}
		else
		{
			Color roleColor = targetRole.GetNameColor(true);

			if (!this.condition.IsBlockShowPlayerInfo)
			{
				SetNameColor(roleColor);
			}
		}
	}

	private void setPlayerNameTag(
		SingleRoleBase targetRole,
		SingleRoleBase localRole)
	{
		this.builder.Clear();
		this.builder.Append(this.NameText.text);
		this.builder.Append(localRole.GetRolePlayerNameTag(targetRole, this.PlayerId));
		if (targetRole.Visual is ILookedTag looked)
		{
			this.builder.Append(looked.GetLookedToThisTag(this.PlayerId));
		}
		SetNameText(this.builder.ToString());
	}

	private void updateRoleInfo(
		SingleRoleBase targetRole,
		GhostRoleBase? targetGhostRole,
		SingleRoleBase localRole,
		GhostRoleBase? localGhostRole)
	{
		var clientOption = ClientOption.Instance;
		bool isGhostSeeRole = clientOption.GhostsSeeRole.Value;
		bool isGhostSeeTask = clientOption.GhostsSeeTask.Value;

		if (!this.local.Data.IsDead ||
			this.condition.IsBlockShowPlayerInfo ||
			this.condition.IsBlockShowName ||
			!this.IsVisual ||
			(!isGhostSeeRole && !isGhostSeeTask))
		{
			this.Info.gameObject.SetActive(false);
			return;
		}

		this.Info.transform.localPosition =
			this.NameText.transform.localPosition + Vector3.up * InfoScale;

		var data = this.Data;
		var localPlayerData = this.local.Data;
		var (tasksCompleted, tasksTotal) = GameSystem.GetTaskInfo(data);

		string roleNames = targetRole.GetColoredRoleName(this.local.Data.IsDead);

		if (targetGhostRole is not null)
		{
			string ghostRoleName = targetGhostRole.GetColoredRoleName();
			roleNames = $"{ghostRoleName}({roleNames})";
		}

		string completedStr = IsCommActive(this.local) ? "?" : tasksCompleted.ToString();
		string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({completedStr}/{tasksTotal})</color>" : "";

		this.builder.Clear();
		
		if (isGhostSeeRole)
		{
			this.builder.Append(roleNames);
		}
		if (isGhostSeeTask)
		{
			this.builder.Append(taskInfo);
		}

		this.Info.text = this.builder.ToString();
		this.Info.gameObject.SetActive(true);
	}
}