using System;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Queen;

public sealed class ServantRole :
	MultiAssignRoleBase,
	IRoleAutoBuildAbility,
	IRoleMurderPlayerHook
{
	public ExtremeAbilityButton? Button { get; set; }

	private byte queenPlayerId;
	private SpriteRenderer? killFlash;

	public override IOptionLoader Loader { get; }
	public override IStatusModel Status => status;
	private readonly ServantStatus status;

	public bool IsSpecialKill { get; }

	public ServantRole(
		byte queenPlayerId,
		QueenRole queen,
		SingleRoleBase baseRole) :
		base(
			ExtremeRoleId.Servant,
			ExtremeRoleType.Neutral,
			ExtremeRoleId.Servant.ToString(),
			ColorPalette.QueenWhite,
			baseRole.CanKill,
			!baseRole.IsImpostor() ? true : baseRole.HasTask,
			baseRole.UseVent,
			baseRole.UseSabotage)
	{
		Loader = queen.Loader;
		this.status = new ServantStatus(queenPlayerId, queen);
		SetControlId(queen.GameControlId);
		this.queenPlayerId = queenPlayerId;
		FakeImposter = baseRole.Team == ExtremeRoleType.Impostor;

		var id = baseRole.Id;

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
			ExtremeRoleManager.GameRole[source.PlayerId] == this) { return; }

		var hudManager = HudManager.Instance;

		if (killFlash == null)
		{
			killFlash = UnityEngine.Object.Instantiate(
				 hudManager.FullScreen,
				 hudManager.transform);
			killFlash.transform.localPosition = new Vector3(0f, 0f, 20f);
			killFlash.gameObject.SetActive(true);
		}

		Color32 color = new Color(0f, 0.8f, 0f);

		if (source.PlayerId == queenPlayerId)
		{
			color = NameColor;
		}

		killFlash.enabled = true;

		hudManager.StartCoroutine(
			Effects.Lerp(1.0f, new Action<float>((p) =>
			{
				if (killFlash == null) { return; }
				if (p < 0.5)
				{
					killFlash.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));

				}
				else
				{
					killFlash.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
				}
				if (p == 1f)
				{
					killFlash.enabled = false;
				}
			}))
		);
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
		if (killFlash != null)
		{
			killFlash.enabled = false;
		}
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

	public override bool TryRolePlayerKillTo(
		PlayerControl rolePlayer, PlayerControl targetPlayer)
	{
		if (targetPlayer.PlayerId == queenPlayerId)
		{
			if (AnotherRole?.Id == ExtremeRoleId.Sheriff)
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

		return base.TryRolePlayerKillTo(rolePlayer, targetPlayer);
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
			return Design.ColoedString(
				ColorPalette.QueenWhite,
				$" {QueenRole.RoleShowTag}");
		}

		return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		throw new Exception("Don't call this class method!!");
	}

	protected override void RoleSpecificInit()
	{
		throw new Exception("Don't call this class method!!");
	}
}
