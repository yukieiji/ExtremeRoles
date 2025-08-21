using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


#nullable enable

namespace ExtremeRoles.Roles.Combination.Barter;

public sealed class BarterManager : FlexibleCombinationRoleManagerBase
{
	public BarterManager() : base(
		CombinationRoleType.Guesser,
		new BarterRole(), 1)
	{ }

}

public sealed class BarterRole :
	MultiAssignRoleBase,
	IRoleResetMeeting,
	IRoleMeetingButtonAbility,
	IRoleUpdate
{

	public enum Option
	{
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
	private bool showOther = false;

	private string roleNamePrefix = "";

	public Sprite AbilityImage => UnityObjectLoader.LoadFromResources(ExtremeRoleId.Guesser);

	private const float defaultXPos = -2.85f;
	private const float subRoleXPos = -1.5f;

	public override IStatusModel? Status => this.status;
	private BarterStatus? status;


	private Dictionary<byte, SpriteRenderer> sourceMark = [];
	private VoteSwapSystem? system;

	public BarterRole(
		) : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Barter,
			ColorPalette.GuesserRedYellow),
		false, true, false, false,
		tab: OptionTab.CombinationTab)
	{ }

	public void Update(PlayerControl rolePlayer)
	{
		var meeting = MeetingHud.Instance;
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
		byte target = instance.TargetPlayerId;
		if (this.status is null ||
			!this.status.CanUseCastling() ||
			target == PlayerVoteArea.DeadVote)
		{
			return false;
		}
		return 
			!source.HasValue || 
			source.Value != target;
	}

	public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
	{

	}

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
					sourceMark.sprite = UnityObjectLoader.LoadSpriteFromResources(
						ObjectPath.CaptainSpecialVoteCheck);
					sourceMark.transform.localPosition = new Vector3(7.25f, -0.5f, -4f);
					sourceMark.transform.localScale = new Vector3(1.0f, 3.5f, 1.0f);
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
			system?.RpcSwapVote(source, target, this.showOther);
		}
		return execCastling;
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public void ResetOnMeetingStart()
	{
		this.status?.Reset();
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		var imposterSetting = factory.Get((int)CombinationRoleCommonOption.IsAssignImposter);
		CreateKillerOption(factory, imposterSetting);

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
			format: OptionUnit.Shot);
		factory.CreateBoolOption(Option.ShowCastlingOther, false);
	}

	protected override void RoleSpecificInit()
	{
		var loader = Loader;

		CanCallMeeting = loader.GetValue<Option, bool>(
			Option.CanCallMeeting);

		sourceMark = [];

		this.status = new BarterStatus(loader);
		this.showOther = loader.GetValue<Option, bool>(
			Option.ShowCastlingOther);

		roleNamePrefix = CreateImpCrewPrefix();
	}

	private void meetingInfoSetActive(bool active)
	{
		if (meetingCastlingText != null)
		{
			meetingCastlingText.gameObject.SetActive(active);
		}
	}
}
