using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Tucker : SingleRoleBase, IRoleAutoBuildAbility
{
	public enum Option
	{
		Range,
		ShadowTimer,
		ShadowOffset,
		RemoveShadowTime,
		KillCoolReduceOnRemoveShadow,
		IsReduceInitKillCoolOnRemove,
		ChimeraCanUseVent,
		ChimeraReviveTime,
		ChimeraDeathKillCoolOffset,
		TuckerDeathKillCoolOffset,
	}

	public ExtremeAbilityButton? Button { get; set; }
	private float range;
	private Chimera.Option? option;
	private TuckerShadowSystem? system;

	public Tucker() : base(
		ExtremeRoleId.Tucker,
		ExtremeRoleType.Crewmate,
		ExtremeRoleId.Tucker.ToString(),
		ColorPalette.GamblerYellowGold,
		false, true, false, false, false)
	{ }

	public static void TargetToChimera(byte rolePlayerId, byte targetPlayerId)
	{
		PlayerControl targetPlayer = Player.GetPlayerControlById(targetPlayerId);
		if (targetPlayer == null ||
			!ExtremeRoleManager.TryGetSafeCastedRole<Tucker>(rolePlayerId, out var tucker) ||
			tucker.option is null)
		{
			return;
		}
		IRoleSpecialReset.ResetRole(targetPlayerId);

		var chimera = new Chimera(targetPlayer.Data, tucker.option);
		ExtremeRoleManager.SetNewRole(targetPlayerId, chimera);

		if (AmongUsClient.Instance.AmHost &&
			tucker.system is not null)
		{
			tucker.system.Enable(rolePlayerId);
		}
	}

	public void CreateAbility()
	{
		this.CreateAbilityCountButton(
			"AddJail",
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Jailer));
		this.Button?.SetLabelToCrewmate();
	}

	public bool IsAbilityUse()
	{

		return IRoleAbility.IsCommonUse();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{ }

	public void ResetOnMeetingStart()
	{ }

	public bool UseAbility()
	{
		return true;
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(factory, 1, 10);

		factory.CreateFloatOption(
			Option.Range,
			0.75f, 0.1f, 1.2f, 0.1f);

		factory.CreateFloatOption(
			Option.ShadowTimer,
			15.0f, 0.5f, 60.0f, 0.1f);
		factory.CreateFloatOption(
			Option.ShadowOffset,
			0.5f, 0.0f, 2.5f, 0.1f);
		factory.CreateFloatOption(
			Option.RemoveShadowTime,
			3.0f, 0.1f, 15.0f, 0.1f);
		factory.CreateFloatOption(
			Option.KillCoolReduceOnRemoveShadow,
			2.5f, 0.1f, 30.0f, 0.1f);
		factory.CreateBoolOption(
			Option.IsReduceInitKillCoolOnRemove, false);

		CreateKillerOption(factory, ignorePrefix: false);

		factory.CreateBoolOption(
			Option.ChimeraCanUseVent, false);
		factory.CreateFloatOption(
			Option.ChimeraReviveTime,
			5.0f, 4.0f, 10.0f, 0.1f,
			format: OptionUnit.Second);
		factory.CreateFloatOption(
			Option.ChimeraDeathKillCoolOffset,
			2.5f, -30.0f, 30.0f, 0.1f,
			format: OptionUnit.Second);
		factory.CreateFloatOption(
			Option.TuckerDeathKillCoolOffset,
			2.5f, -30.0f, 30.0f, 0.1f,
			format: OptionUnit.Second);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;

		float killCool = loader.GetValue<KillerCommonOption, bool>(KillerCommonOption.HasOtherKillCool) ?
			loader.GetValue<KillerCommonOption, float>(KillerCommonOption.KillCoolDown) :
			GameManager.Instance.LogicOptions.GetKillCooldown();

		this.option = new Chimera.Option(
			killCool,
			loader.GetValue<KillerCommonOption, bool>(KillerCommonOption.HasOtherKillRange),
			loader.GetValue<KillerCommonOption, int>(KillerCommonOption.KillRange),
			loader.GetValue<Option, float>(Option.TuckerDeathKillCoolOffset),
			loader.GetValue<Option, float>(Option.ChimeraDeathKillCoolOffset),
			loader.GetValue<Option, float>(Option.ChimeraReviveTime),
			loader.GetValue<Option, bool>(Option.ChimeraCanUseVent));

		this.system = ExtremeSystemTypeManager.Instance.CreateOrGet(
			TuckerShadowSystem.Type, () => new TuckerShadowSystem(
				loader.GetValue<Option, float>(Option.ShadowOffset),
				loader.GetValue<Option, float>(Option.ShadowTimer),
				loader.GetValue<Option, float>(Option.KillCoolReduceOnRemoveShadow),
				loader.GetValue<Option, bool>(Option.IsReduceInitKillCoolOnRemove)));
	}
}

