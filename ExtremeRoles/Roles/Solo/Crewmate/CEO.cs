using System;
using System.Collections.Generic;

using AmongUs.GameOptions;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class CEOAbilityHandler(CEOStatus status) : IAbility, IExiledAnimationOverrideWhenExiled
{
	private readonly CEOStatus status = status;
	public OverideInfo? OverideInfo => this.status.IsAwake ? new OverideInfo(null, "CEO権限により本投票は無効になりました") : null;
}

public sealed class CEOStatus : IStatusModel
{
	public bool IsAwake { get; set; }
}

public sealed class CEO : SingleRoleBase,
	IRoleAwake<RoleTypes>,
	IRoleVoteModifier
{
	public enum Option
	{
		AwakeTaskGage,
		IsShowRolePlayerVote,
		IsUseCEOMeeting
	}

	public bool IsAwake
	{
		get => this.staus is not null && this.staus.IsAwake;
		set
		{
			if (this.staus is not null)
			{
				this.staus.IsAwake = value;
			}
		}
	}

	public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

	public int Order => (int)IRoleVoteModifier.ModOrder.CEOOverrideVote;

	private bool isShowRolePlayerVote;
	private bool useCEOMeeting;

	public override IStatusModel? Status => staus;

	private CEOStatus? staus;

	private float awakeTaskGage;
	private bool awakeHasOtherVision;

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

		if (!this.useCEOMeeting)
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

			if (maxNum > voteNum)
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
		if (!(
				GameProgressSystem.IsTaskPhase &&
				this.IsAwake
			))
		{
			return;
		}

		if (Player.GetPlayerTaskGage(rolePlayer) >= this.awakeTaskGage)
		{
			this.IsAwake = true;
			this.HasOtherVision = this.awakeHasOtherVision;
		}

	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.Create0To100Percentage10StepOption(Option.AwakeTaskGage, defaultGage: 50);
		factory.CreateBoolOption(Option.IsShowRolePlayerVote, true);
		factory.CreateBoolOption(Option.IsUseCEOMeeting, true);
	}

	protected override void RoleSpecificInit()
	{
		this.staus = new CEOStatus();
		this.AbilityClass = new CEOAbilityHandler(this.staus);


		this.isShowRolePlayerVote = this.Loader.GetValue<Option, bool>(Option.IsShowRolePlayerVote);
		this.useCEOMeeting = this.Loader.GetValue<Option, bool>(Option.IsUseCEOMeeting);

		this.awakeTaskGage = this.Loader.GetValue<Option, int>(Option.AwakeTaskGage) / 100.0f;
		this.awakeHasOtherVision = this.HasOtherVision;

		if (this.awakeTaskGage <= 0.0f)
		{
			this.IsAwake = true;
			this.HasOtherVision = this.awakeHasOtherVision;
		}
		else
		{
			this.IsAwake = false;
			this.HasOtherVision = false;
		}
	}
}
