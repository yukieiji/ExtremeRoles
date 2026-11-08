using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Extension.Vector;
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
		RequiredClicks,
		RestoreTime,
	}

	public ExtremeAbilityButton? Button { get; set; }

	private NullifierSystem? system;
	private Vector2 castOriginPos;

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
			isReduceOnActive: false);
	}

	public bool IsAbilityUse()
		=> IRoleAbility.IsCommonUse();

	public bool CheckAbility()
		=> PlayerControl.LocalPlayer.IsAlive() &&
			this.castOriginPos.IsCloseTo(PlayerControl.LocalPlayer.GetTruePosition());

	public bool UseAbility()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer.IsInValid())
		{
			return false;
		}

		this.castOriginPos = localPlayer.GetTruePosition();
		return true;
	}

	public void CleanUp()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer.IsAlive())
		{
			this.system?.RpcReserveSeal(this.castOriginPos);
		}
	}

	public void ForceCleanUp()
	{
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
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer != null && rolePlayer.PlayerId == localPlayer.PlayerId)
		{
			ForceCleanUp();
		}
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(
			factory,
			defaultAbilityCount: 2,
			maxAbilityCount: 15,
			defaultActiveTime: 3.0f);

		factory.CreateSelectionOption<Option, NullifierTargetTeam>(Option.TargetTeam);
		factory.CreateFloatOption(Option.Range, 7.0f, 1.0f, 50.0f, 0.5f);
		factory.CreateIntOption(Option.RequiredClicks, 10, 1, 50, 1);
		factory.CreateFloatOption(Option.RestoreTime, 10.0f, 1.0f, 60.0f, 1.0f, format: OptionUnit.Second);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		var targetTeam = (NullifierTargetTeam)loader.GetValue<Option, int>(Option.TargetTeam);
		float range = loader.GetValue<Option, float>(Option.Range);
		int requiredClicks = loader.GetValue<Option, int>(Option.RequiredClicks);
		float restoreTime = loader.GetValue<Option, float>(Option.RestoreTime);

		this.system = ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.NullifierSystem,
			() => new NullifierSystem(range, requiredClicks, restoreTime, targetTeam));
	}
}
