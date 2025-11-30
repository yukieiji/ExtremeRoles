using TMPro;
using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Module.InGameVisualUpdater;

public sealed class LocalPlayerVisualUpdater(PlayerControl local) : InGameVisualUpdaterBase(local)
{
	private TextMeshPro? tabText;

	public override void Update()
	{
		SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
		GhostRoleBase? ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();

		reset();
		setNameColor(role, ghostRole);
		setTag(role);
		updateRoleInfo(role, ghostRole);

		// ローカルの処理
		roleUpdate(role);

		if (role is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole != null)
		{
			roleUpdate(multiAssignRole.AnotherRole);
			multiAssignRole.OverrideAnotherRoleSetting();
		}
		/* TODO:幽霊役職タスク
        if (ghostRole != null)
        {
            role.HasTask = role.HasTask && ghostRole.HasTask;
        }
        */
	}

	private void reset()
	{
		SetNameText(this.PlayerName);
		SetNameColor(this.Data.Role.IsImpostor ? Palette.ImpostorRed : Palette.White);
	}

	private void setNameColor(SingleRoleBase localRole, GhostRoleBase? ghostRole)
	{
		// Modules.Helpers.DebugLog($"Player Name:{role.NameColor}");

		// まずは自分のプレイヤー名の色を変える
		var localRoleColor = localRole.GetNameColor(this.Data.IsDead);

		if (ghostRole != null)
		{
			var ghostRoleColor = ghostRole.Color;
			localRoleColor = (localRoleColor / 2.0f) + (ghostRoleColor / 2.0f);
		}
		SetNameColor(localRoleColor);
	}
	private void setTag(SingleRoleBase role)
	{
		string tag = role.GetRolePlayerNameTag(role, this.Data.PlayerId);
		SetNameText($"{this.NameText.text}{tag}");
	}

	private void updateRoleInfo(SingleRoleBase role, GhostRoleBase? ghostRole)
	{
		var data = this.Data;

		// タスク情報
		var (tasksCompleted, tasksTotal) = GameSystem.GetTaskInfo(data);
		string completedStr = IsCommActive(this.Owner) ? "?" : tasksCompleted.ToString();
		string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({completedStr}/{tasksTotal})</color>" : "";

		if (HudManager.InstanceExists)
		{
			if (tabText == null)
			{
				var transform = HudManager.Instance.TaskPanel.tab.transform.Find("TabText_TMP");
				if (transform != null)
				{
					tabText = transform.GetComponent<TextMeshPro>();
				}
			}
			if (tabText != null)
			{
				tabText.SetText(
					$"{TranslationController.Instance.GetString(StringNames.Tasks)} {taskInfo}");
			}
		}

		if (!this.IsVisual)
		{
			this.Info.gameObject.SetActive(false);
			return;
		}

		this.Info.transform.localPosition =
			this.NameText.transform.localPosition + Vector3.up * InfoScale;
		string roleNames = role.GetColoredRoleName(data.IsDead);
		if (ghostRole is not null)
		{
			string ghostRoleName = ghostRole.GetColoredRoleName();
			roleNames = $"{ghostRoleName}({roleNames})";
		}
		this.Info.text = roleNames;
		this.Info.gameObject.SetActive(true);
	}

	private void roleUpdate(SingleRoleBase checkRole)
	{
		if (checkRole is IRoleUpdate updateRole)
		{
			updateRole.Update(this.Owner);
		}
	}
}
