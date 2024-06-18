using System;
using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using UnityEngine;
using TMPro;



#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Tucker : SingleRoleBase, IRoleAutoBuildAbility
{
	public enum Option
	{
		UseAdmin,
		UseSecurity,
		UseVital,

		Range,
		TargetMode,
		CanReplaceAssassin,

		IsMissingToDead,
		IsDeadAbilityZero,

		LawbreakerCanKill,
		LawbreakerUseVent,
		LawbreakerUseSab,

		YardbirdAddCommonTask,
		YardbirdAddNormalTask,
		YardbirdAddLongTask,
		YardbirdSpeedMod,
		YardbirdUseAdmin,
		YardbirdUseSecurity,
		YardbirdUseVital,
		YardbirdUseVent,
		YardbirdUseSab,
	}

	public ExtremeAbilityButton? Button { get; set; }

	private bool isMissingToDead = false;
	private bool isDeadAbilityZero = true;

	private TargetMode mode;
	private bool canReplaceAssassin = false;

	private float range;
	private byte targetPlayerId = byte.MaxValue;


	public enum TargetMode
	{
		Both,
		ImpostorOnly,
		NeutralOnly,
	}

	public Tucker() : base(
		ExtremeRoleId.Jailer,
		ExtremeRoleType.Crewmate,
		ExtremeRoleId.Jailer.ToString(),
		ColorPalette.GamblerYellowGold,
		false, true, false, false, false)
	{ }

	public static void NotCrewmateToYardbird(byte rolePlayerId, byte targetPlayerId)
	{
		PlayerControl targetPlayer = Player.GetPlayerControlById(targetPlayerId);
		if (targetPlayer == null ||
			!ExtremeRoleManager.TryGetSafeCastedRole<Tucker>(rolePlayerId, out var jailer))
		{
			return;
		}
		IRoleSpecialReset.ResetRole(targetPlayerId);
		// ExtremeRoleManager.SetNewRole(targetPlayerId, yardbird);
	}

	public void CreateAbility()
	{
		this.CreateAbilityCountButton(
			"AddJail",
			Loader.GetSpriteFromResources(ExtremeRoleId.Jailer));
		this.Button?.SetLabelToCrewmate();
	}

	public bool IsAbilityUse()
	{
		this.targetPlayerId = byte.MaxValue;

		PlayerControl target = Player.GetClosestPlayerInRange(
			CachedPlayerControl.LocalPlayer, this,
			this.range);
		if (target == null) { return false; }

		this.targetPlayerId = target.PlayerId;

		return IRoleAbility.IsCommonUse();
	}

	public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
	{ }

	public void ResetOnMeetingStart()
	{ }

	public bool UseAbility()
	{
		var local = CachedPlayerControl.LocalPlayer;
		if (local == null ||
			this.Button?.Behavior is not CountBehavior count ||
			!ExtremeRoleManager.TryGetRole(this.targetPlayerId, out var role))
		{
			return false;
		}

		byte rolePlayerId = local.PlayerId;

		bool isSuccess = this.mode switch
		{
			TargetMode.Both => !role.IsCrewmate() && (this.canReplaceAssassin || role.Id != ExtremeRoleId.Assassin),
			TargetMode.ImpostorOnly => role.IsImpostor() && (this.canReplaceAssassin || role.Id != ExtremeRoleId.Assassin),
			TargetMode.NeutralOnly => role.IsNeutral(),
			_ => false,
		};

		if (isSuccess)
		{
			// 対象をヤードバード化
			using (var caller = RPCOperator.CreateCaller(
				RPCOperator.Command.ReplaceRole))
			{
				caller.WriteByte(rolePlayerId);
				caller.WriteByte(this.targetPlayerId);
				caller.WriteByte(
					(byte)ExtremeRoleManager.ReplaceOperation.ForceReplaceToYardbird);
			}
			NotCrewmateToYardbird(rolePlayerId, this.targetPlayerId);

			if (this.isDeadAbilityZero && count.AbilityCount <= 1)
			{
				selfKill(rolePlayerId);
			}
		}
		else
		{
			if (this.isMissingToDead)
			{
				selfKill(rolePlayerId);
			}
			else
			{
				// 自分自身をローブレーカー化
				using (var caller = RPCOperator.CreateCaller(
					RPCOperator.Command.ReplaceRole))
				{
					caller.WriteByte(rolePlayerId);
					caller.WriteByte(rolePlayerId);
					caller.WriteByte(
						(byte)ExtremeRoleManager.ReplaceOperation.BecomeLawbreaker);
				}
				ToLawbreaker(rolePlayerId);
			}

		}

		return true;
	}

	protected override void CreateSpecificOption(IOptionInfo parentOps)
	{
		CreateBoolOption(
			Option.UseAdmin,
			false, parentOps);
		CreateBoolOption(
			Option.UseSecurity,
			true, parentOps);
		CreateBoolOption(
			Option.UseVital,
			false, parentOps);

		this.CreateAbilityCountOption(
			parentOps, 1, 5);

		CreateSelectionOption(
			Option.TargetMode,
			Enum.GetValues<TargetMode>().Select(x => x.ToString()).ToArray(),
			parentOps);
		CreateBoolOption(
			Option.CanReplaceAssassin,
			true, parentOps);

		CreateFloatOption(
			Option.Range,
			0.75f, 0.1f, 1.5f, 0.1f,
			parentOps);

		var lowBreakerOpt = CreateBoolOption(
			Option.IsMissingToDead,
			false, parentOps);

		CreateBoolOption(
			Option.IsDeadAbilityZero,
			true, lowBreakerOpt);

		CreateBoolOption(
		   Option.LawbreakerCanKill,
		   false, lowBreakerOpt,
		   invert: true,
		   enableCheckOption: parentOps);
		CreateBoolOption(
		   Option.LawbreakerUseVent,
		   true, lowBreakerOpt,
		   invert: true,
		   enableCheckOption: parentOps);
		CreateBoolOption(
		   Option.LawbreakerUseSab,
		   true, lowBreakerOpt,
		   invert: true,
		   enableCheckOption: parentOps);


		CreateIntOption(
			Option.YardbirdAddCommonTask,
			2, 0, 15, 1,
			parentOps);
		CreateIntOption(
			Option.YardbirdAddNormalTask,
			1, 0, 15, 1,
			parentOps);
		CreateIntOption(
			Option.YardbirdAddLongTask,
			1, 0, 15, 1,
			parentOps);
		CreateFloatOption(
			Option.YardbirdSpeedMod,
			0.8f, 0.1f, 1.0f, 0.1f,
			parentOps);

		CreateBoolOption(
			Option.YardbirdUseAdmin,
			false, parentOps);
		CreateBoolOption(
			Option.YardbirdUseSecurity,
			false, parentOps);
		CreateBoolOption(
			Option.YardbirdUseVital,
			false, parentOps);
		CreateBoolOption(
			Option.YardbirdUseVent,
			true, parentOps);
		CreateBoolOption(
			Option.YardbirdUseSab,
			true, parentOps);
	}

	protected override void RoleSpecificInit()
	{
		var optMng = OptionManager.Instance;

		this.CanUseAdmin = optMng.GetValue<bool>(this.GetRoleOptionId(Option.UseAdmin));
		this.CanUseSecurity = optMng.GetValue<bool>(this.GetRoleOptionId(Option.UseSecurity));
		this.CanUseVital = optMng.GetValue<bool>(this.GetRoleOptionId(Option.UseVital));

		this.isMissingToDead = optMng.GetValue<bool>(this.GetRoleOptionId(Option.IsMissingToDead));
		if (!this.isMissingToDead)
		{
		}
		else
		{
			this.isDeadAbilityZero = optMng.GetValue<bool>(this.GetRoleOptionId(Option.IsDeadAbilityZero));
		}

		this.range = optMng.GetValue<float>(this.GetRoleOptionId(Option.Range));
		this.mode = (TargetMode)optMng.GetValue<int>(this.GetRoleOptionId(Option.TargetMode));
		this.canReplaceAssassin = optMng.GetValue<bool>(this.GetRoleOptionId(Option.CanReplaceAssassin));

	}
	private static void selfKill(byte rolePlayerId)
	{
		Player.RpcUncheckMurderPlayer(
					rolePlayerId, rolePlayerId, byte.MaxValue);
		ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
			rolePlayerId,
			Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus.MissShot);
	}
}

