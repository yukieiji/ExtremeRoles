using System;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.Tucker;

#nullable enable

public sealed class ChimeraRole : SingleRoleBase, IRoleUpdate, IRoleSpecialReset
{
	public sealed record Option(
		KillOption KillOption,
		VisionOption VisionOption,
		float TukerKillCoolOffset,
		float RevieKillCoolOffset,
		float ResurrectTime,
		bool Vent);
	public sealed record KillOption(float KillCool, bool OtherRange, int Range);
	public sealed record VisionOption(bool OtherVision, float Vision, bool ApplyEffect);

	private NetworkedPlayerInfo? tuckerPlayer;
	private readonly float reviveKillCoolOffset;
	private readonly float resurrectTime;
	private readonly float tuckerDeathKillCoolOffset;
	private readonly float initCoolTime;

	private TextMeshPro? resurrectText;
	private float resurrectTimer;
	private bool isReviveNow;
	private bool isTuckerDead;

	public byte Parent { get; }
	public override IOptionLoader Loader { get; }
	public override IStatusModel Status => status;
	private readonly ChimeraStatus status;

	public ChimeraRole(
		IOptionLoader loader,
		NetworkedPlayerInfo tuckerPlayer,
		Option option) : base(
			RoleCore.BuildNeutral(
				ExtremeRoleId.Chimera,
				ColorPalette.TuckerMerdedoie),
			true, false, option.Vent, false)
	{
		Loader = loader;
		this.status = new ChimeraStatus(tuckerPlayer, this);

		this.tuckerPlayer = tuckerPlayer;
		reviveKillCoolOffset = option.RevieKillCoolOffset;
		tuckerDeathKillCoolOffset = option.TukerKillCoolOffset;
		resurrectTime = option.ResurrectTime;
		resurrectTimer = resurrectTime;

		var killOption = option.KillOption;
		HasOtherKillCool = true;
		initCoolTime = killOption.KillCool;
		KillCoolTime = initCoolTime;
		HasOtherKillRange = killOption.OtherRange;
		KillRange = killOption.Range;

		var vision = option.VisionOption;
		HasOtherVision = vision.OtherVision;
		Vision = vision.Vision;
		IsApplyEnvironmentVision = vision.ApplyEffect;

		isTuckerDead = tuckerPlayer.IsDead;
		isReviveNow = false;
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		throw new Exception("Don't call this class method!!");
	}

	protected override void RoleSpecificInit()
	{
		throw new Exception("Don't call this class method!!");
	}

	public void RemoveTucker()
	{
		tuckerPlayer = null;
	}

	public void OnRemoveShadow(byte tuckerPlayerId,
		float reduceTime, bool isReduceInitKillCool)
	{
		if (tuckerPlayer == null ||
			tuckerPlayerId != tuckerPlayer.PlayerId)
		{
			return;
		}

		float min = isReduceInitKillCool ? 0.01f : initCoolTime;
		updateKillCoolTime(-reduceTime, min);
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (!GameProgressSystem.IsTaskPhase)
		{
			if (resurrectText != null)
			{
				resurrectText.gameObject.SetActive(false);
			}
			return;
		}

		if (!isTuckerDead)
		{
			isTuckerDead = tuckerPlayer == null || tuckerPlayer.IsDead;
			if (isTuckerDead)
			{
				updateKillCoolTime(tuckerDeathKillCoolOffset);
			}
		}


		// 復活処理
		if (tuckerPlayer == null ||
			!rolePlayer.Data.IsDead ||
			tuckerPlayer.Disconnected ||
			tuckerPlayer.IsDead ||
			isReviveNow)
		{
			if (resurrectText != null)
			{
				resurrectText.gameObject.SetActive(false);
			}
			return;
		}


		if (resurrectText == null)
		{
			resurrectText = UnityEngine.Object.Instantiate(
				HudManager.Instance.KillButton.cooldownTimerText,
				Camera.main.transform, false);
			resurrectText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
			resurrectText.enableWordWrapping = false;
		}

		resurrectText.gameObject.SetActive(true);
		resurrectTimer -= Time.deltaTime;
		resurrectText.text = string.Format(
			Tr.GetString("resurrectText"),
			Mathf.CeilToInt(resurrectTimer));

		if (resurrectTimer <= 0.0f)
		{
			revive(rolePlayer);
		}
	}

	public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (tuckerPlayer != null &&
			targetRole.Core.Id is ExtremeRoleId.Tucker &&
			targetPlayerId == tuckerPlayer.PlayerId)
		{
			return Core.Color;
		}
		return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
	}

	public override bool IsSameTeam(SingleRoleBase targetRole)
	{
		if (isSameChimeraTeam(targetRole))
		{
			if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
			{
				return true;
			}
			else
			{
				return IsSameControlId(targetRole);
			}
		}
		else
		{
			return base.IsSameTeam(targetRole);
		}
	}

	public override bool IsBlockShowMeetingRoleInfo() => infoBlock();

	public override bool IsBlockShowPlayingRoleInfo() => infoBlock();

	public override string GetFullDescription()
	{
		string full = base.GetFullDescription();
		if (tuckerPlayer == null)
		{
			return full;
		}
		return string.Format(
			full, tuckerPlayer.DefaultOutfit.PlayerName);
	}

	private bool infoBlock()
		=> !isTuckerDead;

	private void revive(PlayerControl rolePlayer)
	{
		if (rolePlayer == null || tuckerPlayer == null) { return; }
		isReviveNow = true;
		resurrectTimer = resurrectTime;

		byte playerId = rolePlayer.PlayerId;

		updateKillCoolTime(reviveKillCoolOffset);
		Player.RpcUncheckRevive(playerId);

		if (rolePlayer.Data == null ||
			rolePlayer.Data.IsDead ||
			rolePlayer.Data.Disconnected) { return; }

		List<Vector2> randomPos = new List<Vector2>();
		Map.AddSpawnPoint(randomPos, playerId);
		Player.RpcUncheckSnap(playerId, randomPos[
			RandomGenerator.Instance.Next(randomPos.Count)]);

		rolePlayer.killTimer = KillCoolTime;

		HudManager.Instance.Chat.chatBubblePool.ReclaimAll();
		if (resurrectText != null)
		{
			resurrectText.gameObject.SetActive(false);
		}

		ExtremeSystemTypeManager.RpcUpdateSystem(
			TuckerShadowSystem.Type, x =>
			{
				x.Write((byte)TuckerShadowSystem.Ops.ChimeraRevive);
				x.Write(tuckerPlayer.PlayerId);
			});

		isReviveNow = false;
	}

	public void AllReset(PlayerControl rolePlayer)
	{
		if (PlayerControl.LocalPlayer == null ||
			rolePlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId)
		{
			return;
		}
		// 累積していたキルクールのオフセットを無効化しておく
		KillCoolTime = initCoolTime;
	}

	private void updateKillCoolTime(float offset, float min = 0.1f)
	{
		KillCoolTime = Mathf.Clamp(KillCoolTime + offset, min, float.MaxValue);
	}

	private bool isSameChimeraTeam(SingleRoleBase targetRole)
	{
		var id = targetRole.Core.Id;
		return id == Core.Id || id is ExtremeRoleId.Tucker;
	}
}
