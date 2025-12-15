using System;

using UnityEngine;

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
		this.Loader = loader;
		this.status = new ChimeraStatus(tuckerPlayer, this);

		this.tuckerPlayer = tuckerPlayer;
		this.reviveKillCoolOffset = option.RevieKillCoolOffset;
		this.tuckerDeathKillCoolOffset = option.TukerKillCoolOffset;

		var killOption = option.KillOption;
		this.HasOtherKillCool = true;
		this.initCoolTime = killOption.KillCool;
		this.KillCoolTime = initCoolTime;
		this.HasOtherKillRange = killOption.OtherRange;
		this.KillRange = killOption.Range;

		var vision = option.VisionOption;
		this.HasOtherVision = vision.OtherVision;
		this.Vision = vision.Vision;
		this.IsApplyEnvironmentVision = vision.ApplyEffect;

		this.isTuckerDead = tuckerPlayer.IsDead;
		this.playerReviver = new PlayerReviver(option.ResurrectTime, revive);
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
			this.playerReviver.Reset();
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

		this.playerReviver.Update();

		// 復活処理
		if (tuckerPlayer == null ||
			!rolePlayer.Data.IsDead ||
			tuckerPlayer.Disconnected ||
			tuckerPlayer.IsDead ||
			this.playerReviver.IsReviving)
		{
			return;
		}

		this.playerReviver.Start(rolePlayer);
	}

	public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (this.tuckerPlayer != null &&
			targetRole.Core.Id is ExtremeRoleId.Tucker &&
			targetPlayerId == this.tuckerPlayer.PlayerId)
		{
			return this.Core.Color;
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
		if (this.tuckerPlayer == null)
		{
			return full;
		}
		return string.Format(
			full, this.tuckerPlayer.DefaultOutfit.PlayerName);
	}

	private bool infoBlock()
		=> !this.isTuckerDead;

	private void revive(PlayerControl rolePlayer)
	{
		if (this.tuckerPlayer == null)
		{
			return;
		}

		updateKillCoolTime(reviveKillCoolOffset);
		rolePlayer.killTimer = KillCoolTime;

		ExtremeSystemTypeManager.RpcUpdateSystem(
			TuckerShadowSystem.Type, x =>
			{
				x.Write((byte)TuckerShadowSystem.Ops.ChimeraRevive);
				x.Write(this.tuckerPlayer.PlayerId);
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
		this.KillCoolTime = this.initCoolTime;
	}

	private void updateKillCoolTime(float offset, float min = 0.1f)
	{
		this.KillCoolTime = Mathf.Clamp(this.KillCoolTime + offset, min, float.MaxValue);
	}

	private bool isSameChimeraTeam(SingleRoleBase targetRole)
	{
		var id = targetRole.Core.Id;
		return id == Core.Id || id is ExtremeRoleId.Tucker;
	}
}
