﻿using System;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Performance;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Patches.Button;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Roles.Solo.Neutral;

#nullable enable

public sealed class Heretic :
	SingleRoleBase,
	IRoleAutoBuildAbility,
	IRoleUpdate,
	IRoleWinPlayerModifier,
	IRoleMeetingButtonAbility
{
	public enum Option
	{
		HasTask,
		SeeImpostorTaskGage,

		KillMode,
		CanKillImpostor,
		Range,
	}

	public enum KillMode : byte
	{
		OnTaskPhase,
		OnTaskPhaseTarget,
		OnMeeting,
		OnMeetingTarget,
		OnExiled,
	}

	public ExtremeAbilityButton? Button { get; set; }

	private bool canKillImpostor;
	private KillMode killMode;
	private float range;
	private PlayerControl? target;
	private byte meetingTarget = byte.MaxValue;
	private bool called = false;

	private bool isSeeImpostorNow = false;
	private float seeImpostorTaskGage;
	private Sprite sprite => UnityObjectLoader.LoadFromResources(ExtremeRoleId.Guesser);

	public Heretic() : base(
		ExtremeRoleId.Heretic,
		ExtremeRoleType.Neutral,
		ExtremeRoleId.Heretic.ToString(),
		Palette.ImpostorRed,
		false, false, false, false,
		canCallMeeting: false,
		canRepairSabotage: false)
	{ }

	public void ModifiedWinPlayer(
		NetworkedPlayerInfo rolePlayerInfo,
		GameOverReason reason,
		in WinnerTempData winner)
	{
		switch (reason)
		{
			case GameOverReason.ImpostorsByVote:
			case GameOverReason.ImpostorsByKill:
			case GameOverReason.ImpostorsBySabotage:
			case GameOverReason.ImpostorDisconnect:
			case GameOverReason.HideAndSeek_ImpostorsByKills:
			case (GameOverReason)RoleGameOverReason.AssassinationMarin:
			case (GameOverReason)RoleGameOverReason.TeroristoTeroWithShip:
				winner.AddWithPlus(rolePlayerInfo);
				break;
			default:
				break;
		}
	}

	public void CreateAbility()
	{
		this.CreateNormalAbilityButton(
			"selfKill", this.sprite);
	}

	public bool IsAbilityUse()
	{
		this.target = null;
		if (this.killMode is KillMode.OnExiled)
		{
			return false;
		}

		bool commonUse = IRoleAbility.IsCommonUse();
		if (!commonUse)
		{
			return false;
		}

		var player = Player.GetClosestPlayerInRange(
		   PlayerControl.LocalPlayer, this, this.range);

		if (player == null ||
			this.meetingTarget == player.PlayerId)
		{
			return false;
		}
		this.target = player;

		return
			this.canKillImpostor ||
			(
				ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) &&
				!role.IsImpostor()
			);
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		if (this.called ||
			exiledPlayer == null ||
			this.killMode is
				KillMode.OnTaskPhase or KillMode.OnMeeting ||
			exiledPlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId)
		{
			return;
		}

		this.called = true;
		byte targetPlayerId = this.meetingTarget;

		var player = Player.GetPlayerControlById(targetPlayerId);
		if (!player.IsValid())
		{
			targetPlayerId = byte.MaxValue;
		}
		if (targetPlayerId == byte.MaxValue)
		{
			// ランダムなプレイヤー選択
			var target = PlayerCache.AllPlayerControl.Where(
				player =>
					player.IsValid() &&
					player.PlayerId != exiledPlayer.PlayerId &&
					this.canKillImpostor ||
					(
						ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) &&
						!role.IsImpostor()
					))
				.OrderBy(x => RandomGenerator.Instance.Next())
				.FirstOrDefault();
			if (target == null)
			{
				return;
			}
			targetPlayerId = target.PlayerId;
		}
		Player.RpcUncheckExiled(targetPlayerId);
	}

	public override bool IsSameTeam(SingleRoleBase targetRole)
		=> this.IsNeutralSameTeam(targetRole) ||
			(targetRole.IsImpostor() && this.canKillImpostor);

	public void ResetOnMeetingStart()
	{ }

	public void Update(PlayerControl rolePlayer)
	{
		if (MeetingHud.Instance != null ||
			ExileController.Instance != null ||
			GameManager.Instance == null)
		{
			return;
		}

		this.Button?.SetButtonShow(
			this.killMode is KillMode.OnTaskPhase or KillMode.OnTaskPhaseTarget);

		if (!this.HasTask || this.isSeeImpostorNow)
		{
			return;
		}

		float taskGage = Player.GetPlayerTaskGage(rolePlayer);
		if (taskGage >= this.seeImpostorTaskGage &&
			!this.isSeeImpostorNow)
		{
			this.isSeeImpostorNow = true;
		}
	}

	public bool UseAbility()
	{
		ExtremeRolesPlugin.Logger.LogInfo($"AbilityMode: {this.killMode}");
		if (this.target == null)
		{
			return false;
		}

		switch (this.killMode)
		{
			case KillMode.OnTaskPhase:
				var killer = PlayerControl.LocalPlayer;
				if (killer == null)
				{
					return false;
				}
				tryKill(killer, this.target);
				break;
			case KillMode.OnTaskPhaseTarget:
				this.meetingTarget = this.target.PlayerId;
				break;
			default:
				return false;
		}
		return true;
	}

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (this.meetingTarget != byte.MaxValue &&
			targetPlayerId == this.meetingTarget)
		{
			return Design.ColoedString(Palette.ImpostorRed, " ×");
		}
		return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
	}

	public override Color GetTargetRoleSeeColor(
		SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (this.isSeeImpostorNow &&
			(targetRole.IsImpostor() || targetRole.FakeImposter))
		{
			return Palette.ImpostorRed;
		}

		return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		var taskOpt = factory.CreateBoolOption(
			Option.HasTask,
			false);
		factory.Create0To100Percentage10StepOption(
			Option.SeeImpostorTaskGage, taskOpt);
		var killModeOpt = factory.CreateSelectionOption(
			Option.KillMode,
			[
				KillMode.OnTaskPhase,
				KillMode.OnTaskPhaseTarget
			]);
		factory.CreateFloatOption(
			RoleAbilityCommonOption.AbilityCoolTime,
			IRoleAbility.DefaultCoolTime,
			IRoleAbility.MinCoolTime,
			IRoleAbility.MaxCoolTime,
			IRoleAbility.Step,
			killModeOpt,
			invert: true,
			format: OptionUnit.Second);
		factory.CreateFloatOption(
			Option.Range,
			1.2f, 0.1f, 2.5f, 0.1f,
			killModeOpt,
			invert: true);

		factory.CreateBoolOption(
			Option.CanKillImpostor,
			false);
	}

	protected override void RoleSpecificInit()
	{
		this.target = null;
		this.called = false;

		var loader = this.Loader;

		this.HasTask = loader.GetValue<Option, bool>(
			Option.HasTask);
		this.seeImpostorTaskGage = loader.GetValue<Option, int>(
			Option.SeeImpostorTaskGage) / 100.0f;
		this.canKillImpostor = loader.GetValue<Option, bool>(
			Option.CanKillImpostor);
		this.killMode = (KillMode)loader.GetValue<Option, int>(Option.KillMode);
		this.range = loader.GetValue<Option, float>(Option.Range);
		this.isSeeImpostorNow = this.HasTask && this.seeImpostorTaskGage > 0;

	}

	public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance)
		=> PlayerControl.LocalPlayer == null ||
			instance.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId ||
			!(this.killMode is KillMode.OnMeeting or KillMode.OnMeetingTarget);

	public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
		=> IRoleMeetingButtonAbility.DefaultButtonMod(instance, abilityButton, "hereticKillTarget");

	public Action CreateAbilityAction(PlayerVoteArea instance)
		=> () =>
		{
			if (instance.AmDead)
			{
				return;
			}

			this.meetingTarget = instance.TargetPlayerId;

			if (this.killMode is KillMode.OnMeetingTarget)
			{
				return;
			}

			byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;

			// 二人殺すので二回ならす
			Sound.RpcPlaySound(Sound.Type.Kill);
			if (!(
					BodyGuard.IsBlockMeetingKill &&
					BodyGuard.TryRpcKillGuardedBodyGuard(
						localPlayerId, this.meetingTarget)
				))
			{
				Player.RpcUncheckMurderPlayer(
					localPlayerId,
					this.meetingTarget, byte.MinValue);
			}

			Sound.RpcPlaySound(Sound.Type.Kill);
			Player.RpcUncheckMurderPlayer(
				localPlayerId, localPlayerId,
				byte.MinValue);
		};

	public Sprite AbilityImage => this.sprite;

	private void tryKill(PlayerControl killer, PlayerControl target)
	{
		var condition = KillButtonDoClickPatch.CheckPreKillCondition(this, killer, target);
		ExtremeRolesPlugin.Logger.LogInfo($"KillCheck Condition: {condition}");
		switch (condition)
		{
			case KillButtonDoClickPatch.KillResult.BlockedToBodyguard:
				break;
			case KillButtonDoClickPatch.KillResult.Success:
				Player.RpcUncheckMurderPlayer(
					killer.PlayerId, target.PlayerId,
					byte.MinValue);
				break;
			default:
				return;
		}
		Player.RpcUncheckMurderPlayer(
			killer.PlayerId, killer.PlayerId,
			byte.MinValue);
	}
}
