using System.Collections.Generic;

using Hazel;

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
	public OverrideInfo? OverrideInfo => this.status.IsAwake ? new OverrideInfo(null, "CEO権限により本投票は無効になりました") : null;
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

	public enum Ops
	{
		Awake,
		ExiledMe
	}

	public bool IsAwake
	{
		get => this.status is not null && this.status.IsAwake;
		set
		{
			if (this.status is not null)
			{
				this.status.IsAwake = value;
			}
		}
	}

	public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

	public int Order => (int)IRoleVoteModifier.ModOrder.CEOOverrideVote;

	private bool isShowRolePlayerVote;
	private bool useCEOMeeting;

	public override IStatusModel? Status => status;

	private CEOStatus? status;

	private float awakeTaskGage;
	private bool awakeHasOtherVision;

	private bool isMeExiled = false;

	public CEO() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.CEO,
			ColorPalette.CaptainLightKonjou),
		false, true, false, false)
	{ }

	public string GetFakeOptionString() => "";

	public IEnumerable<VoteInfo> GetModdedVoteInfo(
		VoteInfoCollector collector,
		NetworkedPlayerInfo rolePlayer)
	{
		if (this.isShowRolePlayerVote || !this.IsAwake || this.isMeExiled)
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

	public static void RpcOps(MessageReader reader)
	{
		var ops = (Ops)reader.ReadByte();
		byte rolePlayerId = reader.ReadByte();

		if (!ExtremeRoleManager.TryGetSafeCastedRole<CEO>(rolePlayerId, out var role))
		{
			return;
		}

		switch (ops)
		{
			case Ops.Awake:
				if (role.Status is CEOStatus ceoStatus)
				{
					ceoStatus.IsAwake = true;
				}
				break;
			case Ops.ExiledMe:
				role.isMeExiled = true;
				break;
		}
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
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
		int maxNum = int.MinValue;
		this.isMeExiled = false;

		foreach (var (playerId, voteNum) in voteResult)
		{

			if (maxNum > voteNum)
			{
				continue;
			}


			isTie = maxNum == voteNum;

			maxNum = voteNum;
			this.isMeExiled = playerId == rolePlayerId;
		}

		if (this.isMeExiled && !isTie)
		{
			using (var op = RPCOperator.CreateCaller(
				RPCOperator.Command.CEOOps))
			{
				op.WriteByte((byte)Ops.ExiledMe);
				op.WriteByte(rolePlayerId);
			}

			return;
		}

		voteResult.Remove(rolePlayerId);
	}

	public void ResetModifier()
	{
		this.isMeExiled = false;
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (!GameProgressSystem.IsTaskPhase || this.IsAwake)
		{
			return;
		}

		if (Player.GetPlayerTaskGage(rolePlayer) < this.awakeTaskGage)
		{
			return;
		}

		this.IsAwake = true;
		this.HasOtherVision = this.awakeHasOtherVision;

		using (var op = RPCOperator.CreateCaller(
			RPCOperator.Command.CEOOps))
		{
			op.WriteByte((byte)Ops.Awake);
			op.WriteByte(rolePlayer.PlayerId);
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
		this.status = new CEOStatus();
		this.AbilityClass = new CEOAbilityHandler(this.status);


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
