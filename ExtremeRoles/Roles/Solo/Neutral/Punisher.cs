using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Performance.Il2Cpp;
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
	private int taskKillCoolReduce;
	private int nonImpostorKillCoolIncrease;

	private HashSet<uint> curTaskListCompleted = [];
	private int totalTaskComplete;
	private byte? winRolePlayerId;

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

		this.winRolePlayerId = null;

		if (targetRole.IsImpostor())
		{
			// 勝利予定者に組み込み
			this.winRolePlayerId = rolePlayer.PlayerId;
		}
		else
		{
			this.KillCoolTime += this.nonImpostorKillCoolIncrease;
		}
		return true;
	}

	public override bool IsSameTeam(SingleRoleBase targetRole) =>
		this.IsNeutralSameTeam(targetRole);

	public void Update(PlayerControl rolePlayer)
	{
		if (rolePlayer.IsInValid() || rolePlayer.Data.Tasks == null)
		{
			return;
		}

		int currentCompletedCount = 0;
		List<uint> newlyCompletedTaskIds = new();

		var allTask = rolePlayer.Data.Tasks;

		foreach (var task in allTask.GetFastEnumerator())
		{
			if (!task.Complete)
			{
				continue;
			}

			currentCompletedCount++;
			if (!this.curTaskListCompleted.Contains(task.Id))
			{
				this.totalTaskComplete++;
				newlyCompletedTaskIds.Add(task.Id);
			}
		}

		if (this.totalTaskComplete >= this.requiredTaskNum)
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
				this.curTaskListCompleted.Add(id);
			}

			if (this.CanKill)
			{
				float maxCool = Mathf.Max(0.01f, this.KillCoolTime);
				rolePlayer.killTimer = Mathf.Clamp(
					rolePlayer.killTimer - (this.taskKillCoolReduce * newlyCompletedTaskIds.Count),
					0.01f, maxCool);
			}
		}

		// 無限タスク生成: 全てのタスクが完了したらランダムな同じタイプのタスクを補充する
		if (currentCompletedCount == allTask.Count && allTask.Count > 0)
		{
			for (int i = 0; i < allTask.Count; ++i)
			{
				var task = allTask[i];
				int taskId = task.TypeId;
				int newTaskId = 0;
				if (ShipStatus.Instance.CommonTasks.Any(
					(NormalPlayerTask t) => t.Index == taskId))
				{
					newTaskId = GameSystem.GetRandomCommonTaskId();
				}
				else if (ShipStatus.Instance.LongTasks.Any(
					(NormalPlayerTask t) => t.Index == taskId))
				{
					newTaskId = GameSystem.GetRandomLongTask();
				}
				else if (ShipStatus.Instance.ShortTasks.Any(
					(NormalPlayerTask t) => t.Index == taskId))
				{
					newTaskId = GameSystem.GetRandomShortTaskId();
				}
				else
				{
					continue;
				}
				GameSystem.RpcReplaceNewTask(rolePlayer.PlayerId, i, newTaskId);
			}
			this.curTaskListCompleted.Clear();
		}
	}

	public void OnStartKill()
	{ }

	public void OnEndKill()
	{
		// 勝利予定者で合った場合にここに来る => sourceがwinRolePlayerIdなので全体RPCを飛ばして勝利にする
		if (this.winRolePlayerId.HasValue)
		{
			ExtremeRolesPlugin.ShipState.RpcRoleIsWin(this.winRolePlayerId.Value);
		}
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateBoolOption(Option.CanUseVent, true);

		factory.CreateIntOption(
			Option.RequiredTaskNumToKill,
			2, 0, 10, 1);

		factory.CreateIntOption(
			Option.TaskCompletionKillCoolReduce,
			5, 0, 120, 1,
			format: OptionUnit.Second);

		factory.CreateIntOption(
			Option.NonImpostorKillCoolIncrease,
			10, 0, 120, 1,
			format: OptionUnit.Second);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;

		this.UseVent = loader.GetValue<Option, bool>(Option.CanUseVent);

		this.requiredTaskNum = loader.GetValue<Option, int>(
			Option.RequiredTaskNumToKill);
		this.taskKillCoolReduce = loader.GetValue<Option, int>(
			Option.TaskCompletionKillCoolReduce);
		this.nonImpostorKillCoolIncrease = loader.GetValue<Option, int>(
			Option.NonImpostorKillCoolIncrease);

		if (!this.HasOtherKillCool)
		{
			this.HasOtherKillCool = true;
			this.KillCoolTime = Player.DefaultKillCoolTime;
		}

		this.totalTaskComplete = 0;
		this.winRolePlayerId = null;
		this.curTaskListCompleted.Clear();
		this.IsWin = false;
		this.CanKill = this.requiredTaskNum == 0;
	}
}
