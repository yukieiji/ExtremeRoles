using System;
using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Jailer : SingleRoleBase, IRoleAutoBuildAbility
{
	public enum Option
	{
		UseAdmin,
		UseSecurity,
		UseVital,

		Range,
		TargetMode,
		CanReplaceAssassin,

		IsMissingToDead,
		IsDeadAbilityZero,

		LawbreakerCanKill,
		LawbreakerUseVent,
		LawbreakerUseSab,

		YardbirdAddCommonTask,
		YardbirdAddNormalTask,
		YardbirdAddLongTask,
		YardbirdSpeedMod,
		YardbirdUseAdmin,
		YardbirdUseSecurity,
		YardbirdUseVital,
		YardbirdUseVent,
		YardbirdUseSab,
	}

	public ExtremeAbilityButton? Button { get; set; }

	private bool isMissingToDead = false;
	private bool isDeadAbilityZero = true;

	private TargetMode mode;
	private bool canReplaceAssassin = false;

	private float range;
	private byte targetPlayerId = byte.MaxValue;

	private Yardbird.Option? yardBirdOption;
	private Lawbreaker.Option? lawBreakerOption;

	public enum TargetMode
	{
		Both,
		ImpostorOnly,
		NeutralOnly,
	}

	public Jailer() : base(
		ExtremeRoleId.Jailer,
		ExtremeRoleType.Crewmate,
		ExtremeRoleId.Jailer.ToString(),
		ColorPalette.GamblerYellowGold,
		false, true, false, false, false)
	{ }

	public static void NotCrewmateToYardbird(byte rolePlayerId, byte targetPlayerId)
	{
		PlayerControl targetPlayer = Player.GetPlayerControlById(targetPlayerId);
		if (targetPlayer == null ||
			!ExtremeRoleManager.TryGetSafeCastedRole<Jailer>(rolePlayerId, out var jailer) ||
			jailer.yardBirdOption == null)
		{
			return;
		}
		IRoleSpecialReset.ResetRole(targetPlayerId);
		var yardbird = new Yardbird(
			jailer.Loader,
			targetPlayerId,
			jailer.yardBirdOption);
		ExtremeRoleManager.SetNewRole(targetPlayerId, yardbird);
		IRoleSpecialReset.ResetLover(targetPlayerId);
	}

	public static void ToLawbreaker(byte rolePlayerId)
	{
		PlayerControl targetPlayer = Player.GetPlayerControlById(rolePlayerId);
		if (targetPlayer == null ||
			!ExtremeRoleManager.TryGetSafeCastedRole<Jailer>(rolePlayerId, out var jailer) ||
			jailer.lawBreakerOption == null)
		{
			return;
		}
		IRoleSpecialReset.ResetRole(rolePlayerId);
		var lawbreaker = new Lawbreaker(
			jailer.Loader,
			jailer.lawBreakerOption);
		ExtremeRoleManager.SetNewRole(rolePlayerId, lawbreaker);
		IRoleSpecialReset.ResetLover(rolePlayerId);
	}

	public void CreateAbility()
	{
		this.CreateAbilityCountButton(
			"AddJail",
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Jailer));
		this.Button?.SetLabelToCrewmate();
	}

	public bool IsAbilityUse()
	{
		this.targetPlayerId = byte.MaxValue;

		PlayerControl target = Player.GetClosestPlayerInRange(
			PlayerControl.LocalPlayer, this,
			this.range);
		if (target == null) { return false; }

		this.targetPlayerId = target.PlayerId;

		return IRoleAbility.IsCommonUse();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{ }

	public void ResetOnMeetingStart()
	{ }

	public bool UseAbility()
	{
		var local = PlayerControl.LocalPlayer;
		if (local == null ||
			this.Button?.Behavior is not CountBehavior count ||
			!ExtremeRoleManager.TryGetRole(this.targetPlayerId, out var role))
		{
			return false;
		}

		byte rolePlayerId = local.PlayerId;

		bool isSuccess = this.mode switch
		{
			TargetMode.Both => !role.IsCrewmate() && (this.canReplaceAssassin || role.Id != ExtremeRoleId.Assassin),
			TargetMode.ImpostorOnly => role.IsImpostor() && (this.canReplaceAssassin || role.Id != ExtremeRoleId.Assassin),
			TargetMode.NeutralOnly => role.IsNeutral(),
			_ => false,
		};

		if (isSuccess)
		{
			// 対象をヤードバード化
			using (var caller = RPCOperator.CreateCaller(
				RPCOperator.Command.ReplaceRole))
			{
				caller.WriteByte(rolePlayerId);
				caller.WriteByte(this.targetPlayerId);
				caller.WriteByte(
					(byte)ExtremeRoleManager.ReplaceOperation.ForceReplaceToYardbird);
			}
			NotCrewmateToYardbird(rolePlayerId, this.targetPlayerId);

			if (this.isDeadAbilityZero && count.AbilityCount <= 1)
			{
				selfKill(rolePlayerId);
			}
		}
		else
		{
			if (this.isMissingToDead)
			{
				selfKill(rolePlayerId);
			}
			else
			{
				// 自分自身をローブレーカー化
				using (var caller = RPCOperator.CreateCaller(
					RPCOperator.Command.ReplaceRole))
				{
					caller.WriteByte(rolePlayerId);
					caller.WriteByte(rolePlayerId);
					caller.WriteByte(
						(byte)ExtremeRoleManager.ReplaceOperation.BecomeLawbreaker);
				}
				ToLawbreaker(rolePlayerId);
			}

		}

		return true;
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateBoolOption(
			Option.UseAdmin, false);
		factory.CreateBoolOption(
			Option.UseSecurity, true);
		factory.CreateBoolOption(
			Option.UseVital, false);

		IRoleAbility.CreateAbilityCountOption(
			factory, 1, 5);

		factory.CreateSelectionOption<Option, TargetMode>(
			Option.TargetMode);
		factory.CreateBoolOption(
			Option.CanReplaceAssassin,
			true);

		factory.CreateFloatOption(
			Option.Range,
			0.75f, 0.1f, 1.5f, 0.1f);

		var lowBreakerOpt = factory.CreateBoolOption(
			Option.IsMissingToDead, false);

		factory.CreateBoolOption(
			Option.IsDeadAbilityZero,
			true, lowBreakerOpt);

		var lowBreakerKillOpt = factory.CreateBoolOption(
		   Option.LawbreakerCanKill,
		   false, lowBreakerOpt,
		   invert: true);

		var killCoolOption = factory.CreateBoolOption(
			KillerCommonOption.HasOtherKillCool,
			false, lowBreakerKillOpt,
			invert: true);
		factory.CreateFloatOption(
			KillerCommonOption.KillCoolDown,
			30f, 1.0f, 120f, 0.5f,
			killCoolOption, format: OptionUnit.Second,
			invert: true);

		var killRangeOption = factory.CreateBoolOption(
			KillerCommonOption.HasOtherKillRange,
			false, lowBreakerKillOpt,
			invert: true);
		factory.CreateSelectionOption(
			KillerCommonOption.KillRange,
			OptionCreator.Range,
			killRangeOption,
			invert: true);

		factory.CreateBoolOption(
		   Option.LawbreakerUseVent,
		   true, lowBreakerOpt,
		   invert: true);
		factory.CreateBoolOption(
		   Option.LawbreakerUseSab,
		   true, lowBreakerOpt,
		   invert: true);


		factory.CreateIntOption(
			Option.YardbirdAddCommonTask,
			2, 0, 15, 1);
		factory.CreateIntOption(
			Option.YardbirdAddNormalTask,
			1, 0, 15, 1);
		factory.CreateIntOption(
			Option.YardbirdAddLongTask,
			1, 0, 15, 1);
		factory.CreateFloatOption(
			Option.YardbirdSpeedMod,
			0.8f, 0.1f, 1.0f, 0.1f);

		factory.CreateBoolOption(
			Option.YardbirdUseAdmin, false);
		factory.CreateBoolOption(
			Option.YardbirdUseSecurity, false);
		factory.CreateBoolOption(
			Option.YardbirdUseVital, false);
		factory.CreateBoolOption(
			Option.YardbirdUseVent, true);
		factory.CreateBoolOption(
			Option.YardbirdUseSab, true);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;

		this.CanUseAdmin = loader.GetValue<Option, bool>(Option.UseAdmin);
		this.CanUseSecurity = loader.GetValue<Option, bool>(Option.UseSecurity);
		this.CanUseVital = loader.GetValue<Option, bool>(Option.UseVital);

		this.isMissingToDead = loader.GetValue<Option, bool>(Option.IsMissingToDead);
		if (!this.isMissingToDead)
		{
			lawBreakerOption = new Lawbreaker.Option(
				loader.GetValue<Option, bool>(Option.LawbreakerCanKill),
				loader.GetValue<KillerCommonOption, bool>(KillerCommonOption.HasOtherKillCool),
				loader.GetValue<KillerCommonOption, float>(KillerCommonOption.KillCoolDown),
				loader.GetValue<KillerCommonOption, bool>(KillerCommonOption.HasOtherKillRange),
				loader.GetValue<KillerCommonOption, int>(KillerCommonOption.KillRange),
				loader.GetValue<Option, bool>(Option.LawbreakerUseVent),
				loader.GetValue<Option, bool>(Option.LawbreakerUseSab));
		}
		else
		{
			this.isDeadAbilityZero = loader.GetValue<Option, bool>(Option.IsDeadAbilityZero);
		}

		this.range = loader.GetValue<Option, float>(Option.Range);
		this.mode = (TargetMode)loader.GetValue<Option, int>(Option.TargetMode);
		this.canReplaceAssassin = loader.GetValue<Option, bool>(Option.CanReplaceAssassin);

		yardBirdOption = new Yardbird.Option(
			loader.GetValue<Option, int  >(Option.YardbirdAddCommonTask),
			loader.GetValue<Option, int  >(Option.YardbirdAddNormalTask),
			loader.GetValue<Option, int  >(Option.YardbirdAddLongTask),
			loader.GetValue<Option, float>(Option.YardbirdSpeedMod),
			loader.GetValue<Option, bool >(Option.YardbirdUseAdmin),
			loader.GetValue<Option, bool >(Option.YardbirdUseSecurity),
			loader.GetValue<Option, bool >(Option.YardbirdUseVital),
			loader.GetValue<Option, bool >(Option.YardbirdUseVent),
			loader.GetValue<Option, bool >(Option.YardbirdUseSab));
	}
	private static void selfKill(byte rolePlayerId)
	{
		Player.RpcUncheckMurderPlayer(
					rolePlayerId, rolePlayerId, byte.MaxValue);
		ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
			rolePlayerId,
			Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus.MissShot);
	}
}

