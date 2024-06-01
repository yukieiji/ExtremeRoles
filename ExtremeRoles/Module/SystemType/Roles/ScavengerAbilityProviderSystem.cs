using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using Hazel;

using System;
using System.Collections.Generic;
using System.Linq;

using WeaponAbility = ExtremeRoles.Roles.Solo.Impostor.Scavenger.Ability;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class ScavengerAbilityProviderSystem(
	in WeaponAbility initWeapon,
	in bool SyncPlayer,
	in ScavengerAbilityProviderSystem.RandomOption? randomOption) : IDirtableSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.ScavengerAbilityProvider;

	public readonly record struct RandomOption(
		bool AllowDupe,
		bool ContainAdvanced);

	private readonly WeaponAbility initAbility = initWeapon;
	private readonly bool isSyncPlayer = SyncPlayer;
	private readonly RandomOption? randomOption = randomOption;
	private WeaponAbility[]? abilities;
	private HashSet<WeaponAbility> initProvided = new HashSet<WeaponAbility>();
	private bool init = false;

	public bool IsDirty => false;

	public void Deteriorate(float deltaTime)
	{
		if (this.init || IntroCutscene.Instance)
		{
			return;
		}
		putWeaponToMap();
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{ }

	public void Deserialize(MessageReader reader, bool initialState)
	{ }

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{

	}

	public WeaponAbility GetInitWepon()
	{
		if (!this.randomOption.HasValue)
		{
			this.initProvided.Add(this.initAbility);
			return this.initAbility;
		}
		else
		{
			var opt = this.randomOption.Value;
			if (this.abilities is null)
			{
				this.abilities = Enum.GetValues<WeaponAbility>()
					.Where(x =>
						x is not WeaponAbility.Null && (
						opt.ContainAdvanced || (
							x is not
								WeaponAbility.SniperRifle or
								WeaponAbility.BeamRifle or
								WeaponAbility.BeamSaber or
								WeaponAbility.All)
						)
					).ToArray();
			}

			if (opt.AllowDupe || this.initProvided.Count < this.abilities.Length)
			{
				var ability = getRandomAbility();
				this.initProvided.Add(ability);

				return ability;
			}
			else
			{
				return WeaponAbility.Null;
			}
		}
	}

	private void putWeaponToMap()
	{
		this.init = true;

		bool isSwordProvided = this.initProvided.Contains(WeaponAbility.Sword);
		bool isHandGunProvided = this.initProvided.Contains(WeaponAbility.HandGun);
		bool isFlameThrowerProvided = this.initProvided.Contains(WeaponAbility.FlameThrower);

		if (this.isSyncPlayer)
		{
			if (this.initProvided.Contains(WeaponAbility.All) ||
				(isSwordProvided && isHandGunProvided && isFlameThrowerProvided))
			{
				return;
			}
			else if (this.initProvided.Contains(WeaponAbility.SniperRifle))
			{
				if (isFlameThrowerProvided)
				{
					return;
				}
				put(WeaponAbility.FlameThrower);
			}
			else if (this.initProvided.Contains(WeaponAbility.BeamRifle))
			{
				if (isSwordProvided)
				{
					return;
				}
				put(WeaponAbility.Sword);
			}
			else if (this.initProvided.Contains(WeaponAbility.BeamSaber))
			{
				if (isHandGunProvided)
				{
					return;
				}
				put(WeaponAbility.HandGun);
			}
			else
			{
				if (!isSwordProvided)
				{
					put(WeaponAbility.Sword);
				}
				if (!isHandGunProvided)
				{
					put(WeaponAbility.HandGun);
				}
				if (!isFlameThrowerProvided)
				{
					put(WeaponAbility.FlameThrower);
				}
			}
		}
		else
		{
			var scavent = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<
				ExtremeRoles.Roles.Solo.Impostor.Scavenger>();
			if (scavent is null) { return; }

			switch (scavent.InitAbility)
			{
				case WeaponAbility.Null:
					put(WeaponAbility.HandGun,
						WeaponAbility.Sword,
						WeaponAbility.FlameThrower);
					break;
				case WeaponAbility.HandGun:
					put(WeaponAbility.FlameThrower, WeaponAbility.Sword);
					break;
				case WeaponAbility.Sword:
					put(WeaponAbility.FlameThrower, WeaponAbility.HandGun);
					break;
				case WeaponAbility.FlameThrower:
					put(WeaponAbility.HandGun, WeaponAbility.Sword);
					break;
				case WeaponAbility.SniperRifle:
					put(WeaponAbility.FlameThrower);
					break;
				case WeaponAbility.BeamSaber:
					put(WeaponAbility.HandGun);
					break;
				case WeaponAbility.BeamRifle:
					put(WeaponAbility.Sword);
					break;
				default:
					break;
			}
		}
	}

	private void put(params WeaponAbility[] ability)
	{

	}

	private WeaponAbility getRandomAbility()
	{
		if (this.abilities is null)
		{
			return WeaponAbility.Null;
		}
		int index = RandomGenerator.Instance.Next(0, this.abilities.Length);
		var ability = this.abilities[index];
		return ability;
	}
}
