using System;
using System.Collections.Generic;
using UnityEngine;

using AmongUs.GameOptions;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
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
using ExtremeRoles.Roles.API.Interface.Status;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class DeepOneAbilityHandler(
	DeepOneFrogsControlSystem system,
	DeepOneStatus status) : IAbility, IInvincible
{
	private readonly DeepOneStatus status = status;
	private readonly DeepOneFrogsControlSystem system = system;

	public bool UseAbility()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer.IsInValid())
		{
			return false;
		}

		Vector2 pos = localPlayer.GetTruePosition();
		this.system.PlaceFrogLocally(pos);
		return true;
	}

	public bool IsBlockKillFrom(byte? fromPlayer)
	{
		return this.status.IsBlockKill;
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

public sealed class DeepOneStatus(
	bool enableKillBlock,
	bool enableExileBlock,
	bool enableVentUnlock,
	bool enableMaxSpeed
) : IStatusModel
{
	public bool IsBlockKill => this.enableKillBlock && this.currentStatus >= DeepOne.FrogsStatus.BlockKill;
	public bool IsBlockExile => this.enableExileBlock && this.currentStatus >= DeepOne.FrogsStatus.BlockExile;
	public bool UseVent => this.enableVentUnlock && this.currentStatus >= DeepOne.FrogsStatus.UseVent;
	public bool IsSpeedUp => this.enableMaxSpeed && this.currentStatus >= DeepOne.FrogsStatus.SpeedUp;

	private readonly bool enableKillBlock = enableKillBlock;
	private readonly bool enableExileBlock = enableExileBlock;
	private readonly bool enableVentUnlock = enableVentUnlock;
	private readonly bool enableMaxSpeed = enableMaxSpeed;

	private DeepOne.FrogsStatus currentStatus = DeepOne.FrogsStatus.None;
	private int prevFrogCount = 0;

	public void ResetStatus()
	{
		this.currentStatus = DeepOne.FrogsStatus.None;
	}

	public bool UpdateStatus(int frogCount)
	{
		int prevNum = this.prevFrogCount;
		this.prevFrogCount = frogCount;

		this.currentStatus = frogCount switch
		{
			>= 5 => DeepOne.FrogsStatus.Multiplier,
			>= 4 => DeepOne.FrogsStatus.SpeedUp,
			>= 3 => DeepOne.FrogsStatus.UseVent,
			>= 2 => DeepOne.FrogsStatus.BlockExile,
			>= 1 => DeepOne.FrogsStatus.BlockKill,
			_ => DeepOne.FrogsStatus.None
		};
		return prevNum > frogCount;
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
		FrogsRemovePenalty,
		DeathCooldownReduction,
		EnableKillBlock,
		EnableExileBlock,
		EnableVentUnlock,
		EnableMaxSpeed,
		EnableClickMultiplier,
	}

	public enum FrogsStatus : byte
	{
		None,
		BlockKill,
		BlockExile,
		UseVent,
		SpeedUp,
		Multiplier,
	}

	public int Order => 114514;

	public ExtremeAbilityButton? Button { get; set; }
	public override IStatusModel? Status => this.status;

	private DeepOneAbilityHandler? abilityHandler;
	private DeepOneStatus? status;

	private float deathCooldownReduction = 0.0f;
	private float removeFrogsPenalty = 0.0f;
	private int requiredWinFrogs = 0;

	public DeepOne() : base(
		RoleArgs.BuildNeutral(
			ExtremeRoleId.DeepOne,
			ColorPalette.DeepOneDarkGreen,
			RolePropPresets.OptionalDefault))
	{
		
	}

	public void UpdateFrogs(int frogCount)
	{
		if (this.status is null)
		{
			return;
		}

		if (this.status.UpdateStatus(frogCount) &&
			this.Button is not null)
		{
			// クールタイムのリセットとクールタイムの追加
			this.Button.OnMeetingStart();
			this.Button.OnMeetingEnd();
			this.Button.SetCooldownTimer(this.Button.Timer + this.removeFrogsPenalty);
		}
		this.UseVent = this.status.UseVent;
		if (frogCount >= this.requiredWinFrogs)
		{
			this.IsWin = true;
		}
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
		=> IRoleAbility.IsCommonUse();

	public bool UseAbility()
		=> this.abilityHandler?.UseAbility() ?? false;

	public void CleanUp()
	{
	}

	public void AllReset(PlayerControl rolePlayer)
	{
		this.IsWin = false;
		this.MoveSpeed = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
			FloatOptionNames.PlayerSpeedMod);
		this.status?.ResetStatus();
		this.UseVent = false;
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (rolePlayer.IsInValid() ||
			this.Button is null ||
			this.status is null)
		{
			return;
		}

		if (this.status.IsSpeedUp)
		{
			this.IsBoost = true;
			this.MoveSpeed = 3.0f;
		}
		else
		{
			this.IsBoost = false;
			this.MoveSpeed = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
				FloatOptionNames.PlayerSpeedMod);
		}


		int deadCount = 0;
		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (player.IsInValid())
			{
				deadCount++;
			}
		}

		float reduction = deadCount * this.deathCooldownReduction;
		float calculatedCooldown = Math.Max(0.5f, this.Button.Timer * (1.0f - reduction));

		this.Button?.SetCooldownTimer(calculatedCooldown);
	}

	public void ModifiedVote(byte rolePlayerId, ref Dictionary<byte, byte> voteTarget, ref Dictionary<byte, int> voteResult)
	{
		if (this.status is null || this.status.IsBlockExile || voteResult.Count <= 0 || OnemanMeetingSystemManager.IsActive)
		{
			return;
		}
		voteResult.Remove(rolePlayerId);
	}

	public IEnumerable<VoteInfo> GetModdedVoteInfo(VoteInfoCollector collector, NetworkedPlayerInfo rolePlayer)
	{
		if (this.status is null || this.status.IsBlockExile || OnemanMeetingSystemManager.IsActive)
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
		=> this.IsNeutralSameTeam(targetRole);

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateCommonAbilityOption(factory, 15.0f);

		factory.CreateIntOption(
			DeepOneOption.RequiredClicks,
			5, 1, 10, 1);

		factory.CreateIntOption(
			DeepOneOption.RequiredWinFrogs,
			5, 1, 10, 1);

		factory.CreateFloatOption(
			DeepOneOption.FrogsRemovePenalty,
			1.0f, 0.0f, 10.0f, 0.5f,
			format: OptionUnit.Percentage);

		factory.CreateIntOption(
			DeepOneOption.DeathCooldownReduction,
			1, 0, 100, 1,
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

		this.deathCooldownReduction = loader.GetValue<DeepOneOption, int>(
			DeepOneOption.DeathCooldownReduction) / 100.0f;
		this.removeFrogsPenalty = loader.GetValue<DeepOneOption, float>(
			DeepOneOption.FrogsRemovePenalty);
		this.requiredWinFrogs = loader.GetValue<DeepOneOption, int>(DeepOneOption.RequiredWinFrogs);

		this.status = new DeepOneStatus(
			enableKillBlock: loader.GetValue<DeepOneOption, bool>(DeepOneOption.EnableKillBlock),
			enableExileBlock: loader.GetValue<DeepOneOption, bool>(DeepOneOption.EnableExileBlock),
			enableVentUnlock: loader.GetValue<DeepOneOption, bool>(DeepOneOption.EnableVentUnlock),
			enableMaxSpeed: loader.GetValue<DeepOneOption, bool>(DeepOneOption.EnableMaxSpeed));

		var system = ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.DeepOneFrogsControlSystem,
			() => new DeepOneFrogsControlSystem(
				loader.GetValue<DeepOneOption, int>(DeepOneOption.RequiredClicks),
				loader.GetValue<DeepOneOption, bool>(DeepOneOption.EnableClickMultiplier)));

		this.abilityHandler = new DeepOneAbilityHandler(system, this.status);
		this.AbilityClass = this.abilityHandler;

		this.UseVent = false;
		this.IsWin = false;
	}
}
