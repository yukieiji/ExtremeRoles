using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.Compat;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class SnatcherAbilityHandler
{
	private readonly DoveCommonAbilityHandler handler;
	private NormalPlayerTask? task;

	private float waitTimer = 0.0f;

	public SnatcherAbilityHandler(LiberalDefaultOptionLoader option)
	{
		this.handler = new DoveCommonAbilityHandler(option);
	}

	public void Update(PlayerControl player)
	{
		this.handler.Update(player);
		if (!GameProgressSystem.IsTaskPhase ||
			player.IsInValid() ||
			player.Data.Tasks.Count == 0)
		{
			return;
		}

		if (this.waitTimer >= 0.0f)
		{
			this.waitTimer -= UnityEngine.Time.deltaTime;
			return;
		}

		// 1秒ごとに見る
		this.waitTimer = 1.0f;
		replaceAllTaskToUnlock(player);
	}

	public void ClearTask(PlayerControl player)
		=> this.handler.ClearTask(player);

	private void replaceAllTaskToUnlock(PlayerControl player)
	{
		if (this.task == null)
		{
			this.task = getTask(player);
		}

		for (int i = 0; i < player.myTasks.Count; ++i)
		{
			var task = player.myTasks[i];

			if (task == null ||
				task.gameObject.TryGetComponent<ImportantTextTask>(out var _) ||
				(
					task.gameObject.TryGetComponent<TaskTextOverrider>(out var overrider) && 
					overrider.Type == TaskTypes.UnlockSafe
				)||
				PlayerTask.TaskIsEmergency(task))
			{
				continue;
			}

			if (CompatModManager.Instance.TryGetModMap(out var modMap) &&
				modMap.IsCustomSabotageTask(task.TaskType))
			{
				continue;
			}

			NormalPlayerTask normalPlayerTask = UnityEngine.Object.Instantiate(
				this.task, player.transform);
			normalPlayerTask.gameObject.SetActive(true);
			normalPlayerTask.Id = task.Id;
			normalPlayerTask.Owner = player;
			normalPlayerTask.Initialize();

			player.myTasks[i] = normalPlayerTask;

			normalPlayerTask.TaskType = task.TaskType;
			normalPlayerTask.StartAt = task.StartAt;

			var text = normalPlayerTask.gameObject.TryAddComponent<TaskTextOverrider>();
			text.Task = normalPlayerTask;
			text.Room = task.StartAt;
			text.Type = TaskTypes.UnlockSafe;

			task.OnRemove();
			UnityEngine.Object.Destroy(task.gameObject);
		}
	}

	private static NormalPlayerTask getTask(PlayerControl player)
	{
		var airShip = GameSystem.GetShipObj(4);

		var target = airShip.LongTasks.FirstOrDefault(
			x => x.TaskType == TaskTypes.UnlockSafe);
		if (target == null)
		{
			target = airShip.CommonTasks.FirstOrDefault(
				x => x.TaskType == TaskTypes.UnlockSafe);
		}
		if (target == null)
		{
			target = airShip.ShortTasks.FirstOrDefault(
				x => x.TaskType == TaskTypes.UnlockSafe);
		}
		if (target == null)
		{
			throw new System.InvalidOperationException();
		}
		NormalPlayerTask normalPlayerTask = UnityEngine.Object.Instantiate(target, player.transform);
		normalPlayerTask.gameObject.SetActive(false);
		return normalPlayerTask;
	}
}


public sealed class Snatcher : SingleRoleBase, IRoleUpdate
{
	private DoveCommonAbilityHandler? handler;

	public Snatcher() : base(
		RoleArgs.BuildLiberalDove(ExtremeRoleId.SpecialDove))
	{
	}

	public override string GetRoleTag()
		=> "Sn";

	public void Update(PlayerControl rolePlayer)
	{
		this.handler?.Update(rolePlayer);
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		this.handler?.ClearTask(rolePlayer);
	}

	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		this.handler?.ClearTask(rolePlayer);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
	}

	protected override void RoleSpecificInit()
	{
		var liberalOption = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<LiberalDefaultOptionLoader>();
		LiberalSettingOverrider.OverrideDefault(this, liberalOption);
		this.handler = new DoveCommonAbilityHandler(liberalOption);
	}
}
