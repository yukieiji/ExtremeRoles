using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class DoveCommonAbilityHandler
{
	private readonly float taskDelta;
	private readonly float boostDelta;

	private readonly int shortTask;
	private readonly int normalTask;
	private readonly int allTaskNum;

	private HashSet<uint> oldTaskComplete = [];

	public DoveCommonAbilityHandler(LiberalDefaultOptionLoader option) : this(
		option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.TaskCompletedMoney),
		0.0f)
	{

	}

	public DoveCommonAbilityHandler(float taskDelta, float boostDelta)
	{
		var option = GameOptionsManager.Instance.CurrentGameOptions;
		this.shortTask = option.GetInt(Int32OptionNames.NumShortTasks);
		this.normalTask = option.GetInt(Int32OptionNames.NumCommonTasks);

		this.allTaskNum = this.shortTask + this.normalTask + option.GetInt(Int32OptionNames.NumLongTasks);

		this.taskDelta = taskDelta;
		this.boostDelta = boostDelta;
		this.oldTaskComplete.Clear();
	}

	private NetworkedPlayerInfo? cachePlayer;

	public void Update(PlayerControl player)
	{
		if (!GameProgressSystem.IsTaskPhase)
		{
			return;
		}

		if (cachePlayer == null)
		{
			cachePlayer = GameData.Instance.GetPlayerById(player.PlayerId);
		}
		if (cachePlayer.IsInValid() || cachePlayer.Tasks.Count == 0)
		{
			return;
		}

		List<uint> curTaskComplete = [];

		for (int i = 0; i < cachePlayer.Tasks.Count; ++i)
		{
			var task = cachePlayer.Tasks[i];
			if (!task.Complete)
			{
				continue;
			}
			curTaskComplete.Add(task.Id);
		}

		if (curTaskComplete.Count == 0 || curTaskComplete.Count == this.oldTaskComplete.Count)
		{
			return;
		}

		byte playerId = cachePlayer.PlayerId;
		
		foreach (uint id in curTaskComplete)
		{
			if (this.oldTaskComplete.Contains(id))
			{
				continue;
			}
			LiberalMoneyBankSystem.RpcUpdateSystem(playerId, LiberalMoneyHistory.Reason.AddOnTask, taskDelta, boostDelta);
		}

		this.oldTaskComplete = curTaskComplete.ToHashSet();

		// 全てのタスクが完了している場合、タスクをランダムに置き換える
		if (this.oldTaskComplete.Count != cachePlayer.Tasks.Count)
		{
			return;
		}

		for (int i = 0; i < cachePlayer.Tasks.Count; ++i)
		{
			var task = cachePlayer.Tasks[i];
			if (!task.Complete)
			{
				continue;
			}

			int taskTarget = RandomGenerator.Instance.Next(0, this.allTaskNum);

			int taskIndex;
			if (taskTarget < this.shortTask)
			{
				taskIndex = GameSystem.GetRandomShortTaskId();
			}
			else if (taskTarget < this.normalTask)
			{
				taskIndex = GameSystem.GetRandomCommonTaskId();
			}
			else
			{
				taskIndex = GameSystem.GetRandomLongTask();
			}
			GameSystem.RpcReplaceNewTask(playerId, i, taskIndex);
		}
	}

	public void ClearTask(PlayerControl player)
	{
		if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			player.ClearTasks();
		}
	}
}

public static class LiberalSettingOverrider
{
	public static void OverrideDefault(SingleRoleBase role, LiberalDefaultOptionLoader option)
	{
		role.IsApplyEnvironmentVision = false;
		role.UseVent = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.UseVent);
		role.Vision = option.GetValue<LiberalGlobalSetting, float>(LiberalGlobalSetting.LiberalVison);
	}
}
