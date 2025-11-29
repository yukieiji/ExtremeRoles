using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.Meeting;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class LeaderAbilityHandler(LiberalDefaultOptipnLoader option) : IAbility, IInvincible
{
	private readonly bool isBlockKill = !option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.CanKilledLeader);

	// 設定次第でキル等の対象には取れる
	public bool IsBlockKillFrom(byte? fromPlayer)
		=> isBlockKill;
	public bool IsValidKillFromSource(byte target)
		=> !isBlockKill;

	// リーダーが消えるのは困るので能力の対象には取れない
	public bool IsValidAbilitySource(byte source)
		=> false;
}

public sealed class Leader : SingleRoleBase, IRoleVoteModifier
{
	private readonly LiberalMoneyBankSystem system;

	public int Order => 114514;

	private readonly LeaderAbilityHandler abilityHandler;

	public Leader(
		LiberalDefaultOptipnLoader option,
		LeaderAbilityHandler abilityHandler,
		LiberalMoneyBankSystem system) : base(
		RoleCore.BuildLiberal(
			ExtremeRoleId.Leader,
			ColorPalette.AgencyYellowGreen),
		option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.CanKillLeader),
		option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.CanHasTaskLeader),
		false, false)
	{
		this.system = system;

		this.abilityHandler = abilityHandler;
		this.AbilityClass = abilityHandler;

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
		if (!this.abilityHandler.IsBlockKillFrom(null) || voteResult.Count <= 0)
		{
			return;
		}
		voteResult.Remove(rolePlayerId);
	}

	public IEnumerable<VoteInfo> GetModdedVoteInfo(VoteInfoCollector collector, NetworkedPlayerInfo rolePlayer)
	{
		if (!this.abilityHandler.IsBlockKillFrom(null))
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

	public void ResetModifier()
	{

	}
}
