using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Roles.API.Interface.Visual;
using ExtremeRoles.Roles.API.Extension.State;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class LeaderCoreOption(LiberalDefaultOptipnLoader option)
{
	public bool IsAutoExit { get; } = option.TryGet(LiberalGlobalSetting.IsAutoExitWhenLeaderSolo, out var autoExitSetting) &&
		autoExitSetting.IsViewActive && autoExitSetting.Value<bool>();
	public bool IsAutoRevive { get; } =
			option.TryGet(LiberalGlobalSetting.IsAutoRevive, out var autoReviveSetting) &&
			autoReviveSetting.IsViewActive && autoReviveSetting.Value<bool>();
	public bool IsAutoCanKillWhenSolo { get; } = 
		option.TryGet(LiberalGlobalSetting.CanKilledWhenLeaderSolo, out var autoCanKillSetting) &&
		autoCanKillSetting.IsViewActive && autoCanKillSetting.Value<bool>();

	public bool CanKilled { get; } = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.CanKilledLeader);

	public bool CanKill { get; } = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.CanKillLeader);
	public bool HasTask { get; } = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.CanHasTaskLeader);

	public bool HasOtherVison { get; } = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.LeaderHasOtherVisonSize);
	public float Vison { get; } = option.GetValue<LiberalGlobalSetting, float>(LiberalGlobalSetting.LeaderVison);

	public bool HasOtherKillRange { get; } = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.LeaderHasOtherKillRange);
	public int KillRange { get; } = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.LeaderKillRange);

	public bool HasOtherKillCool { get; } = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.LeaderHasOtherKillCool);
	public float KillCool { get; } = option.GetValue<LiberalGlobalSetting, float>(LiberalGlobalSetting.LeaderKillCool);

	public float KilledBoost { get; } = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.LeaderKilledBoost) / 100.0f;

	public int KillMoney { get; } = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.KillMoney);
	public int LeaderKillMoney { get; } = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.KillMoney);
	public float LeaderKillBoostDelta { get; } = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.LeaderKillBoost) / 100.0f;

	public int TaskMoney { get; } = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.TaskCompletedMoney);
	public float TaskBootDelta { get; } = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.LeaderTaskBoost) / 100.0f;
}

public sealed class LeaderVisual(LiberalMoneyBankSystem system) : IVisual, ILookedTag
{
	private readonly LiberalMoneyBankSystem system = system;

	public string Tag
		=> $" ({Mathf.CeilToInt(this.system.Money)}/{Mathf.CeilToInt(this.system.WinMoney)})";

	public string GetLookedToThisTag(byte _) => Tag;
}

public sealed class LeaderStatus : IStatusModel
{
	public int OtherLiberal
	{
		get
		{
			int result = 0;
			foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
			{
				if (player == null ||
					player.IsDead ||
					player.Disconnected ||
					!ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) ||
					!role.IsLiberal() ||
					role.Core.Id is ExtremeRoleId.Leader)
				{
					continue;
				}
				++result;
			}
			return result;
		}
	}
}

public sealed class LeaderAbilityHandler(
	LeaderCoreOption option,
	LeaderStatus status) : IAbility, IInvincible
{
	private readonly LeaderStatus status = status;
	private readonly bool isBlockKill = !option.CanKilled;
	private readonly bool isBlockKillWhenMultiLiberal = !option.IsAutoCanKillWhenSolo;

	public bool IsBlockKill => this.isBlockKill && (this.status.OtherLiberal > 0 || this.isBlockKillWhenMultiLiberal);

	// 設定次第でキル等の対象には取れる
	public bool IsBlockKillFrom(byte? fromPlayer)
		=> IsBlockKill;

	public bool IsValidKillFromSource(byte target)
		=> !IsBlockKillFrom(target);

	// リーダーが消えるのは困るので能力の対象には取れない
	public bool IsValidAbilitySource(byte source)
		=> false;
}

public sealed class Leader : SingleRoleBase, IRoleVoteModifier, IRoleUpdate, IRoleMurderPlayerHook
{
	public int Order => 114514;

	public override IVisual? Visual => this.visual;
	public override IStatusModel? Status => this.status;

	private readonly LeaderVisual visual;
	private readonly LeaderStatus status;
	private readonly LeaderAbilityHandler abilityHandler;

	private readonly DoveCommonAbilityHandler? doveHandler;
	private readonly ReviveSetting revive;

	private readonly record struct ReviveSetting(bool IsAutoExit, bool IsAutoRevive);
	private readonly PlayerReviver reviver;

	private readonly KillSetting killSetting;
	// リベラルがキルしたロジックはリーダーが全部引き受けるため
	private readonly record struct KillSetting(int KillMoney, int LeadeKillMoney, float LeadeKillBoostDelta);

