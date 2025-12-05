using AmongUs.GameOptions;
using System.Collections.Generic;

using Hazel;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Module.CustomOption.Factory;


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
	IRoleVoteModifier,
	IRoleResetMeeting
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

	private bool isMonikaMeeting = false;
	private bool isMeExiled = false;
	private float exiledTimer = 0.0f;
	private TMPro.TextMeshPro? resurrectText;

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
		if (this.isShowRolePlayerVote || !this.IsAwake || this.isMeExiled || OnemanMeetingSystemManager.IsActive)
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
		if (this.isMonikaMeeting)
		{
			this.isMonikaMeeting = false;
			return;
		}

		this.exiledTimer = 5.0f;
		
		if (OnemanMeetingSystemManager.IsActive ||
			!this.useCEOMeeting ||
			!OnemanMeetingSystemManager.TryGetSystem(out var system))
		{
			return;
		}
		system.Start(rolePlayer, OnemanMeetingSystemManager.Type.CEO, null);
	}

	public override string GetColoredRoleName(bool isTruthColor = false)
	{
		if (isTruthColor || IsAwake)
		{
			return base.GetColoredRoleName();
		}
		else
		{
			return Design.ColoredString(
				Palette.White,
				Tr.GetString(RoleTypes.Crewmate.ToString()));
		}
	}
	public override string GetFullDescription()
	{
		if (IsAwake)
		{
			return Tr.GetString(
				$"{this.Core.Id}FullDescription");
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
			return base.GetImportantText(isContainFakeTask);

		}
		else
		{
			return Design.ColoredString(
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
			return Design.ColoredString(
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


	public void ModifiedVote(byte rolePlayerId, ref Dictionary<byte, byte> voteTarget, ref Dictionary<byte, int> voteResult)
	{
		if (this.isShowRolePlayerVote || !this.IsAwake || voteResult.Count <= 0 || OnemanMeetingSystemManager.IsActive)
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

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{

	}

	public void ResetOnMeetingStart()
	{
		// モニカの会議かどうか確認する
		this.isMonikaMeeting =
			OnemanMeetingSystemManager.TryGetActiveSystem(out var system) &&
			system.TryGetOnemanMeeting<MonikaLoveTargetMeeting>(out _);

		if (this.resurrectText != null)
		{
			this.resurrectText.gameObject.SetActive(false);
		}
	}

	public void ResetModifier()
	{
		this.isMeExiled = false;
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (!GameProgressSystem.IsTaskPhase)
		{
			if (GameProgressSystem.Is(GameProgressSystem.Progress.Meeting) && 
				this.exiledTimer > 0.0f)
			{
				this.exiledTimer = 5.0f;
			}
			return;
		}

		if (this.IsAwake)
		{
			if (this.exiledTimer > 0.0f)
			{
				if (this.resurrectText == null)
				{
					this.resurrectText = Object.Instantiate(
						HudManager.Instance.KillButton.cooldownTimerText,
						Camera.main.transform, false);
					this.resurrectText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
					this.resurrectText.enableWordWrapping = false;
				}

				this.resurrectText.gameObject.SetActive(true);
				this.exiledTimer -= Time.deltaTime;
				this.resurrectText.text = Tr.GetString(
					"resurrectText",
					Mathf.CeilToInt(this.exiledTimer));

				if (this.exiledTimer <= 0.0f)
				{
					revive(rolePlayer);
				}
			}
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

	private void revive(PlayerControl rolePlayer)
	{
		if (rolePlayer == null)
		{
			return;
		}

		byte playerId = rolePlayer.PlayerId;

		Player.RpcUncheckRevive(playerId);

		if (rolePlayer.Data == null ||
			rolePlayer.Data.IsDead ||
			rolePlayer.Data.Disconnected)
		{
			return;
		}

		List<Vector2> randomPos = new List<Vector2>();
		Map.AddSpawnPoint(randomPos, playerId);

		Player.RpcUncheckSnap(playerId, randomPos[
			RandomGenerator.Instance.Next(randomPos.Count)]);

		HudManager.Instance.Chat.chatBubblePool.ReclaimAll();
		if (this.resurrectText != null)
		{
			this.resurrectText.gameObject.SetActive(false);
		}
	}
}
