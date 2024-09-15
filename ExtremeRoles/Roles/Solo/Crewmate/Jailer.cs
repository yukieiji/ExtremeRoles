using System;
using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;
using UnityEngine;

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
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.Solo.Neutral;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Jailer : SingleRoleBase, IRoleAutoBuildAbility, IRoleAwake<RoleTypes>
{
	public enum Option
	{
		AwakeTaskGage,
		AwakeDeadPlayerNum,

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

	public bool IsAwake
	{
		get
		{
			return GameSystem.IsLobby || this.awakeRole;
		}
	}

	public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

	private bool isMissingToDead = false;
	private bool isDeadAbilityZero = true;

	private TargetMode mode;
	private bool canReplaceAssassin = false;

	private float range;
	private byte targetPlayerId = byte.MaxValue;
	private bool awakeRole = false;

	private float awakeTaskGage;
	private float awakeDeadPlayerNum;
	private bool awakeHasOtherVision;

	private Yardbird.Option? yardBirdOption;
	private Lawbreaker.Option? lawBreakerOption;

	private static string impShortStr => Design.ColoedString(
		Palette.ImpostorRed,
		Tr.GetString("impostorShotCall"));
	private static string neutShortStr => Design.ColoedString(
		ColorPalette.NeutralColor,
		Tr.GetString("neutralShotCall"));
	private string andShortStr => Design.ColoedString(
		this.NameColor,
		Tr.GetString("andFirst"));

	public enum TargetMode
	{
		BothImpostorAndNautral,
		Impostor,
		Neutral,
	}

	public Jailer() : base(
		ExtremeRoleId.Jailer,
		ExtremeRoleType.Crewmate,
		ExtremeRoleId.Jailer.ToString(),
		ColorPalette.JailerSapin,
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
			Tr.GetString("AddJail"),
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

	public override string GetColoredRoleName(bool isTruthColor = false)
	{
		if (isTruthColor || IsAwake)
		{
			return base.GetColoredRoleName();
		}
		else
		{
			return Design.ColoedString(
				Palette.White,
				Tr.GetString(RoleTypes.Crewmate.ToString()));
		}
	}
	public override string GetFullDescription()
	{
		if (IsAwake)
		{
			return Tr.GetString(
				$"{this.Id}FullDescription");
		}
		else
		{
			return Tr.GetString(
				$"{RoleTypes.Crewmate}FullDescription");
		}
	}

	public override string GetImportantText(bool isContainFakeTask = true)
	{
		if (IsAwake)
		{
			string shortText = this.mode switch
			{
				TargetMode.BothImpostorAndNautral => $"{impShortStr}{andShortStr}{neutShortStr}",
				TargetMode.Impostor => impShortStr,
				TargetMode.Neutral => neutShortStr,
				_ => "",
			};

			return string.Format("{0}: {1}{2}",
				Design.ColoedString(
					this.NameColor,
					Tr.GetString(this.RoleName)),
				shortText,
				Design.ColoedString(
					this.NameColor,
					Tr.GetString(
						$"{this.Id}ShortDescription")));
		}

		else
		{
			return Design.ColoedString(
				Palette.White,
				$"{this.GetColoredRoleName()}: {Tr.GetString("crewImportantText")}");
		}
	}

	public override string GetIntroDescription()
	{
		if (IsAwake)
		{
			return base.GetIntroDescription();
		}
		else
		{
			return Design.ColoedString(
				Palette.CrewmateBlue,
				PlayerControl.LocalPlayer.Data.Role.Blurb);
		}
	}

	public override Color GetNameColor(bool isTruthColor = false)
	{
		if (isTruthColor || IsAwake)
		{
			return base.GetNameColor(isTruthColor);
		}
		else
		{
			return Palette.White;
		}
	}

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

		if (ExtremeRoleManager.TryGetSafeCastedLocalRole<Servant>(out var servant) &&
			servant.Parent == this.targetPlayerId)
		{
			selfKill(rolePlayerId);
			return true;
		}

		bool isSuccess = this.mode switch
		{
			TargetMode.BothImpostorAndNautral => !role.IsCrewmate() && (this.canReplaceAssassin || role.Id != ExtremeRoleId.Assassin),
			TargetMode.Impostor => role.IsImpostor() && (this.canReplaceAssassin || role.Id != ExtremeRoleId.Assassin),
			TargetMode.Neutral => role.IsNeutral(),
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
		factory.CreateIntOption(
			Option.AwakeTaskGage,
			70, 0, 100, 10,
			format: OptionUnit.Percentage);

		factory.CreateIntOption(
			Option.AwakeDeadPlayerNum,
			7, 0, 12, 1);

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

		factory.CreateBoolOption(
			Option.IsDeadAbilityZero,
			true);

		var lowBreakerOpt = factory.CreateBoolOption(
			Option.IsMissingToDead, false);

		var lowBreakerKillOpt = factory.CreateBoolOption(
		   Option.LawbreakerCanKill,
		   true, lowBreakerOpt,
		   invert: true);

		var killCoolOption = factory.CreateBoolOption(
			KillerCommonOption.HasOtherKillCool,
			false, lowBreakerKillOpt,
			invert: true);
		factory.CreateFloatOption(
			KillerCommonOption.KillCoolDown,
			30f, 1.0f, 120f, 0.5f,
			killCoolOption, format: OptionUnit.Second);

		var killRangeOption = factory.CreateBoolOption(
			KillerCommonOption.HasOtherKillRange,
			false, lowBreakerKillOpt,
			invert: true);
		factory.CreateSelectionOption(
			KillerCommonOption.KillRange,
			OptionCreator.Range,
			killRangeOption);

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

		this.awakeTaskGage = loader.GetValue<Option, int>(Option.AwakeTaskGage) / 100.0f;
		this.awakeDeadPlayerNum = loader.GetValue<Option, int>(Option.AwakeDeadPlayerNum);

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

		this.awakeRole =
			this.awakeTaskGage <= 0.0f &&
			this.awakeDeadPlayerNum == 0;

		if (!this.awakeRole)
		{
			this.CanCallMeeting = true;
			this.awakeHasOtherVision = this.HasOtherVision;
			this.HasOtherVision = false;
		}

	}
	private static void selfKill(byte rolePlayerId)
	{
		Player.RpcUncheckMurderPlayer(
			rolePlayerId, rolePlayerId, byte.MaxValue);
		ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
			rolePlayerId,
			Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus.MissShot);
	}

	public string GetFakeOptionString() => "";

	public void Update(PlayerControl rolePlayer)
	{
		if (GameData.Instance == null ||
			CachedShipStatus.Instance == null ||
			!CachedShipStatus.Instance.enabled ||
			this.Button is null ||
			this.awakeRole)
		{
			return;
		}

		int deadPlayerNum = 0;
		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (player == null ||
				player.IsDead ||
				player.Disconnected)
			{
				++deadPlayerNum;
			}
		}

		if (deadPlayerNum >= this.awakeDeadPlayerNum &&
			Player.GetPlayerTaskGage(rolePlayer) >= this.awakeTaskGage)
		{
			this.awakeRole = true;
			this.CanCallMeeting = false;
			this.HasOtherVision = this.awakeHasOtherVision;
			this.Button.SetButtonShow(true);
		}
		else
		{
			this.Button.SetButtonShow(false);
		}
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
		ColorPalette.YardbirdYenHown,
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
		ColorPalette.LowbreakerNoir,
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