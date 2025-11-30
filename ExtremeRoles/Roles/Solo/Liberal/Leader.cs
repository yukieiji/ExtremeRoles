using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class LeaderStatus : IStatusModel
{
	public int OtherLiberal { get; private set; }

	public void Update()
	{
		OtherLiberal = 0;
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
			++OtherLiberal;
		}

	}
}

public readonly struct LeaderCoreOption(LiberalDefaultOptipnLoader option)
{
	public readonly bool IsAutoExit = option.TryGet(LiberalGlobalSetting.IsAutoExitWhenLeaderSolo, out var autoExitSetting) &&
		autoExitSetting.IsViewActive && autoExitSetting.Value<bool>();
	public readonly bool IsAutoRevive =
			option.TryGet(LiberalGlobalSetting.IsAutoRevive, out var autoReviveSetting) &&
			autoReviveSetting.IsViewActive && autoReviveSetting.Value<bool>();

	public readonly float KilledBoost = option.GetValue<LiberalGlobalSetting, float>(LiberalGlobalSetting.LeaderKilledBoost);

	public readonly int KillMoney = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.KillMoney);
	public readonly float KillBoost = option.GetValue<LiberalGlobalSetting, float>(LiberalGlobalSetting.LeaderKillBoost);

	public readonly int TaskMoney = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.TaskCompletedMoney);
	public readonly float TaskBoot = option.GetValue<LiberalGlobalSetting, float>(LiberalGlobalSetting.LeaderTaskBoost);
}


public sealed class LeaderAbilityHandler(
	LiberalDefaultOptipnLoader option,
	LeaderStatus status) : IAbility, IInvincible
{
	private readonly LeaderStatus status = status;
	private readonly bool isBlockKill = !option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.CanKilledLeader);
	private readonly bool isAutoCanKillWhenSolo = 
		option.TryGet(LiberalGlobalSetting.CanKilledWhenLeaderSolo, out var autoCanKillSetting) &&
		autoCanKillSetting.IsViewActive && autoCanKillSetting.Value<bool>();

	// 設定次第でキル等の対象には取れる
	public bool IsBlockKillFrom(byte? fromPlayer)
	{
		if (this.isAutoCanKillWhenSolo && this.status.OtherLiberal <= 0)
		{
			return false;
		}
		return isBlockKill;
	}
	public bool IsValidKillFromSource(byte target)
		=> !IsBlockKillFrom(target);

	// リーダーが消えるのは困るので能力の対象には取れない
	public bool IsValidAbilitySource(byte source)
		=> false;
}

public sealed class Leader : SingleRoleBase, IRoleVoteModifier, IRoleUpdate
{
	private readonly LiberalMoneyBankSystem system;

	public int Order => 114514;
	public override IStatusModel? Status => this.status;

	private readonly LeaderStatus status;
	private readonly LeaderAbilityHandler abilityHandler;
	private readonly LeaderCoreOption option;

	public Leader(
		LeaderCoreOption leaderCoreOption,
		LiberalDefaultOptipnLoader option,
		LeaderStatus status,
		LiberalMoneyBankSystem system) : base(
		RoleCore.BuildLiberal(
			ExtremeRoleId.Leader,
			ColorPalette.AgencyYellowGreen),
		option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.CanKillLeader),
		option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.CanHasTaskLeader),
		false, false)
	{
		this.system = system;

		this.status = status;
		this.abilityHandler = new LeaderAbilityHandler(option, status);
		this.AbilityClass = this.abilityHandler;

		this.option = leaderCoreOption;

		LiberalSettingOverrider.OverrideDefault(this, option);

		this.HasOtherVision = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.LeaderHasOtherVisonSize);
		if (this.HasOtherVision)
		{
			this.Vision = option.GetValue<LiberalGlobalSetting, float>(LiberalGlobalSetting.LeaderVison);
		}
		if (!this.CanKill)
		{
			return;
		}
		this.HasOtherKillRange = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.LeaderHasOtherKillRange);
		if (this.HasOtherKillRange)
		{
			this.KillRange = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.LeaderKillRange);
		}

		this.HasOtherKillCool = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.LeaderHasOtherKillCool);
		if (this.HasOtherKillCool)
		{
			this.KillCoolTime = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.LeaderKillCool);
		}
	}

	public override string GetRoleTag()
		=> $" ({Mathf.CeilToInt(this.system.Money)}/{Mathf.CeilToInt(this.system.WinMoney)})";
	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
		=> this.GetRoleTag();

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

		// ローカルの人以外のstatusは更新されてないので更新をここでいれる
		if (PlayerControl.LocalPlayer == null ||
			rolePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			this.status.Update();
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

	public void ResetModifier()
	{

	}

	public void Update(PlayerControl rolePlayer)
	{
		if (!GameProgressSystem.IsGameNow)
		{
			return;
		}

		this.status.Update();

		if (this.status.OtherLiberal <= 0 && this.option.IsAutoExit)
		{
			// 死亡処理を入れる
		}

		// 無敵のときに死んだら復活処理する
		if (!this.option.IsAutoRevive)
		{
			return;
		}
		// 復活処理をここに書く
	}
}
