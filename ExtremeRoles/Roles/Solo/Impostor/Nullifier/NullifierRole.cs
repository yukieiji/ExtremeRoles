using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.Nullifier;

public sealed class NullifierRole :
	SingleRoleBase,
	IRoleAutoBuildAbility,
	IRoleSpecialReset
{
	public enum Option
	{
		TargetTeam,
		Range,
		MaxSealCount,
		RequiredClicks,
		RestoreTime,
	}

	public ExtremeAbilityButton? Button { get; set; }

	private NullifierSystem? system;
	private PlayerSpeedSystem? speedSystem;
	private bool isCasting;

	public NullifierRole() : base(
		RoleArgs.BuildImpostor(ExtremeRoleId.Nullifier))
	{
	}

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			Tr.GetString("NullifierAbility"),
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Nullifier),
			checkAbility: CheckAbility,
			abilityOff: CleanUp,
			forceAbilityOff: ForceCleanUp,
			isReduceOnActive: true);
	}

	public bool IsAbilityUse()
	{
		if (this.system == null || !this.system.CanReserve())
		{
			return false;
		}

		return IRoleAbility.IsCommonUse();
	}

	public bool CheckAbility()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		return localPlayer.IsAlive();
	}

	public bool UseAbility()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer.IsInValid())
		{
			return false;
		}

		this.isCasting = true;
		this.speedSystem?.UpdateSelfSpeed(0.001f);
		return true;
	}

	public void CleanUp()
	{
		if (!this.isCasting)
		{
			return;
		}

		this.isCasting = false;
		this.speedSystem?.UpdateSelfSpeed(0.0f);

		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer.IsAlive())
		{
			this.system?.RpcReserveSeal(localPlayer.GetTruePosition());
		}
	}

	public void ForceCleanUp()
	{
		if (!this.isCasting)
		{
			return;
		}

		this.isCasting = false;
		this.speedSystem?.UpdateSelfSpeed(0.0f);
	}

	public void ResetOnMeetingStart()
	{
		ForceCleanUp();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		ForceCleanUp();
	}

	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		reset(rolePlayer);
	}

	public void AllReset(PlayerControl rolePlayer)
	{
		reset(rolePlayer);
	}

	private void reset(PlayerControl rolePlayer)
	{
		if (PlayerControl.LocalPlayer != null && rolePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			ForceCleanUp();
		}
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(
			factory,
			defaultAbilityCount: 5,
			maxAbilityCount: 15,
			defaultActiveTime: 3.0f);

		factory.CreateSelectionOption<Option, NullifierTargetTeam>(Option.TargetTeam);
		factory.CreateFloatOption(Option.Range, 2.5f, 1.0f, 10.0f, 0.5f);
		factory.CreateIntOption(Option.MaxSealCount, 2, 1, 10, 1);
		factory.CreateIntOption(Option.RequiredClicks, 10, 1, 50, 1);
		factory.CreateFloatOption(Option.RestoreTime, 10.0f, 1.0f, 60.0f, 1.0f, format: OptionUnit.Second);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		var targetTeam = (NullifierTargetTeam)loader.GetValue<Option, int>(Option.TargetTeam);
		float range = loader.GetValue<Option, float>(Option.Range);
		int maxSealCount = loader.GetValue<Option, int>(Option.MaxSealCount);
		int requiredClicks = loader.GetValue<Option, int>(Option.RequiredClicks);
		float restoreTime = loader.GetValue<Option, float>(Option.RestoreTime);

		this.speedSystem = PlayerSpeedSystem.Get();
		this.system = ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.Nullifier,
			() => new NullifierSystem(range, maxSealCount, requiredClicks, restoreTime, targetTeam));
	}
}