public sealed class Chimera : SingleRoleBase, IRoleUpdate, IRoleSpecialReset
{
	public sealed record Option(
		float KillCoolOffset,
		float ResurrectTime,
		bool Vent);

	private readonly GameData.PlayerInfo tuckerPlayer;
	private readonly float killCoolOffset;
	private readonly float resurrectTime;

	private TextMeshPro? resurrectText;
	private float resurrectTimer;
	private bool isReviveNow;

	public Chimera(
		GameData.PlayerInfo tuckerPlayer,
		Option option) : base(
		ExtremeRoleId.Yardbird,
		ExtremeRoleType.Crewmate,
		ExtremeRoleId.Yardbird.ToString(),
		ColorPalette.GamblerYellowGold,
		true, false, option.Vent, false)
	{
		this.tuckerPlayer = tuckerPlayer;
		this.killCoolOffset = option.KillCoolOffset;
		this.resurrectTime = option.ResurrectTime;
		this.resurrectTimer = this.resurrectTime;

		this.HasOtherKillCool = true;
	}

	protected override void CreateSpecificOption(IOptionInfo parentOps)
	{
		throw new Exception("Don't call this class method!!");
	}

	protected override void RoleSpecificInit()
	{
		throw new Exception("Don't call this class method!!");
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (CachedShipStatus.Instance == null ||
			GameData.Instance == null ||
			!CachedShipStatus.Instance.enabled ||
			rolePlayer == null ||
			rolePlayer.Data == null ||
			!rolePlayer.Data.IsDead ||
			MeetingHud.Instance != null ||
			ExileController.Instance != null ||
			this.tuckerPlayer == null ||
			this.tuckerPlayer.Disconnected ||
			this.tuckerPlayer.IsDead ||
			this.isReviveNow) { return; }

		if (this.resurrectText == null)
		{
			this.resurrectText = UnityEngine.Object.Instantiate(
				FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
				Camera.main.transform, false);
			this.resurrectText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
			this.resurrectText.enableWordWrapping = false;
		}

		this.resurrectText.gameObject.SetActive(true);
		this.resurrectTimer -= Time.deltaTime;
		this.resurrectText.text = string.Format(
			Translation.GetString("resurrectText"),
			Mathf.CeilToInt(this.resurrectTimer));

		if (this.resurrectTimer <= 0.0f)
		{
			revive(rolePlayer);
		}
	}