public sealed class Chimera : SingleRoleBase, IRoleUpdate, IRoleSpecialReset
{
	public sealed record Option(
		float KillCool,
		bool HasOtherRange,
		int Range,
		float TukerKillCoolOffset,
		float RevieKillCoolOffset,
		float ResurrectTime,
		bool Vent);

	private readonly NetworkedPlayerInfo tuckerPlayer;
	private readonly float reviveKillCoolOffset;
	private readonly float resurrectTime;
	private readonly float tuckerDeathKillCoolOffset;
	private readonly float initCoolTime;

	private TextMeshPro? resurrectText;
	private float resurrectTimer;
	private bool isReviveNow;
	private bool isTuckerDead;

	public Chimera(
		NetworkedPlayerInfo tuckerPlayer,
		Option option) : base(
		ExtremeRoleId.Yardbird,
		ExtremeRoleType.Crewmate,
		ExtremeRoleId.Yardbird.ToString(),
		ColorPalette.GamblerYellowGold,
		true, false, option.Vent, false)
	{
		this.tuckerPlayer = tuckerPlayer;
		this.reviveKillCoolOffset = option.RevieKillCoolOffset;
		this.tuckerDeathKillCoolOffset = option.TukerKillCoolOffset;
		this.resurrectTime = option.ResurrectTime;
		this.resurrectTimer = this.resurrectTime;

		this.HasOtherKillCool = true;
		this.initCoolTime = option.KillCool;
		this.KillCoolTime = this.initCoolTime;
		this.isTuckerDead = tuckerPlayer.IsDead;
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		throw new Exception("Don't call this class method!!");
	}

	protected override void RoleSpecificInit()
	{
		throw new Exception("Don't call this class method!!");
	}

	public void OnRemoveShadow(byte tuckerPlayerId,
		float reduceTime, bool isReduceInitKillCool)
	{
		if (tuckerPlayerId != this.tuckerPlayer.PlayerId)
		{
			return;
		}

		float min = isReduceInitKillCool ? 0.01f : this.initCoolTime;
		updateKillCoolTime(-reduceTime, min);
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (CachedShipStatus.Instance == null ||
			GameData.Instance == null ||
			!CachedShipStatus.Instance.enabled ||
			rolePlayer == null ||
			rolePlayer.Data == null ||
			MeetingHud.Instance != null ||
			ExileController.Instance != null ||
			this.tuckerPlayer == null)
		{
			return;
		}

		if (!this.isTuckerDead)
		{
			this.isTuckerDead = this.tuckerPlayer.IsDead;
			if (this.isTuckerDead)
			{
				updateKillCoolTime(this.KillCoolTime, this.tuckerDeathKillCoolOffset);
			}
		}


		// 復活処理
		if (!rolePlayer.Data.IsDead ||
			this.tuckerPlayer.Disconnected ||
			this.tuckerPlayer.IsDead ||
			this.isReviveNow)
		{
			return;
		}


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

		updateKillCoolTime(this.KillCoolTime, this.reviveKillCoolOffset);
		Player.RpcUncheckRevive(playerId);

		if (rolePlayer.Data == null ||
			rolePlayer.Data.IsDead ||
			rolePlayer.Data.Disconnected) { return; }

		List<Vector2> randomPos = new List<Vector2>();
		Map.AddSpawnPoint(randomPos, playerId);
		Player.RpcUncheckSnap(playerId, randomPos[
			RandomGenerator.Instance.Next(randomPos.Count)]);

		rolePlayer.killTimer = this.KillCoolTime;

		FastDestroyableSingleton<HudManager>.Instance.Chat.chatBubblePool.ReclaimAll();
		if (this.resurrectText != null)
		{
			this.resurrectText.gameObject.SetActive(false);
		}

		ExtremeSystemTypeManager.RpcUpdateSystem(
			TuckerShadowSystem.Type, x =>
			{
				x.Write((byte)TuckerShadowSystem.Ops.ChimeraRevive);
				x.Write(this.tuckerPlayer.PlayerId);
			});

		this.isReviveNow = false;
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

	private void updateKillCoolTime(float offset, float min=0.1f)
	{
		this.KillCoolTime = Mathf.Clamp(this.KillCoolTime + offset, min, float.MaxValue);
	}
}