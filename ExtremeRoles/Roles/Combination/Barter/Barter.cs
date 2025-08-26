using AmongUs.GameOptions;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


#nullable enable

namespace ExtremeRoles.Roles.Combination.Barter;

public sealed class BarterManager : FlexibleCombinationRoleManagerBase
{
	public BarterManager() : base(
		CombinationRoleType.Barter,
		new BarterRole(), 1)
	{ }

}

public sealed class BarterRole :
	MultiAssignRoleBase,
	IRoleResetMeeting,
	IRoleMeetingButtonAbility,
	IRoleAwake<RoleTypes>
{

	public enum Option
	{
		AwakeTaskRate,
		AwakeDeadPlayerNum,
		AwakeKillNum,
		CanCallMeeting,
		CastlingNum,
		MaxCastlingNumWhenMeeting,
		RandomCastling,
		OneCastlingNum,
		ShowCastlingOther,
	}

	public override string RoleName =>
		string.Concat(roleNamePrefix, Core.Name);
	private TextMeshPro? meetingCastlingText = null;
	private byte? source = null;
	private VoteSwapSystem.ShowOps showOps = VoteSwapSystem.ShowOps.Hide;

	private bool awakeHasOtherVision;
	private bool awakeHasOtherKillCool;
	private bool awakeHasOtherKillRange;


	private string roleNamePrefix = "";

	public Sprite AbilityImage => UnityObjectLoader.LoadFromResources(ExtremeRoleId.Barter);

	private const float defaultXPos = -2.85f;
	private const float subRoleXPos = -1.5f;

	public override IStatusModel? Status => this.status;

	public bool IsAwake => this.status is not null && this.status.IsAwake;

	public RoleTypes NoneAwakeRole => 
		this.IsImpostor() ? 
		RoleTypes.Impostor : RoleTypes.Crewmate;

	private BarterStatus? status;


	private Dictionary<byte, SpriteRenderer> sourceMark = [];
	private VoteSwapSystem? system;

	public BarterRole(
		) : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Barter,
			ColorPalette.BarterUsusuou),
		false, true, false, false,
		tab: OptionTab.CombinationTab)
	{ }

	public void Update(PlayerControl rolePlayer)
	{
		var meeting = MeetingHud.Instance;

		if (!this.IsAwake)
		{
			if (this.IsCrewmate())
			{
				this.updateAwakeStatus(rolePlayer);
			}
			return;
		}

		if (meeting != null && this.status is not null)
		{
			if (meetingCastlingText == null)
			{
				meetingCastlingText = UnityEngine.Object.Instantiate(
					HudManager.Instance.TaskPanel.taskText,
					meeting.transform);
				meetingCastlingText.alignment = TextAlignmentOptions.BottomLeft;
				meetingCastlingText.transform.position = Vector3.zero;

				float xPos = AnotherRole != null ? subRoleXPos : defaultXPos;

				meetingCastlingText.transform.localPosition = new Vector3(xPos, 3.15f, -20f);
				meetingCastlingText.transform.localScale *= 0.9f;
				meetingCastlingText.color = Palette.White;
				meetingCastlingText.gameObject.SetActive(false);
			}

			meetingCastlingText.text = this.status.CastlingStatus();
			if (this.status.IsRandomCastling)
			{
				// ランダムキャスリングのボタン等の設定
			}
			meetingInfoSetActive(true);
		}
		else
		{
			meetingInfoSetActive(false);
		}
	}

	public void IntroEndSetUp()
	{
		return;
	}

	public bool IsBlockMeetingButtonAbility(
		PlayerVoteArea instance)
	{
		if (!this.IsAwake)
		{
			return true;
		}

		byte target = instance.TargetPlayerId;
		if (this.status is null ||
			!this.status.CanUseCastling() ||
			target == PlayerVoteArea.DeadVote ||
			target == PlayerVoteArea.SkippedVote)
		{
			return true;
		}
		return 
			source.HasValue && 
			source.Value == target;
	}

	public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
		=> IRoleMeetingButtonAbility.DefaultButtonMod(instance, abilityButton, "barterCastling");

	public Action CreateAbilityAction(PlayerVoteArea instance)
	{
		void execCastling()
		{
			byte target = instance.TargetPlayerId;
			if (!this.source.HasValue)
			{
				this.source = target;

				if (!this.sourceMark.TryGetValue(
						target,
						out SpriteRenderer? sourceMark) ||
					sourceMark == null)
				{
					sourceMark = UnityEngine.Object.Instantiate(
						instance.Background, instance.LevelNumberText.transform);
					sourceMark.name = $"captain_SpecialVoteCheckMark_{target}";
					sourceMark.sprite = UnityObjectLoader.LoadFromResources<Sprite>(
						ObjectPath.CommonTextureAsset,
						string.Format(
							ObjectPath.CommonImagePathFormat, 
							ObjectPath.VoteSwapSource));
					sourceMark.transform.localPosition = new Vector3(7.25f, -0.5f, -4f);
					sourceMark.transform.localScale = new Vector3(0.5f, 3.0f, 1.0f);
					sourceMark.gameObject.layer = 5;
					this.sourceMark[target] = sourceMark;
				}
				sourceMark.gameObject.SetActive(true);
				return;
			}
			byte source = this.source.Value;
			if (sourceMark.TryGetValue(source, out var rend))
			{
				rend.gameObject.SetActive(false);
			}
			system?.RpcSwapVote(source, target, this.showOps);
			this.source = null;
		}
		return execCastling;
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public void ResetOnMeetingStart()
	{
		this.status?.Reset();
		this.source = null;
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		var imposterSetting = factory.Get((int)CombinationRoleCommonOption.IsAssignImposter);
		CreateKillerOption(factory, imposterSetting);

		factory.CreateIntOption(
			Option.AwakeTaskRate,
			70, 0, 100, 10,
			format: OptionUnit.Percentage);

		factory.CreateIntOption(
			Option.AwakeDeadPlayerNum,
			7, 0, 12, 1);
		factory.CreateIntOption(
			Option.AwakeKillNum,
			2, 0, 5, 1);

		factory.CreateBoolOption(
			Option.CanCallMeeting,
			false);
		factory.CreateIntOption(
			Option.CastlingNum,
			1, 1, 100, 1,
			format: OptionUnit.Shot);
		factory.CreateIntOption(
			Option.MaxCastlingNumWhenMeeting,
			1, 1, 25, 1,
			format: OptionUnit.Shot);

		var randOpt = factory.CreateBoolOption(Option.RandomCastling, false);
		factory.CreateIntOption(
			Option.OneCastlingNum, 1, 1, 25, 1,
			randOpt,
			format: OptionUnit.Shot);
		factory.CreateBoolOption(Option.ShowCastlingOther, false);
	}

	protected override void RoleSpecificInit()
	{
		var loader = Loader;

		this.sourceMark = [];

		this.source = null;
		this.status = new BarterStatus(loader, this.IsImpostor());
		this.showOps = loader.GetValue<Option, bool>(
			Option.ShowCastlingOther) ?
			VoteSwapSystem.ShowOps.ShowAll : VoteSwapSystem.ShowOps.ShowOnlyCaller;

		this.roleNamePrefix = CreateImpCrewPrefix();
		this.system = VoteSwapSystem.CreateOrGet();

		if (!this.status.IsAwake)
		{
			this.awakeHasOtherVision = this.HasOtherVision;
			this.HasOtherVision = false;
			this.CanCallMeeting = true;

			if (this.IsImpostor())
			{
				this.awakeHasOtherKillCool = this.HasOtherKillCool;
				this.awakeHasOtherKillRange = this.HasOtherKillRange;
				this.HasOtherKillCool = false;
				this.HasOtherKillRange = false;
			}
		}
	}

	private void meetingInfoSetActive(bool active)
	{
		if (meetingCastlingText != null)
		{
			meetingCastlingText.gameObject.SetActive(active);
		}
	}

	public string GetFakeOptionString()
		=> "";

	public override bool TryRolePlayerKillTo(PlayerControl rolePlayer, PlayerControl targetPlayer)
	{
		this.updateAwakeStatus(rolePlayer);
		return true;
	}

	public override string GetColoredRoleName(bool isTruthColor = false)
	{
		if (isTruthColor || IsAwake)
		{
			return base.GetColoredRoleName();
		}
		else
		{
			var type = this.IsImpostor() ? RoleTypes.Impostor : RoleTypes.Crewmate;
			return Design.ColoredString(
				this.IsImpostor() ? Palette.ImpostorRed : Palette.White,
				Tr.GetString(type.ToString()));
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
			var type = this.IsImpostor() ? RoleTypes.Impostor : RoleTypes.Crewmate;
			return Tr.GetString(
				$"{type}FullDescription");
		}
	}

	public override string GetImportantText(bool isContainFakeTask = true)
	{
		if (IsAwake)
		{
			return base.GetImportantText(isContainFakeTask);

		}
		else if (this.IsImpostor())
		{

			return string.Concat(
			[
            
                TranslationController.Instance.GetString(
                    StringNames.ImpostorTask, Array.Empty<Il2CppSystem.Object>()),
                "\r\n",
                Palette.ImpostorRed.ToTextColor(),
                TranslationController.Instance.GetString(
                    StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()),
                "</color>"
            ]);
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
				this.IsImpostor() ? Palette.ImpostorRed : Palette.CrewmateBlue,
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
			return this.IsImpostor() ? Palette.ImpostorRed : Palette.White;
		}
	}

	private void updateAwakeStatus(PlayerControl rolePlayer)
	{
		this.status?.UpdateAwakeStatus(rolePlayer);

		if (!this.IsAwake)
		{
			return;
		}

		this.CanCallMeeting = this.Loader.GetValue<Option, bool>(
			Option.CanCallMeeting);
		this.HasOtherVision = this.awakeHasOtherVision;
		if (this.IsImpostor() || 
			(this.AnotherRole != null && this.AnotherRole.Core.Id is ExtremeRoleId.Servant))
		{
			this.HasOtherKillCool = this.awakeHasOtherKillCool;
			this.HasOtherKillRange = this.awakeHasOtherKillRange;
		}
	}

	private void randomCastling()
	{
		if (MeetingHud.Instance == null ||
			this.status is null)
		{
			return;
		}
		var target = MeetingHud.Instance.playerStates
			.Select(x => x.TargetPlayerId)
			.Where(x => x != PlayerVoteArea.SkippedVote && x != PlayerVoteArea.DeadVote);

		for (int i = 0; i < this.status.OneCastlingNum; ++i)
		{
			byte[] item = target.OrderBy(x => RandomGenerator.Instance.Next()).Take(2).ToArray();
			this.system?.RpcSwapVote(item[0], item[1], this.showOps);
		}
	}
}
