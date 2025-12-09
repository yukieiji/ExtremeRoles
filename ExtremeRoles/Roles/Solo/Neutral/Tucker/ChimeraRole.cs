using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.GameMode;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;

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
	private readonly float tuckerDeathKillCoolOffset;
	private readonly float initCoolTime;

	private bool isTuckerDead;

	public byte Parent { get; }
	public override IOptionLoader Loader { get; }
	public override IStatusModel Status => status;
	private readonly ChimeraStatus status;
    private readonly PlayerReviver playerReviver;

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
        playerReviver = new PlayerReviver(option.ResurrectTime);
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
        playerReviver.Update();

		if (!GameProgressSystem.IsTaskPhase)
		{
            playerReviver.Reset();
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
            playerReviver.IsReviving)
		{
			return;
		}

        playerReviver.Start(rolePlayer, () => revive(rolePlayer));
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
		if (tuckerPlayer == null) { return; }

		updateKillCoolTime(reviveKillCoolOffset);
		rolePlayer.killTimer = KillCoolTime;

		ExtremeSystemTypeManager.RpcUpdateSystem(
			TuckerShadowSystem.Type, x =>
			{
				x.Write((byte)TuckerShadowSystem.Ops.ChimeraRevive);
				x.Write(tuckerPlayer.PlayerId);
			});
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
