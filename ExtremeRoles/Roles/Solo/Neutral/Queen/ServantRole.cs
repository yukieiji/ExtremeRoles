using System;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Module.CustomOption.Factory.Old;
using ExtremeRoles.Module.CustomOption.Interfaces.Old;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Queen;

public sealed class ServantRole :
	MultiAssignRoleBase,
	IRoleAutoBuildAbility,
	IRoleMurderPlayerHook,
	ITryKillTo
{
	public ExtremeAbilityButton? Button { get; set; }

	private byte queenPlayerId;
	private readonly FullScreenFlasher flasher = new FullScreenFlasher(new Color(0f, 0.8f, 0f));

	public override IOptionLoader Loader { get; }
	public override IStatusModel Status => status;
	private readonly ServantStatus status;

	public bool IsSpecialKill { get; }

	public ServantRole(
		byte queenPlayerId,
		QueenRole queen,
		SingleRoleBase baseRole) :
		base(
			RoleCore.BuildNeutral(
				ExtremeRoleId.Servant,
				ColorPalette.QueenWhite),
			baseRole.CanKill,
			!baseRole.IsImpostor() ? true : baseRole.HasTask,
			baseRole.UseVent,
			baseRole.UseSabotage)
	{
		Loader = queen.Loader;
		this.status = new ServantStatus(queenPlayerId, queen, baseRole);
		SetControlId(queen.GameControlId);
		this.queenPlayerId = queenPlayerId;

		var core = baseRole.Core;
		var id = core.Id;

		IsSpecialKill = id is
			ExtremeRoleId.Fencer or ExtremeRoleId.Sheriff;

		CanKill = id switch
		{
			ExtremeRoleId.Fencer => false,
			ExtremeRoleId.Yandere or ExtremeRoleId.Hero => true,
			_ => CanKill,
		};

		if (baseRole.IsImpostor())
		{
			HasOtherVision = true;
		}
		else
		{
			HasOtherVision = baseRole.HasOtherVision;
		}
		Vision = baseRole.Vision;
		IsApplyEnvironmentVision = baseRole.IsApplyEnvironmentVision;

		HasOtherKillCool = baseRole.HasOtherKillCool;
		KillCoolTime = baseRole.KillCoolTime;
		HasOtherKillRange = baseRole.HasOtherKillRange;
		KillRange = baseRole.KillRange;
	}

	public void SelfKillAbility(float coolTime)
	{
		Button = RoleAbilityFactory.CreateReusableAbility(
			"selfKill",
			UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.SucideSprite),
			IsAbilityUse,
			UseAbility);
		Button.Behavior.SetCoolTime(coolTime);
		Button.OnMeetingEnd();
	}

	public void HookMuderPlayer(
		PlayerControl source, PlayerControl target)
	{

		if (MeetingHud.Instance ||
			source.PlayerId == target.PlayerId ||
			ExtremeRoleManager.GameRole[source.PlayerId] == this)
		{
			return;
		}

		Color? flashColor = source.PlayerId == queenPlayerId ? 
			this.Core.Color : null;
		flasher.Flash(flashColor);
	}

	public void CreateAbility()
	{
		throw new Exception("Don't call this class method!!");
	}

	public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		return;
	}

	public void ResetOnMeetingStart()
	{
		this.flasher.Hide();
	}

	public bool UseAbility()
	{
		byte playerId = PlayerControl.LocalPlayer.PlayerId;
		Player.RpcUncheckMurderPlayer(
			playerId, playerId, byte.MaxValue);

		return true;
	}

	public override void OverrideAnotherRoleSetting()
	{
		var queenPlayer = GameData.Instance.GetPlayerById(this.status.Parent);

		if (AnotherRole is Resurrecter resurrecter &&
			(queenPlayer == null || queenPlayer.IsDead || queenPlayer.Disconnected))
		{
			Resurrecter.UseResurrect(resurrecter);
		}
	}

	public bool TryRolePlayerKillTo(
		PlayerControl rolePlayer, PlayerControl targetPlayer)
	{
		if (targetPlayer.PlayerId != queenPlayerId)
		{
			return true;
		}

		if (AnotherRole?.Core.Id is ExtremeRoleId.Sheriff)
		{
			Player.RpcUncheckMurderPlayer(
				rolePlayer.PlayerId,
				rolePlayer.PlayerId,
				byte.MaxValue);

			ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
				rolePlayer.PlayerId, ExtremeShipStatus.PlayerStatus.MissShot);
		}
		return false;
	}

	public override string GetFullDescription()
	{
		var queen = Player.GetPlayerControlById(queenPlayerId);
		string fullDesc = base.GetFullDescription();

		if (queen == null ||
			queen.Data == null)
		{
			return fullDesc;
		}

		return string.Format(
			fullDesc, queen.Data.PlayerName);
	}

	public override Color GetTargetRoleSeeColor(
		SingleRoleBase targetRole,
		byte targetPlayerId)
	{

		if (targetPlayerId == queenPlayerId)
		{
			return ColorPalette.QueenWhite;
		}
		return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
	}

	public override string GetRoleTag() => QueenRole.RoleShowTag;

	public override string GetRolePlayerNameTag(
		SingleRoleBase targetRole, byte targetPlayerId)
	{

		if (targetPlayerId == queenPlayerId)
		{
			return Design.ColoredString(
				ColorPalette.QueenWhite,
				$" {QueenRole.RoleShowTag}");
		}

		return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
	}

	protected override void CreateSpecificOption(
		OldAutoParentSetOptionCategoryFactory factory)
	{
		throw new Exception("Don't call this class method!!");
	}

	protected override void RoleSpecificInit()
	{
		throw new Exception("Don't call this class method!!");
	}
}
