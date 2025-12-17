using AmongUs.GameOptions;

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
	}

	private NetworkedPlayerInfo? cachePlayer;

	public void Update(PlayerControl player)
	{
		if (!GameProgressSystem.IsTaskPhase)
		{
			return;
		}

		if (player.Data.IsDead || player.Data.Disconnected)
		{
			return;
		}

		if (cachePlayer == null)
		{
			cachePlayer = GameData.Instance.GetPlayerById(player.PlayerId);
		}
		if (cachePlayer == null || cachePlayer.Tasks.Count == 0)
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
			byte playerId = cachePlayer.PlayerId;
			GameSystem.RpcReplaceNewTask(playerId, i, taskIndex);
			LiberalMoneyBankSystem.RpcUpdateSystem(playerId, LiberalMoneyHistory.Reason.AddOnTask, taskDelta, boostDelta);
			break;
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
