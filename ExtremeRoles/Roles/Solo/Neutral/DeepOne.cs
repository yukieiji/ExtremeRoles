using System;
using System.Collections.Generic;
using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class DeepOneAbilityHandler(DeepOne role) : IAbility, IInvincible
{
	private readonly DeepOne role = role;

	public bool IsBlockKillFrom(byte? fromPlayer)
	{
		return this.role.IsBlockKill;
	}

	public bool IsValidKillFromSource(byte source)
	{
		return !IsBlockKillFrom(source);
	}

	public bool IsValidAbilitySource(byte source)
	{
		return !IsBlockKillFrom(source);
	}
}

public sealed class DeepOne :
	SingleRoleBase,
	IRoleAutoBuildAbility,
	IRoleUpdate,
	IRoleVoteModifier,
	IRoleSpecialReset
{
	public enum DeepOneOption
	{
		RequiredClicks,
		RequiredWinFrogs,
		DeathCooldownReduction,
		EnableKillBlock,
		EnableExileBlock,
		EnableVentUnlock,
		EnableMaxSpeed,
		EnableClickMultiplier,
	}

	public int Order => 100;

	public ExtremeAbilityButton? Button { get; set; }

	public bool IsBlockKill { get; set; } = false;
	public bool IsBlockExile { get; set; } = false;

	public DeepOneSystem System { get; private set; }

	private readonly DeepOneAbilityHandler abilityHandler;

	private float baseCooldown = 15.0f;
	private float deathCooldownReduction = 0.0f;

	public DeepOne() : base(
		RoleArgs.BuildNeutral(
			ExtremeRoleId.DeepOne,
			ColorPalette.DeepOneDarkGreen,
			RolePropPresets.OptionalDefault))
	{
		this.System = new DeepOneSystem(5, 5, true, true, true, true, true);
		this.abilityHandler = new DeepOneAbilityHandler(this);
		this.AbilityClass = this.abilityHandler;
	}

	public void CreateAbility()
	{
		this.CreateNormalActivatingAbilityButton(
			"sacrifice",
			UnityObjectLoader.LoadSpriteFromResources(ObjectPath.TestButton),
			abilityOff: CleanUp,
			forceAbilityOff: () => { });
	}

	public bool IsAbilityUse()
	{
		return IRoleAbility.IsCommonUse();
	}

	public bool UseAbility()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer.IsInValid() ||
			localPlayer.inVent ||
			localPlayer.inMovingPlat ||
			localPlayer.onLadder)
		{
			return false;
		}

		Vector2 pos = localPlayer.GetTruePosition();
		this.System.PlaceFrogLocally(pos);
		return true;
	}

	public void CleanUp()
	{
	}

	public void AllReset(PlayerControl rolePlayer)
	{
		this.IsWin = false;
		this.IsBlockKill = false;
		this.IsBlockExile = false;
		this.MoveSpeed = 1.0f;
		this.UseVent = false;
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (rolePlayer == null || rolePlayer.Data == null)
		{
			return;
		}

		int deadCount = 0;
		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (player != null && (player.IsDead || player.Disconnected))
			{
				deadCount++;
			}
		}

		float reduction = deadCount * this.deathCooldownReduction;
		float calculatedCooldown = Math.Max(0.5f, this.baseCooldown * (1.0f - reduction));

		if (this.Button != null && this.Button.Behavior != null)
		{
			this.Button.Behavior.SetCoolTime(calculatedCooldown);
		}
	}

	public void ModifiedVote(byte rolePlayerId, ref Dictionary<byte, byte> voteTarget, ref Dictionary<byte, int> voteResult)
	{
		if (!this.IsBlockExile || voteResult.Count <= 0 || OnemanMeetingSystemManager.IsActive)
		{
			return;
		}
		voteResult.Remove(rolePlayerId);
	}

	public IEnumerable<VoteInfo> GetModdedVoteInfo(VoteInfoCollector collector, NetworkedPlayerInfo rolePlayer)
	{
		if (!this.IsBlockExile || OnemanMeetingSystemManager.IsActive)
		{
			yield break;
		}

		foreach (var info in collector.Vote)
		{
			if (info.TargetId == rolePlayer.PlayerId && info.Count > 0)
			{
				yield return new VoteInfo(info.VoterId, info.TargetId, -info.Count);
			}
		}
	}

	public void ResetModifier()
	{
	}

	public void ResetOnMeetingStart()
	{
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public override bool IsSameTeam(SingleRoleBase targetRole)
	{
		return this.IsNeutralSameTeam(targetRole);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateCommonAbilityOption(factory, 15.0f);

		factory.CreateIntOption(
			DeepOneOption.RequiredClicks,
			5, 1, 10, 1);

		factory.CreateIntOption(
			DeepOneOption.RequiredWinFrogs,
			5, 1, 20, 1);

		factory.CreateFloatOption(
			DeepOneOption.DeathCooldownReduction,
			1.0f, 0.0f, 10.0f, 0.5f,
			format: OptionUnit.Percentage);

		factory.CreateBoolOption(
			DeepOneOption.EnableKillBlock,
			true);

		factory.CreateBoolOption(
			DeepOneOption.EnableExileBlock,
			true);

		factory.CreateBoolOption(
			DeepOneOption.EnableVentUnlock,
			true);

		factory.CreateBoolOption(
			DeepOneOption.EnableMaxSpeed,
			true);

		factory.CreateBoolOption(
			DeepOneOption.EnableClickMultiplier,
			true);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;

		this.baseCooldown = loader.GetValue<RoleAbilityCommonOption, float>(
			RoleAbilityCommonOption.AbilityCoolTime);
		this.deathCooldownReduction = loader.GetValue<DeepOneOption, float>(
			DeepOneOption.DeathCooldownReduction) / 100.0f;

		bool enableKillBlock = loader.GetValue<DeepOneOption, bool>(
			DeepOneOption.EnableKillBlock);
		bool enableExileBlock = loader.GetValue<DeepOneOption, bool>(
			DeepOneOption.EnableExileBlock);
		bool enableVentUnlock = loader.GetValue<DeepOneOption, bool>(
			DeepOneOption.EnableVentUnlock);
		bool enableMaxSpeed = loader.GetValue<DeepOneOption, bool>(
			DeepOneOption.EnableMaxSpeed);

		this.System = ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.DeepOneSystem,
			() => new DeepOneSystem(
				loader.GetValue<DeepOneOption, int>(DeepOneOption.RequiredClicks),
				loader.GetValue<DeepOneOption, int>(DeepOneOption.RequiredWinFrogs),
				loader.GetValue<DeepOneOption, bool>(DeepOneOption.EnableClickMultiplier),
				enableKillBlock,
				enableExileBlock,
				enableVentUnlock,
				enableMaxSpeed));

		this.System.UpdateOptions(
			loader.GetValue<DeepOneOption, int>(DeepOneOption.RequiredClicks),
			loader.GetValue<DeepOneOption, int>(DeepOneOption.RequiredWinFrogs),
			loader.GetValue<DeepOneOption, bool>(DeepOneOption.EnableClickMultiplier),
			enableKillBlock,
			enableExileBlock,
			enableVentUnlock,
			enableMaxSpeed);

		this.UseVent = false;
		this.IsBlockKill = false;
		this.IsBlockExile = false;
		this.MoveSpeed = 1.0f;
		this.IsWin = false;
	}
}