public sealed class Yardbird : SingleRoleBase, IRoleUpdate
{
	public sealed record Option(
		int AddCommonTask,
		int AddNormalTask,
		int AddLongTask,
		float SpeedMod,
		bool Admin,
		bool Security,
		bool Vital,
		bool Vent,
		bool Sab);

	public override IOptionLoader Loader { get; }

	private readonly List<int> allTask;

	public Yardbird(
		in IOptionLoader loader,
		byte targetPlayerId,
		Option option) : base(
		ExtremeRoleId.Yardbird,
		ExtremeRoleType.Crewmate,
		ExtremeRoleId.Yardbird.ToString(),
		ColorPalette.GamblerYellowGold,
		false, true,
		option.Vent,
		option.Sab,
		false, true,
		option.Admin,
		option.Security,
		option.Vital)
	{
		this.Loader = loader;
		this.MoveSpeed = option.SpeedMod;
		var addTasks = new List<int>(option.AddCommonTask + option.AddNormalTask + option.AddLongTask);

		if (targetPlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			for (int i = 0; i < option.AddCommonTask; ++i)
			{
				addTasks.Add(GameSystem.GetRandomCommonTaskId());
			}
			for (int i = 0; i < option.AddNormalTask; ++i)
			{
				addTasks.Add(GameSystem.GetRandomShortTaskId());
			}
			for (int i = 0; i < option.AddLongTask; ++i)
			{
				addTasks.Add(GameSystem.GetRandomLongTask());
			}
			this.allTask = addTasks.OrderBy(x => RandomGenerator.Instance.Next()).ToList();
		}
		else
		{
			this.allTask = new List<int>();
		}
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		throw new Exception("Don't call this class method!!");
	}

