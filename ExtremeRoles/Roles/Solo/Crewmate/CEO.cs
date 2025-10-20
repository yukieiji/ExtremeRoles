using System;
using System.Collections.Generic;

using AmongUs.GameOptions;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;


namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class CEOAbilityHandler : IAbility, IExiledAnimationOverrideWhenExiled
{
	public string AnimationText => "CEO権限により本投票は無効になりました";

	public NetworkedPlayerInfo OverideExiledTarget => null;
}

public sealed class CEO : SingleRoleBase,
	IRoleAwake<RoleTypes>,
	IRoleVoteModifier
{
	public bool IsAwake { get; private set; }

	public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

	public int Order => (int)IRoleVoteModifier.ModOrder.CEOOverrideVote;


	private bool isShowRolePlayerVote;
	private bool useCEOmeeting;


	public CEO() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Captain,
			ColorPalette.CaptainLightKonjou),
		false, true, false, false)
	{ }

	public string GetFakeOptionString() => "";

	public IEnumerable<VoteInfo> GetModdedVoteInfo(NetworkedPlayerInfo rolePlayer)
	{
		yield break;
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		// 死んでも蘇らせる
		rolePlayer.Revive();

		if (!this.useCEOmeeting)
		{
			return;
		}

		if (!OnemanMeetingSystemManager.TryGetSystem(out var system))
		{
			return;
		}
		system.Start(rolePlayer, OnemanMeetingSystemManager.Type.CEO, null);
	}

	public void ModifiedVote(byte rolePlayerId, ref Dictionary<byte, byte> voteTarget, ref Dictionary<byte, int> voteResult)
	{
		if (this.isShowRolePlayerVote || !this.IsAwake || voteResult.Count <= 0)
		{
			return;
		}
		
		bool isTie = false;
		bool isMeExiled = false;
		int maxNum = -50;
		
		foreach (var (playerId, voteNum) in voteResult)
		{

			if (maxNum < voteNum)
			{
				continue;
			}


			isTie = maxNum == voteNum;

			maxNum = voteNum;
			isMeExiled = playerId == rolePlayerId;
		}

		// 自分自身が吊られるときは何もいじらない
		if (isMeExiled && !isTie)
		{
			return;
		}

		// 票を消し飛ばす
		voteResult.Remove(rolePlayerId);
		voteTarget.Remove(rolePlayerId);

	}

	public void ResetModifier()
	{

	}

	public void Update(PlayerControl rolePlayer)
	{
		if (!GameProgressSystem.IsTaskPhase)
		{
			return;
		}
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		throw new NotImplementedException();
	}

	protected override void RoleSpecificInit()
	{
		throw new NotImplementedException();
	}
}