	public override bool IsBlockShowMeetingRoleInfo() => this.infoBlock();

	public override bool IsBlockShowPlayingRoleInfo() => this.infoBlock();

	private bool infoBlock()
		=> !(this.tuckerPlayer == null ||
			 this.tuckerPlayer.Disconnected ||
			 this.tuckerPlayer.IsDead);

	private void revive(PlayerControl rolePlayer)
	{
		if (rolePlayer == null) { return; }
		this.isReviveNow = true;
		this.resurrectTimer = this.resurrectTime;

		byte playerId = rolePlayer.PlayerId;

		Player.RpcUncheckRevive(playerId);

		if (rolePlayer.Data == null ||
			rolePlayer.Data.IsDead ||
			rolePlayer.Data.Disconnected) { return; }

		List<Vector2> randomPos = new List<Vector2>();
		Map.AddSpawnPoint(randomPos, playerId);
		Player.RpcUncheckSnap(playerId, randomPos[
			RandomGenerator.Instance.Next(randomPos.Count)]);

		RoleState.AddKillCoolOffset(this.killCoolOffset);
		if (this.TryGetKillCool(out float killCool))
		{
			rolePlayer.killTimer = killCool;
		}

		FastDestroyableSingleton<HudManager>.Instance.Chat.chatBubblePool.ReclaimAll();
		if (this.resurrectText != null)
		{
			this.resurrectText.gameObject.SetActive(false);
		}
		this.isReviveNow = false;
	}

	public void AllReset(PlayerControl rolePlayer)
	{
		if (CachedPlayerControl.LocalPlayer == null ||
			rolePlayer.PlayerId != CachedPlayerControl.LocalPlayer.PlayerId)
		{
			return;
		}
		// 累積していたキルクールのオフセットを無効化しておく
		RoleState.AddKillCoolOffset(0.0f);
	}
}