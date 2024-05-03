using System;
using System.Collections.Generic;
using ExtremeRoles.Resources;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.ButtonAutoActivator;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Scavenger : SingleRoleBase, IRoleUpdate, IRoleAbility
{
	public ExtremeAbilityButton? Button
	{
		get => this.internalButton;
		set
		{
			if (value is not ExtremeMultiModalAbilityButton button)
			{
				throw new ArgumentException("This role using multimodal ability");
			}
			this.internalButton = button;
		}
	}

	public enum Ability
	{
		// 何もないよ
		Null,

		// 落ちてるやつ
		HandGun,
		FlameThrower,
		Sword,

		// HandGun + Sword
		SniperRifle,
		// HandGun + FlameThrower
		BeamRifle,
		// FlameThrower + Sword
		BeamSaber,

		// FlameThrower + Sword + HandGun
		All,
	}

	private Ability initMode = Ability.Null;
	private IReadOnlyDictionary<Ability, BehaviorBase>? allAbility;

	private ExtremeMultiModalAbilityButton? internalButton;

	public Scavenger() : base(
		ExtremeRoleId.Scavenger,
		ExtremeRoleType.Impostor,
		ExtremeRoleId.Scavenger.ToString(),
		Palette.ImpostorRed,
		true, false, true, true)
	{ }

	public void CreateAbility()
	{
		if (this.allAbility is null ||
			!this.allAbility.TryGetValue(this.initMode, out var ability))
		{
			return;
		}

		this.Button = new ExtremeMultiModalAbilityButton(
			[ ability ],
			new RoleButtonActivator(),
			KeyCode.F);
	}

	public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
	{　}

	public void ResetOnMeetingStart()
	{　}

	public void Update(PlayerControl rolePlayer)
	{
		if (this.Button?.Behavior is NullBehaviour)
		{
			this.Button.SetButtonShow(false);
		}
	}

	protected override void CreateSpecificOption(IOptionInfo parentOps)
	{
		throw new NotImplementedException();
	}

	protected override void RoleSpecificInit()
	{
		this.allAbility = new Dictionary<Ability, BehaviorBase>()
		{
			{ Ability.Null, new NullBehaviour() },
			{
				Ability.HandGun,
				new CountBehavior(
					"",
					Loader.CreateSpriteFromResources(Path.TestButton),
					IRoleAbility.IsCommonUse,
					handGunAbility,
					isReduceOnActive: true)
			},
			{
				Ability.SniperRifle,
				new CountBehavior(
					"",
					Loader.CreateSpriteFromResources(Path.TestButton),
					IRoleAbility.IsCommonUse,
					sniperRifleAbility,
					isReduceOnActive: true)
			},
			{
				Ability.BeamRifle,
				new CountBehavior(
					"",
					Loader.CreateSpriteFromResources(Path.TestButton),
					IRoleAbility.IsCommonUse,
					beamRifleAbility,
					isReduceOnActive: true)
			}
		};
	}
	private bool handGunAbility()
	{
		return true;
	}

	private bool sniperRifleAbility()
	{
		return true;
	}

	private bool beamRifleAbility()
	{
		return true;
	}
}