	protected override void RoleSpecificInit()
	{
		throw new Exception("Don't call this class method!!");
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (CachedShipStatus.Instance == null ||
			GameData.Instance == null ||
			!CachedShipStatus.Instance.enabled ||
			this.allTask.Count == 0) { return; }

		var playerInfo = GameData.Instance.GetPlayerById(
			rolePlayer.PlayerId);

		for (int i = 0; i < playerInfo.Tasks.Count; ++i)
		{
			if (playerInfo.Tasks[i].Complete)
			{
				int taskId = this.allTask[0];
				this.allTask.RemoveAt(0);

				GameSystem.RpcReplaceNewTask(
					rolePlayer.PlayerId, i, taskId);
				break;
			}
		}
	}
}

public sealed class Lawbreaker : SingleRoleBase, IRoleWinPlayerModifier
{
	public sealed record Option(
		bool Kill,
		bool HasOtherKillCool,
		float KillCool,
		bool HasOtherKillRange,
		int KillRange,
		bool Vent,
		bool Sab);

	public override IOptionLoader Loader { get; }

	public Lawbreaker(
		IOptionLoader loader,
		Option option) : base(
		ExtremeRoleId.Lawbreaker,
		ExtremeRoleType.Neutral,
		ExtremeRoleId.Lawbreaker.ToString(),
		ColorPalette.GamblerYellowGold,
		option.Kill, false, option.Vent, option.Sab)
	{

		this.Loader = loader;

		if (this.CanKill)
		{
			var baseOption = GameOptionsManager.Instance.CurrentGameOptions;

			this.HasOtherKillCool = option.HasOtherKillCool;
			this.KillCoolTime = this.HasOtherKillCool ? option.KillCool : baseOption.GetFloat(FloatOptionNames.KillCooldown);

			this.HasOtherKillRange = option.HasOtherKillRange;
			this.KillRange = this.HasOtherKillRange ? option.KillRange : baseOption.GetInt(Int32OptionNames.KillDistance);
		}
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		throw new Exception("Don't call this class method!!");
	}

	protected override void RoleSpecificInit()
	{
		throw new Exception("Don't call this class method!!");
	}

	public void ModifiedWinPlayer(
		NetworkedPlayerInfo rolePlayerInfo,
		GameOverReason reason,
		in ExtremeGameResult.WinnerTempData winner)
	{
		if (reason is
				GameOverReason.HumansByTask or
				GameOverReason.HumansByVote or
				GameOverReason.HumansDisconnect)
		{
			return;
		}
		winner.AddWithPlus(rolePlayerInfo);
	}
}