	public Leader(
		LeaderVisual visual,
		LeaderCoreOption leaderCoreOption,
		LiberalDefaultOptipnLoader option,
		LeaderStatus status) : base(
		RoleCore.BuildLiberal(
			ExtremeRoleId.Leader,
			ColorPalette.LiberalColor),
		leaderCoreOption.CanKill,
		leaderCoreOption.HasTask,
		false, false)
	{
		this.visual = visual;
		this.status = status;

		this.abilityHandler = new LeaderAbilityHandler(leaderCoreOption, status);
		this.revive = new ReviveSetting(leaderCoreOption.IsAutoExit, leaderCoreOption.IsAutoRevive);
		this.killSetting = new KillSetting(leaderCoreOption.KillMoney, leaderCoreOption.LeaderKillMoney, leaderCoreOption.LeaderKillBoostDelta);
		this.reviver = new PlayerReviver(3.0f);
		this.AbilityClass = this.abilityHandler;

		LiberalSettingOverrider.OverrideDefault(this, option);

		this.doveHandler = this.HasTask ? new DoveCommonAbilityHandler(leaderCoreOption.TaskMoney, leaderCoreOption.TaskBootDelta) : null;

		this.HasOtherVision = leaderCoreOption.HasOtherVison;
		if (this.HasOtherVision)
		{
			this.Vision = leaderCoreOption.Vison;
		}
		if (!this.CanKill)
		{
			return;
		}

		this.HasOtherKillRange = leaderCoreOption.HasOtherKillRange;
		if (this.HasOtherKillRange)
		{
			this.KillRange = leaderCoreOption.KillRange;
		}

		this.HasOtherKillCool = leaderCoreOption.HasOtherKillCool;
		if (this.HasOtherKillCool)
		{
			this.KillCoolTime = leaderCoreOption.KillCool;
		}
	}

	public override string GetRoleTag()
		=> this.visual.Tag;

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (targetPlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			return GetRoleTag();
		}
		return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{

	}

	protected override void RoleSpecificInit()
	{

	}

	// 無敵 => 投票すら打ち消す
	public void ModifiedVote(byte rolePlayerId, ref Dictionary<byte, byte> voteTarget, ref Dictionary<byte, int> voteResult)
	{
		if (!this.abilityHandler.IsBlockKillFrom(null) || voteResult.Count <= 0 || OnemanMeetingSystemManager.IsActive)
		{
			return;
		}
		voteResult.Remove(rolePlayerId);
	}

	public IEnumerable<VoteInfo> GetModdedVoteInfo(VoteInfoCollector collector, NetworkedPlayerInfo rolePlayer)
	{
		if (!this.abilityHandler.IsBlockKillFrom(null) || OnemanMeetingSystemManager.IsActive)
		{
			yield break;
		}

		foreach (var info in collector.Vote)
		{
			// 自分に入っている票だけ打ち消す票情報を追加
			if (info.TargetId == rolePlayer.PlayerId &&
				info.Count > 0)
			{
				yield return new VoteInfo(info.VoterId, info.TargetId, -info.Count);
			}
		}
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		reviveIfValid(rolePlayer);
	}

	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		reviveIfValid(rolePlayer);
	}

	public void ResetModifier()
	{

	}

	public void Update(PlayerControl rolePlayer)
	{
		if (!GameProgressSystem.IsGameNow)
		{
			this.reviver.Reset();
			return;
		}

		this.doveHandler?.Update(rolePlayer);

		if (this.revive.IsAutoExit && this.status.OtherLiberal <= 0)
		{
			// リベラルが0になったので自動排除
			if (rolePlayer != null &&
				rolePlayer.Data != null &&
				!rolePlayer.Data.IsDead &&
				!rolePlayer.Data.Disconnected)
			{
				Player.RpcUncheckMurderPlayer(
					rolePlayer.PlayerId, rolePlayer.PlayerId,
					byte.MinValue);
			}
			return;
		}

		// 無敵のときに死んだら復活処理する
		if (!this.revive.IsAutoRevive)
		{
			return;
		}
		this.reviver.Update();
	}

	public void HookMuderPlayer(PlayerControl source, PlayerControl target)
	{
		if (!(
				ExtremeRoleManager.TryGetRole(source.PlayerId, out var role) &&
				role.IsLiberal() &&
				role.CanKill() &&
				source.PlayerId != target.PlayerId
			))
		{
			return;
		}
		
		bool isLeader = role.Core.Id is ExtremeRoleId.Leader;
		float money = isLeader ? this.killSetting.LeadeKillMoney : this.killSetting.KillMoney;
		float delta = isLeader ? this.killSetting.LeadeKillBoostDelta : 0.0f;

		LiberalMoneyBankSystem.RpcUpdateSystem(source.PlayerId, LiberalMoneyHistory.Reason.AddOnKill, money, delta);
	}

	private void reviveIfValid(PlayerControl rolePlayer)
	{
		if (rolePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId &&
			this.revive.IsAutoRevive &&
			(!this.revive.IsAutoExit || this.status.OtherLiberal > 0))
		{
			this.reviver.Start(rolePlayer);
		}
	}
}
