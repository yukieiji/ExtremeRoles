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
		OnExiled,
		OnMeeting,
	}

	public ExtremeAbilityButton Button { get; set; }

	private bool canKillImpostor;
	private KillMode killMode;
	private float range;
	private byte target;
	private byte meetingTarget;

	private bool isSeeImpostorNow = false;
	private float seeImpostorTaskGage;

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
		}
		// プレイヤーを殺す
	}

	public void ResetOnMeetingStart()
	{ }

	public void Update(PlayerControl rolePlayer)
	{
		if (!this.HasTask) { return; }

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
			case KillMode.OnExiled:
				return false;
			case KillMode.OnTaskPhase:
				var killer = PlayerControl.LocalPlayer;
				if (killer == null ||
					!tryKill(killer, this.target))
				{
					return false;
				}
				Player.RpcUncheckMurderPlayer(
					killer.PlayerId, killer.PlayerId,
					byte.MinValue);
				break;
			case KillMode.OnTaskPhaseTarget:
				this.meetingTarget = this.target;
				break;
			default:
				break;
		}
		return true;
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
	}

	private bool tryKill(PlayerControl killer, byte targetPlayerId)
	{
		var target = Player.GetPlayerControlById(targetPlayerId);

		if (killer == null ||
			!KillButtonDoClickPatch.CheckPreKillConditionWithBool(this, killer, target))
		{
			return false;
		}

		Player.RpcUncheckMurderPlayer(
			killer.PlayerId, target.PlayerId,
			byte.MinValue);
		return true;
	}

	public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance)
		=> this.killMode is KillMode.OnMeeting;

	public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
		=> IRoleMeetingButtonAbility.DefaultButtonMod(instance, abilityButton, "hereticKillTarget");

	public Action CreateAbilityAction(PlayerVoteArea instance)
		=> () =>
		{
			this.target = instance.TargetPlayerId;
		};

	public void SetSprite(SpriteRenderer render)
	{
		throw new NotImplementedException();
	}
}
