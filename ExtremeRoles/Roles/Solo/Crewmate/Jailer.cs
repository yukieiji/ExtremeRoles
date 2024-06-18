using System;
using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

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
			targetPlayerId,
			jailer.yardBirdOption);
		ExtremeRoleManager.SetNewRole(targetPlayerId, yardbird);
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
			jailer.lawBreakerOption);
		ExtremeRoleManager.SetNewRole(rolePlayerId, lawbreaker);
	}

	public void CreateAbility()
	{
		this.CreateAbilityCountButton(
			"AddJail",
			Loader.GetSpriteFromResources(ExtremeRoleId.Jailer));
		this.Button?.SetLabelToCrewmate();
	}

	public bool IsAbilityUse()
	{
		this.targetPlayerId = byte.MaxValue;

		PlayerControl target = Player.GetClosestPlayerInRange(
			CachedPlayerControl.LocalPlayer, this,
			this.range);
		if (target == null) { return false; }

		this.targetPlayerId = target.PlayerId;

		return IRoleAbility.IsCommonUse();
	}

	public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
	{ }

	public void ResetOnMeetingStart()
	{ }

	public bool UseAbility()
	{
		var local = CachedPlayerControl.LocalPlayer;
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

	protected override void CreateSpecificOption(IOptionInfo parentOps)
	{
		CreateBoolOption(
			Option.UseAdmin,
			false, parentOps);
		CreateBoolOption(
			Option.UseSecurity,
			true, parentOps);
		CreateBoolOption(
			Option.UseVital,
			false, parentOps);

		this.CreateAbilityCountOption(
			parentOps, 1, 5);

		CreateSelectionOption(
			Option.TargetMode,
			Enum.GetValues<TargetMode>().Select(x => x.ToString()).ToArray(),
			parentOps);
		CreateBoolOption(
			Option.CanReplaceAssassin,
			true, parentOps);

		CreateFloatOption(
			Option.Range,
			0.75f, 0.1f, 1.5f, 0.1f,
			parentOps);

		var lowBreakerOpt = CreateBoolOption(
			Option.IsMissingToDead,
			false, parentOps);

		CreateBoolOption(
			Option.IsDeadAbilityZero,
			true, lowBreakerOpt);

		CreateBoolOption(
		   Option.LawbreakerCanKill,
		   false, lowBreakerOpt,
		   invert: true,
		   enableCheckOption: parentOps);
		CreateBoolOption(
		   Option.LawbreakerUseVent,
		   true, lowBreakerOpt,
		   invert: true,
		   enableCheckOption: parentOps);
		CreateBoolOption(
		   Option.LawbreakerUseSab,
		   true, lowBreakerOpt,
		   invert: true,
		   enableCheckOption: parentOps);


		CreateIntOption(
			Option.YardbirdAddCommonTask,
			2, 0, 15, 1,
			parentOps);
		CreateIntOption(
			Option.YardbirdAddNormalTask,
			1, 0, 15, 1,
			parentOps);
		CreateIntOption(
			Option.YardbirdAddLongTask,
			1, 0, 15, 1,
			parentOps);
		CreateFloatOption(
			Option.YardbirdSpeedMod,
			0.8f, 0.1f, 1.0f, 0.1f,
			parentOps);

		CreateBoolOption(
			Option.YardbirdUseAdmin,
			false, parentOps);
		CreateBoolOption(
			Option.YardbirdUseSecurity,
			false, parentOps);
		CreateBoolOption(
			Option.YardbirdUseVital,
			false, parentOps);
		CreateBoolOption(
			Option.YardbirdUseVent,
			true, parentOps);
		CreateBoolOption(
			Option.YardbirdUseSab,
			true, parentOps);
	}

	protected override void RoleSpecificInit()
	{
		var optMng = OptionManager.Instance;

		this.CanUseAdmin = optMng.GetValue<bool>(this.GetRoleOptionId(Option.UseAdmin));
		this.CanUseSecurity = optMng.GetValue<bool>(this.GetRoleOptionId(Option.UseSecurity));
		this.CanUseVital = optMng.GetValue<bool>(this.GetRoleOptionId(Option.UseVital));

		this.isMissingToDead = optMng.GetValue<bool>(this.GetRoleOptionId(Option.IsMissingToDead));
		if (!this.isMissingToDead)
		{
			lawBreakerOption = new Lawbreaker.Option(
				optMng.GetValue<bool>(this.GetRoleOptionId(Option.LawbreakerCanKill)),
				optMng.GetValue<bool>(this.GetRoleOptionId(Option.LawbreakerUseVent)),
				optMng.GetValue<bool>(this.GetRoleOptionId(Option.LawbreakerUseSab)));
		}
		else
		{
			this.isDeadAbilityZero = optMng.GetValue<bool>(this.GetRoleOptionId(Option.IsDeadAbilityZero));
		}

		this.range = optMng.GetValue<float>(this.GetRoleOptionId(Option.Range));
		this.mode = (TargetMode)optMng.GetValue<int>(this.GetRoleOptionId(Option.TargetMode));
		this.canReplaceAssassin = optMng.GetValue<bool>(this.GetRoleOptionId(Option.CanReplaceAssassin));

		yardBirdOption = new Yardbird.Option(
			optMng.GetValue<int>(this.GetRoleOptionId(Option.YardbirdAddCommonTask)),
			optMng.GetValue<int>(this.GetRoleOptionId(Option.YardbirdAddNormalTask)),
			optMng.GetValue<int>(this.GetRoleOptionId(Option.YardbirdAddLongTask)),
			optMng.GetValue<float>(this.GetRoleOptionId(Option.YardbirdSpeedMod)),
			optMng.GetValue<bool>(this.GetRoleOptionId(Option.YardbirdUseAdmin)),
			optMng.GetValue<bool>(this.GetRoleOptionId(Option.YardbirdUseSecurity)),
			optMng.GetValue<bool>(this.GetRoleOptionId(Option.YardbirdUseVital)),
			optMng.GetValue<bool>(this.GetRoleOptionId(Option.YardbirdUseVent)),
			optMng.GetValue<bool>(this.GetRoleOptionId(Option.YardbirdUseSab)));
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

	private readonly List<int> allTask;

	public Yardbird(
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
		this.MoveSpeed = option.SpeedMod;
		var addTasks = new List<int>(option.AddCommonTask + option.AddNormalTask + option.AddLongTask);

		if (targetPlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
		{
			for (int i = 0; i < option.AddCommonTask; ++i)
			{
				addTasks.Add(GameSystem.GetRandomCommonTaskId());
			}
			for (int i = 0; i < option.AddNormalTask; ++i)
			{
				addTasks.Add(GameSystem.GetRandomNormalTaskId());
			}
			for (int i = 0; i < option.AddLongTask; ++i)
			{
				addTasks.Add(GameSystem.GetRandomNormalTaskId());
			}
			this.allTask = addTasks.OrderBy(x => RandomGenerator.Instance.Next()).ToList();
		}
		else
		{
			this.allTask = new List<int>();
		}
	}

	protected override void CreateSpecificOption(IOptionInfo parentOps)
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
		bool Vent,
		bool Sab);

	public Lawbreaker(
		Option option) : base(
		ExtremeRoleId.Lawbreaker,
		ExtremeRoleType.Neutral,
		ExtremeRoleId.Lawbreaker.ToString(),
		ColorPalette.GamblerYellowGold,
		option.Kill, false, option.Vent, option.Sab)
	{ }

	protected override void CreateSpecificOption(IOptionInfo parentOps)
	{
		throw new Exception("Don't call this class method!!");
	}

	protected override void RoleSpecificInit()
	{
		throw new Exception("Don't call this class method!!");
	}

	public void ModifiedWinPlayer(
		GameData.PlayerInfo rolePlayerInfo,
		GameOverReason reason,
		ref ExtremeGameResult.WinnerTempData winner)
	{
		if (reason is
			GameOverReason.HumansByTask or
			GameOverReason.HumansByVote)
		{
			return;
		}
		winner.AddWithPlus(rolePlayerInfo);
	}
}