using System;
using System.Linq;
using UnityEngine;
using AmongUs.GameOptions;
using ExtremeRoles.Compat;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public enum BurgermonOption
{
	RequiredTaskNum,
	SpeedMode,
	SpeedValue,
	SizeMode,
	SizeValue,
	KeepAfterDeath,
	AffectDeadBody,
}

public sealed class BurgermonTaskHandler
{
	private NormalPlayerTask? taskTemplate;
	private float waitTimer = 0.0f;

	public void Update(PlayerControl player)
	{
		if (!GameProgressSystem.IsTaskPhase ||
			player.IsInValid() ||
			player.Data.Tasks.Count == 0)
		{
			return;
		}

		if (this.waitTimer >= 0.0f)
		{
			this.waitTimer -= Time.deltaTime;
			return;
		}

		this.waitTimer = 1.0f;
		ReplaceAllTasksToBurger(player);
	}

	public void ClearTask(PlayerControl player)
	{
		if (this.taskTemplate != null)
		{
			UnityEngine.Object.Destroy(this.taskTemplate.gameObject);
			this.taskTemplate = null;
		}
	}

	private void ReplaceAllTasksToBurger(PlayerControl player)
	{
		if (this.taskTemplate == null)
		{
			this.taskTemplate = GetBurgerTaskTemplate(player);
			if (this.taskTemplate == null) return;
		}

		for (int i = 0; i < player.myTasks.Count; ++i)
		{
			var task = player.myTasks[i];

			if (task == null ||
				task.gameObject.TryGetComponent<ImportantTextTask>(out _) ||
				(
					task.gameObject.TryGetComponent<TaskTextOverrider>(out var overrider) &&
					overrider.Type == TaskTypes.MakeBurger
				) ||
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
				this.taskTemplate, player.transform);
			normalPlayerTask.gameObject.SetActive(true);
			normalPlayerTask.Id = task.Id;
			normalPlayerTask.Owner = player;
			normalPlayerTask.Initialize();

			player.myTasks[i] = normalPlayerTask;

			normalPlayerTask.TaskType = TaskTypes.MakeBurger;
			normalPlayerTask.StartAt = task.StartAt;

			var text = normalPlayerTask.gameObject.TryAddComponent<TaskTextOverrider>();
			text.Task = normalPlayerTask;
			text.Room = task.StartAt;
			text.Type = TaskTypes.MakeBurger;

			task.OnRemove();
			UnityEngine.Object.Destroy(task.gameObject);
		}
	}

	private static NormalPlayerTask? GetBurgerTaskTemplate(PlayerControl player)
	{
		for (byte mapId = 0; mapId <= 5; mapId++)
		{
			var ship = GameSystem.GetShipObj(mapId);
			if (ship == null) continue;

			NormalPlayerTask? target = ship.LongTasks.FirstOrDefault(x => x.TaskType == TaskTypes.MakeBurger);
			if (target == null) target = ship.CommonTasks.FirstOrDefault(x => x.TaskType == TaskTypes.MakeBurger);
			if (target == null) target = ship.ShortTasks.FirstOrDefault(x => x.TaskType == TaskTypes.MakeBurger);

			if (target != null)
			{
				NormalPlayerTask template = UnityEngine.Object.Instantiate(target, player.transform);
				template.gameObject.SetActive(false);
				return template;
			}
		}

		for (byte mapId = 0; mapId <= 5; mapId++)
		{
			var ship = GameSystem.GetShipObj(mapId);
			if (ship == null) continue;

			NormalPlayerTask? target = ship.LongTasks.FirstOrDefault();
			if (target == null) target = ship.CommonTasks.FirstOrDefault();
			if (target == null) target = ship.ShortTasks.FirstOrDefault();

			if (target != null)
			{
				NormalPlayerTask template = UnityEngine.Object.Instantiate(target, player.transform);
				template.gameObject.SetActive(false);
				return template;
			}
		}

		return null;
	}
}

