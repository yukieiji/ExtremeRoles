using System;
using System.Collections.Generic;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Punisher : SingleRoleBase, IRoleUpdate, IRolePerformKillHook, ITryKillTo
{
	public enum Option
	{
		CanUseVent,
		RequiredTaskNumToKill,
		TaskCompletionKillCoolReduce,
		NonImpostorKillCoolIncrease,
	}

	private int requiredTaskNum;
	private float taskKillCoolReduce;
	private float nonImpostorKillCoolIncrease;

	private HashSet<uint> completedTasks = new();

	public Punisher() : base(
		RoleArgs.BuildNeutral(
			ExtremeRoleId.Punisher,
			ColorPalette.PunisherDarkGold,
			RoleProp.CanKill | RoleProp.UseVent | RoleProp.HasTask |
			RolePropPresets.OptionalDefault))
	{ }

	public bool TryRolePlayerKillTo(
		PlayerControl rolePlayer, PlayerControl targetPlayer)
	{
		if (!ExtremeRoleManager.TryGetRole(targetPlayer.PlayerId, out var targetRole))
		{
			return false;
		}

		if (targetRole.IsImpostor())
		{
			this.IsWin = true;
			return true;
		}
		else
		{
			this.KillCoolTime += this.nonImpostorKillCoolIncrease;
			return true;
		}
	}

	public override bool IsSameTeam(SingleRoleBase targetRole) =>
		this.IsNeutralSameTeam(targetRole);

	public void Update(PlayerControl rolePlayer)
	{
		if (rolePlayer == null || rolePlayer.Data == null || rolePlayer.Data.Tasks == null)
		{
			return;
		}

		int currentCompletedCount = 0;
		List<uint> newlyCompletedTaskIds = new();

		for (int i = 0; i < rolePlayer.Data.Tasks.Count; ++i)
		{
			var task = rolePlayer.Data.Tasks[i];
			if (!task.Complete)
			{
				continue;
			}

			currentCompletedCount++;
			if (!this.completedTasks.Contains(task.Id))
			{
				newlyCompletedTaskIds.Add(task.Id);
			}
		}

		if (currentCompletedCount >= this.requiredTaskNum)
		{
			this.CanKill = true;
		}
		else
		{
			this.CanKill = false;
		}

		if (newlyCompletedTaskIds.Count > 0)
		{
			foreach (uint id in newlyCompletedTaskIds)
			{
				this.completedTasks.Add(id);
			}

			if (this.CanKill)
			{
				float maxCool = Mathf.Max(0.01f, this.KillCoolTime);
				rolePlayer.killTimer = Mathf.Clamp(
					rolePlayer.killTimer - (this.taskKillCoolReduce * newlyCompletedTaskIds.Count),
					0.01f, maxCool);
			}
		}

		// 無限タスク生成: 全てのタスクが完了したらランダムなショートタスクを補充する
		if (currentCompletedCount == rolePlayer.Data.Tasks.Count && rolePlayer.Data.Tasks.Count > 0)
		{
			int newTaskId = GameSystem.GetRandomShortTaskId();
			uint nextId = (uint)rolePlayer.Data.Tasks.Count;
			if (GameSystem.SetPlayerNewTask(rolePlayer, (byte)newTaskId, nextId))
			{
				rolePlayer.Data.Tasks[rolePlayer.Data.Tasks.Count - 1] = new((byte)newTaskId, nextId);
				rolePlayer.Data.MarkDirty();
			}
		}
	}

	public void OnStartKill()
	{ }

	public void OnEndKill()
	{
		if (this.IsWin && PlayerControl.LocalPlayer != null)
		{
			ExtremeRolesPlugin.ShipState.RpcRoleIsWin(PlayerControl.LocalPlayer.PlayerId);
		}
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateBoolOption(Option.CanUseVent, true);

		factory.CreateIntOption(
			Option.RequiredTaskNumToKill,
			2, 0, 10, 1);

		factory.CreateFloatOption(
			Option.TaskCompletionKillCoolReduce,
			5.0f, 0.0f, 120.0f, 1.0f,
			format: OptionUnit.Second);

		factory.CreateFloatOption(
			Option.NonImpostorKillCoolIncrease,
			10.0f, 0.0f, 120.0f, 1.0f,
			format: OptionUnit.Second);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;

		this.UseVent = loader.GetValue<Option, bool>(Option.CanUseVent);

		this.requiredTaskNum = loader.GetValue<Option, int>(
			Option.RequiredTaskNumToKill);
		this.taskKillCoolReduce = loader.GetValue<Option, float>(
			Option.TaskCompletionKillCoolReduce);
		this.nonImpostorKillCoolIncrease = loader.GetValue<Option, float>(
			Option.NonImpostorKillCoolIncrease);

		this.completedTasks.Clear();
		this.IsWin = false;
		this.CanKill = this.requiredTaskNum == 0;
	}
}
