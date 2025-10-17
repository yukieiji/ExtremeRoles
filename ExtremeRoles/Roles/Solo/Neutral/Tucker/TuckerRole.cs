using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.GameMode;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Tucker;

public sealed class TuckerRole :
	SingleRoleBase, IRoleAbility,
	IRoleSpecialReset, IRoleUpdate,
	IRoleReviveHook
{
	private sealed record RemoveInfo(int Target, Vector2 StartPos);

	public enum Option
	{
		Range,
		ShadowTimer,
		ShadowOffset,
		RemoveShadowTime,
		IsKillCoolReduceOnRemove,
		KillCoolReduceOnRemoveShadow,
		IsReduceInitKillCoolOnRemove,
		ChimeraHasOtherVision,
		ChimeraVision,
		ChimeraApplyEnvironmentVisionEffect,
		ChimeraCanUseVent,
		ChimeraReviveTime,
		ChimeraDeathKillCoolOffset,
		TuckerDeathWithChimera,
		TuckerDeathKillCoolOffset
	}

	public ExtremeAbilityButton? Button
	{
		get => internalButton;
		set
		{
			if (value is not ExtremeMultiModalAbilityButton button)
			{
				throw new ArgumentException("This role using multimodal ability");
			}
			internalButton = button;
		}
	}
	private ExtremeMultiModalAbilityButton? internalButton;

	private float range;

	private ChimeraRole.Option? option;
	private TuckerShadowSystem? system;
	private CountBehavior? createBehavior;

	private bool withDeath = false;
	private byte target;
	private int targetShadowId;
	private RemoveInfo? removeInfo;
	private readonly FullScreenFlasher flasher = new FullScreenFlasher(ColorPalette.TuckerMerdedoie);

	private HashSet<byte> chimera = new HashSet<byte>();

	public TuckerRole() : base(
		RoleCore.BuildNeutral(
			ExtremeRoleId.Tucker,
			ColorPalette.TuckerMerdedoie),
		false, false, false, false)
	{ }

	public static void TargetToChimera(byte rolePlayerId, byte targetPlayerId)
	{
		PlayerControl targetPlayer = Player.GetPlayerControlById(targetPlayerId);
		PlayerControl rolePlayer = Player.GetPlayerControlById(rolePlayerId);
		if (rolePlayer == null ||
			targetPlayer == null ||
			!ExtremeRoleManager.TryGetSafeCastedRole<TuckerRole>(rolePlayerId, out var tucker) ||
			tucker.option is null)
		{
			return;
		}
		IRoleSpecialReset.ResetRole(targetPlayerId);

		var chimera = new ChimeraRole(tucker.Loader, rolePlayer.Data, tucker.option);
		IRoleSpecialReset.ResetLover(targetPlayerId);
		ExtremeRoleManager.SetNewRole(targetPlayerId, chimera);
		chimera.SetControlId(tucker.GameControlId);

		tucker.chimera.Add(targetPlayerId);

		if (AmongUsClient.Instance.AmHost &&
			tucker.system is not null)
		{
			tucker.system.Enable(rolePlayerId);
		}
	}

	public static void RemoveChimera(byte rolePlayerId, byte targetPlayerId)
	{
		var rolePlayer = Player.GetPlayerControlById(rolePlayerId);
		if (rolePlayer == null ||
			!ExtremeRoleManager.TryGetSafeCastedRole<TuckerRole>(rolePlayerId, out var tucker) ||
			tucker.option is null)
		{
			return;
		}
		tucker.OnResetChimera(targetPlayerId, tucker.option.KillOption.KillCool);
	}

	public void CreateAbility()
	{
		var loader = Loader;

		float coolTime = loader.GetValue<RoleAbilityCommonOption, float>(
			RoleAbilityCommonOption.AbilityCoolTime);

		createBehavior = new CountBehavior(
			Tr.GetString("createChimera"),
			UnityObjectLoader.LoadFromResources(
				ExtremeRoleId.Tucker,
				ObjectPath.TuckerCreateChimera),
			isCreateChimera,
			createChimera);
		createBehavior.SetCoolTime(coolTime);
		createBehavior.SetAbilityCount(
			loader.GetValue<RoleAbilityCommonOption, int>(
				RoleAbilityCommonOption.AbilityCount));

		var summonAbility = new ReusableActivatingBehavior(
			Tr.GetString("removeShadow"),
			UnityObjectLoader.LoadFromResources(
				ExtremeRoleId.Tucker,
				ObjectPath.TuckerRemoveShadow),
			isRemoveShadow,
			startRemove,
			isRemoving,
			remove,
			() => { });
		summonAbility.SetCoolTime(coolTime);
		summonAbility.ActiveTime = loader.GetValue<Option, float>(Option.RemoveShadowTime);

		Button = new ExtremeMultiModalAbilityButton(
			new RoleButtonActivator(),
			KeyCode.F,
			createBehavior,
			summonAbility);
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		if (withDeath)
		{
			foreach (byte playerId in chimera)
			{
				PlayerControl player = Player.GetPlayerControlById(playerId);

				if (player == null ||
					player.Data.IsDead ||
					player.Data.Disconnected ||
					!ExtremeRoleManager.TryGetSafeCastedRole<ChimeraRole>(
						playerId, out _)) { continue; }

				player.Exiled();
			}
		}

		disableShadow(rolePlayer.PlayerId);
	}
	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		if (withDeath)
		{
			foreach (byte playerId in chimera)
			{
				PlayerControl player = Player.GetPlayerControlById(playerId);

				if (player == null ||
					player.Data.IsDead ||
					player.Data.Disconnected ||
					!ExtremeRoleManager.TryGetSafeCastedRole<ChimeraRole>(
						playerId, out _)) { continue; }

				RPCOperator.UncheckedMurderPlayer(
					playerId, playerId,
					byte.MaxValue);
			}
		}

		disableShadow(rolePlayer.PlayerId);
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{ }

	public void ResetOnMeetingStart()
	{
		this.flasher.Hide();
	}

	public void OnResetChimera(byte chimeraId, float killCoolTime)
	{
		chimera.Remove(chimeraId);

		if (chimera.Count == 0 &&
			createBehavior == null)
		{
			CanKill = true;
			HasOtherKillCool = false;
			KillCoolTime = killCoolTime;
		}
	}

	public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (targetRole.Core.Id is ExtremeRoleId.Chimera &&
			IsSameControlId(targetRole) &&
			chimera.Contains(targetPlayerId))
		{
			return Core.Color;
		}
		return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
	}

	public override bool IsSameTeam(SingleRoleBase targetRole)
	{
		if (isSameTuckerTeam(targetRole))
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

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(factory, 1, 10);

		factory.CreateFloatOption(
			Option.Range,
			0.7f, 0.1f, 1.2f, 0.1f);

		factory.CreateFloatOption(
			Option.ShadowTimer,
			15.0f, 0.5f, 60.0f, 0.1f,
			format: OptionUnit.Second);
		factory.CreateFloatOption(
			Option.ShadowOffset,
			0.5f, 0.0f, 2.5f, 0.1f);
		factory.CreateFloatOption(
			Option.RemoveShadowTime,
			3.0f, 0.1f, 30.0f, 0.1f,
			format: OptionUnit.Second);

		var triggerOpt = factory.CreateBoolOption(
			Option.IsKillCoolReduceOnRemove, true);
		factory.CreateFloatOption(
			Option.KillCoolReduceOnRemoveShadow,
			2.5f, 0.1f, 30.0f, 0.1f,
			triggerOpt,
			format: OptionUnit.Second,
			invert: true);
		factory.CreateBoolOption(
			Option.IsReduceInitKillCoolOnRemove,
			false, triggerOpt,
			invert: true);

		var visionOption = factory.CreateBoolOption(
			Option.ChimeraHasOtherVision,
			false);
		factory.CreateFloatOption(
			Option.ChimeraVision,
			2f, 0.25f, 5.0f, 0.25f,
			visionOption, format: OptionUnit.Multiplier);
		factory.CreateBoolOption(
			Option.ChimeraApplyEnvironmentVisionEffect,
			IsCrewmate(), visionOption);

		CreateKillerOption(factory, ignorePrefix: false);

		factory.CreateBoolOption(
			Option.ChimeraCanUseVent, false);
		factory.CreateFloatOption(
			Option.ChimeraReviveTime,
			5.0f, 4.0f, 60.0f, 0.1f,
			format: OptionUnit.Second);
		factory.CreateFloatOption(
			Option.ChimeraDeathKillCoolOffset,
			2.5f, -30.0f, 30.0f, 0.1f,
			format: OptionUnit.Second);
		var chimeraDeathOpt = factory.CreateBoolOption(
			Option.TuckerDeathWithChimera,
			false);
		factory.CreateFloatOption(
			Option.TuckerDeathKillCoolOffset,
			2.5f, -30.0f, 30.0f, 0.1f,
			chimeraDeathOpt,
			format: OptionUnit.Second,
			invert: true);
	}

	protected override void RoleSpecificInit()
	{
		var loader = Loader;

		float killCool = loader.GetValue<KillerCommonOption, bool>(KillerCommonOption.HasOtherKillCool) ?
			loader.GetValue<KillerCommonOption, float>(KillerCommonOption.KillCoolDown) :
			GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
		var killOption = new ChimeraRole.KillOption(
			killCool,
			loader.GetValue<KillerCommonOption, bool>(KillerCommonOption.HasOtherKillRange),
			loader.GetValue<KillerCommonOption, int>(KillerCommonOption.KillRange));

		var visonOption = new ChimeraRole.VisionOption(
			loader.GetValue<Option, bool>(Option.ChimeraHasOtherVision),
			loader.GetValue<Option, float>(Option.ChimeraVision),
			loader.GetValue<Option, bool>(Option.ChimeraApplyEnvironmentVisionEffect));

		withDeath = loader.GetValue<Option, bool>(Option.TuckerDeathWithChimera);

		option = new ChimeraRole.Option(
			killOption, visonOption,
			withDeath ? 0.0f : loader.GetValue<Option, float>(Option.TuckerDeathKillCoolOffset),
			loader.GetValue<Option, float>(Option.ChimeraDeathKillCoolOffset),
			loader.GetValue<Option, float>(Option.ChimeraReviveTime),
			loader.GetValue<Option, bool>(Option.ChimeraCanUseVent));

		system = ExtremeSystemTypeManager.Instance.CreateOrGet(
			TuckerShadowSystem.Type, () => new TuckerShadowSystem(
				loader.GetValue<Option, float>(Option.ShadowOffset),
				loader.GetValue<Option, float>(Option.ShadowTimer),
				loader.GetValue<Option, float>(Option.KillCoolReduceOnRemoveShadow),
				loader.GetValue<Option, bool>(Option.IsReduceInitKillCoolOnRemove)));

		range = loader.GetValue<Option, float>(Option.Range);

		removeInfo = null;
		chimera = new HashSet<byte>();
	}

	private bool isCreateChimera()
	{
		target = byte.MaxValue;

		var targetPlayer = Player.GetClosestPlayerInRange(range);
		if (targetPlayer == null)
		{
			return false;
		}
		target = targetPlayer.PlayerId;

		return IRoleAbility.IsCommonUse();
	}

	private bool createChimera()
	{
		var local = PlayerControl.LocalPlayer;
		if (createBehavior is null ||
			target == byte.MaxValue ||
			local == null)
		{
			return false;
		}

		ExtremeRoleManager.RpcReplaceRole(
			local.PlayerId, this.target,
			ExtremeRoleManager.ReplaceOperation.ForceRelaceToChimera);

		target = byte.MaxValue;

		if (createBehavior.AbilityCount <= 1 &&
			internalButton is not null)
		{
			internalButton.Remove(createBehavior);
			createBehavior = null;
		}

		return true;
	}

	private bool isRemoveShadow()
	{
		targetShadowId = int.MaxValue;
		var local = PlayerControl.LocalPlayer;

		if (local == null ||
			system is null ||
			!system.TryGetClosedShadowId(local, range, out targetShadowId))
		{
			return false;
		}
		return IRoleAbility.IsCommonUse();
	}

	private bool startRemove()
	{
		removeInfo = null;
		var local = PlayerControl.LocalPlayer;

		if (local == null)
		{
			return false;
		}
		removeInfo = new RemoveInfo(targetShadowId, local.GetTruePosition());
		return true;
	}

	private bool isRemoving()
	{
		var local = PlayerControl.LocalPlayer;
		if (removeInfo is null ||
			local == null)
		{
			return false;
		}
		return local.GetTruePosition() == removeInfo.StartPos;
	}

	private void remove()
	{
		var local = PlayerControl.LocalPlayer;
		if (removeInfo is null ||
			local == null)
		{
			return;
		}

		ExtremeSystemTypeManager.RpcUpdateSystem(
			TuckerShadowSystem.Type, x =>
			{
				x.Write((byte)TuckerShadowSystem.Ops.Remove);
				x.Write(local.PlayerId);
				x.WritePacked(removeInfo.Target);
			});

		removeInfo = null;

	}


	public void AllReset(PlayerControl rolePlayer)
	{
		disableShadow(rolePlayer.PlayerId);

		// Tuckerが消えるので関係性を解除
		var local = PlayerControl.LocalPlayer;
		if (local == null)
		{
			return;
		}
		byte localPlayerId = local.PlayerId;
		foreach (byte chimera in chimera)
		{
			if (chimera != localPlayerId ||
				!ExtremeRoleManager.TryGetSafeCastedLocalRole<ChimeraRole>(out var role))
			{
				continue;
			}
			role.RemoveTucker();
		}
		chimera.Clear();
	}

	private bool isSameTuckerTeam(SingleRoleBase targetRole)
	{
		var id = targetRole.Core.Id;
		return id == Core.Id || id is ExtremeRoleId.Chimera;
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (GameData.Instance == null ||
			option is null ||
			internalButton is null ||
			createBehavior is not null)
		{
			return;
		}
		var removed = new HashSet<byte>(chimera.Count);
		foreach (byte chimera in chimera)
		{
			var player = GameData.Instance.GetPlayerById(chimera);
			if (player == null || player.Disconnected)
			{
				removed.Add(chimera);
			}
		}
		foreach (byte chimera in removed)
		{
			ExtremeRoleManager.RpcReplaceRole(
				rolePlayer.PlayerId, chimera,
				ExtremeRoleManager.ReplaceOperation.RemoveChimera);
		}
	}

	public void HookRevive(PlayerControl revivePlayer)
	{
		byte playerId = revivePlayer.PlayerId;
		if (!(ExtremeRoleManager.TryGetRole(playerId, out var role) &&
			IsSameTeam(role) &&
			chimera.Contains(revivePlayer.PlayerId)))
		{
			return;
		}

		flasher.Flash();
	}

	private void disableShadow(byte playerId)
	{
		if (system != null)
		{
			system.Disable(playerId);
		}
	}
}