public sealed class Burgermon :
	SingleRoleBase,
	IRoleAutoBuildAbility,
	IRoleUpdate,
	IRoleResetMeeting
{
	public ExtremeAbilityButton? Button { get; set; }

	private int requiredTaskNum = 1;
	private const float feedRange = 0.75f;

	private PlayerControl? tmpTargetPlayer;
	private PlayerControl? currentTargetPlayer;
	private BurgermonTaskHandler? taskHandler;

	private int lastEarnedAbilityCount = 0;

	public Burgermon() : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Burgermon,
			ColorPalette.BurgermonBrown))
	{ }

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"feedBurger",
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Burgermon),
			checkAbility: CheckAbility,
			abilityOff: CleanUp,
			forceAbilityOff: ResetTarget,
			isReduceOnActive: false);
		this.Button?.SetLabelToCrewmate();
	}

	public bool IsAbilityUse()
		=> IRoleAbility.IsCommonUse() &&
		   Player.TryGetClosestPlayerInRange(this, feedRange, out this.tmpTargetPlayer);

	public bool UseAbility()
	{
		this.currentTargetPlayer = this.tmpTargetPlayer;
		return true;
	}

	public bool CheckAbility()
		=> this.currentTargetPlayer != null &&
		   this.currentTargetPlayer.IsAlive() &&
		   Player.IsPlayerInRangeAndDrawOutLine(
			   PlayerControl.LocalPlayer,
			   this.currentTargetPlayer,
			   this,
			   feedRange);

	public void CleanUp()
	{
		if (this.currentTargetPlayer != null && PlayerControl.LocalPlayer != null)
		{
			byte targetId = this.currentTargetPlayer.PlayerId;
			BurgermonSystem.Get().FeedBurger(targetId);

			if (this.Button?.Behavior is ICountBehavior countBehavior)
			{
				countBehavior.SetAbilityCount(countBehavior.AbilityCount - 1);
			}
		}
		ResetTarget();
	}

	private void ResetTarget()
	{
		this.currentTargetPlayer = null;
		this.tmpTargetPlayer = null;
	}

	public void Update(PlayerControl rolePlayer)
	{
		this.taskHandler?.Update(rolePlayer);

		if (this.Button != null && this.Button.Behavior is ICountBehavior countBehavior)
		{
			var (tasksCompleted, _) = GameSystem.GetTaskInfo(rolePlayer.Data);
			if (this.requiredTaskNum > 0)
			{
				int earnedCount = tasksCompleted / this.requiredTaskNum;
				if (earnedCount > this.lastEarnedAbilityCount)
				{
					int diff = earnedCount - this.lastEarnedAbilityCount;
					this.lastEarnedAbilityCount = earnedCount;
					countBehavior.SetAbilityCount(countBehavior.AbilityCount + diff);
				}
			}
		}
	}

	public void ResetOnMeetingStart()
	{
		ResetTarget();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		ResetTarget();
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		this.taskHandler?.ClearTask(rolePlayer);
	}

	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		this.taskHandler?.ClearTask(rolePlayer);
	}

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
	{
		int count = BurgermonSystem.Get().GetTotalFedCount(targetPlayerId);
		if (count > 0)
		{
			return $" <color=#B16423>({count})</color>";
		}
		return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateIntOption(BurgermonOption.RequiredTaskNum, 1, 1, 10, 1);

		IRoleAbility.CreateAbilityCountOption(
			factory,
			defaultAbilityCount: 0,
			maxAbilityCount: 99,
			defaultActiveTime: 3.0f,
			minAbilityCount: 0);

		factory.CreateSelectionOption<BurgermonOption, BurgermonSpeedMode>(BurgermonOption.SpeedMode);
		factory.CreateFloatOption(BurgermonOption.SpeedValue, 0.03f, 0.01f, 0.20f, 0.01f, format: OptionUnit.Percentage);
		factory.CreateSelectionOption<BurgermonOption, BurgermonSizeMode>(BurgermonOption.SizeMode);
		factory.CreateFloatOption(BurgermonOption.SizeValue, 0.25f, 0.05f, 0.50f, 0.05f, format: OptionUnit.Percentage);
		factory.CreateBoolOption(BurgermonOption.KeepAfterDeath, true);
		factory.CreateBoolOption(BurgermonOption.AffectDeadBody, true);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.requiredTaskNum = loader.GetValue<BurgermonOption, int>(BurgermonOption.RequiredTaskNum);

		var speedMode = (BurgermonSpeedMode)loader.GetValue<BurgermonOption, int>(BurgermonOption.SpeedMode);
		float speedValue = loader.GetValue<BurgermonOption, float>(BurgermonOption.SpeedValue);
		var sizeMode = (BurgermonSizeMode)loader.GetValue<BurgermonOption, int>(BurgermonOption.SizeMode);
		float sizeValue = loader.GetValue<BurgermonOption, float>(BurgermonOption.SizeValue);
		bool keepAfterDeath = loader.GetValue<BurgermonOption, bool>(BurgermonOption.KeepAfterDeath);
		bool affectDeadBody = loader.GetValue<BurgermonOption, bool>(BurgermonOption.AffectDeadBody);

		var system = BurgermonSystem.Get();
		system.SpeedMode = speedMode;
		system.SpeedValue = speedValue;
		system.SizeMode = sizeMode;
		system.SizeValue = sizeValue;
		system.KeepAfterDeath = keepAfterDeath;
		system.AffectDeadBody = affectDeadBody;

		this.lastEarnedAbilityCount = 0;
		this.taskHandler = new BurgermonTaskHandler();
	}
}
