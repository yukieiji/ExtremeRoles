using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Patches.Button;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API.Extension.Neutral;

namespace ExtremeRoles.Roles.Solo.Neutral;

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
	}

	public enum KillMode
	{
		OnTaskPhase,
		OnTaskPhaseTarget,
		OnMeeting,
		OnMeetingTarget,
		OnExiled,
	}

	public ExtremeAbilityButton Button { get; set; }

	private bool canKillImpostor;
	private KillMode killMode;
	private float range;
	private byte target;
	private byte meetingTarget;

	private bool isSeeImpostorNow = false;
	private float seeImpostorTaskGage;
	private Sprite sprite => UnityObjectLoader.LoadFromResources(ExtremeRoleId.Heretic);

	public Heretic() : base(
		ExtremeRoleId.Heretic,
		ExtremeRoleType.Neutral,
		ExtremeRoleId.Heretic.ToString(),
		Palette.ImpostorRed,
		false, false, false, false,
		canRepairSabotage: false)
	{ }

	public void ModifiedWinPlayer(
		NetworkedPlayerInfo rolePlayerInfo,
		GameOverReason reason,
		in WinnerTempData winner)
	{
		switch (reason)
		{
			case GameOverReason.ImpostorByVote:
			case GameOverReason.ImpostorByKill:
			case GameOverReason.ImpostorBySabotage:
			case GameOverReason.ImpostorDisconnect:
			case GameOverReason.HideAndSeek_ByKills:
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
		this.target = byte.MaxValue;
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

		return
			this.canKillImpostor ||
			(
				ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) &&
				!role.IsImpostor()
			);
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
	{
		if (exiledPlayer == null ||
			this.killMode is not KillMode.OnExiled ||
			exiledPlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId)
		{
			return;
		}
		byte targetPlayerId = this.meetingTarget;
		if (targetPlayerId == byte.MaxValue)
		{
			// ランダムなプレイヤー選択
			var target = PlayerCache.AllPlayerControl.Where(
				player =>
					player.IsValid() &&
					player.PlayerId != PlayerControl.LocalPlayer.PlayerId &&
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
		this.Button.SetButtonShow(
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
				this.meetingTarget = this.target;
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
		factory.CreateSelectionOption<Option, KillMode>(
			Option.KillMode);
		factory.CreateBoolOption(
			Option.CanKillImpostor,
			false);
	}

	protected override void RoleSpecificInit()
	{
		this.target = byte.MaxValue;
		var loader = this.Loader;
		this.HasTask = loader.GetValue<Option, bool>(
			Option.HasTask);
		this.seeImpostorTaskGage = loader.GetValue<Option, int>(
			Option.SeeImpostorTaskGage) / 100.0f;
		this.canKillImpostor = loader.GetValue<Option, bool>(
			Option.CanKillImpostor);
		this.killMode = (KillMode)loader.GetValue<Option, int>(Option.KillMode);
		this.isSeeImpostorNow = this.HasTask && this.seeImpostorTaskGage > 0;

	}

	public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance)
		=> this.killMode is not KillMode.OnMeeting or KillMode.OnMeetingTarget;

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

	private void tryKill(PlayerControl killer, byte targetPlayerId)
	{
		var target = Player.GetPlayerControlById(targetPlayerId);
		switch (
			KillButtonDoClickPatch.CheckPreKillCondition(this, killer, target))
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
