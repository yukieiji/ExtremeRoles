using System;
using System.Collections.Generic;
using System.Linq;

using Hazel;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;

using UnityObject = UnityEngine.Object;
using WeaponAbility = ExtremeRoles.Roles.Solo.Impostor.Scavenger.Ability;


using Newtonsoft.Json.Linq;
using ExtremeRoles.Extension.Json;


#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class ScavengerAbilityProviderSystem(
	in WeaponAbility initWeapon,
	in bool isSetWepon,
	in bool syncPlayer,
	in ScavengerAbilityProviderSystem.RandomOption? randomOption) : IDirtableSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.ScavengerAbilityProvider;

	public readonly record struct RandomOption(
		bool AllowDupe,
		bool ContainAdvanced);

	private readonly WeaponAbility initAbility = initWeapon;
	private readonly bool isSyncPlayer = syncPlayer;
	private readonly RandomOption? randomOption = randomOption;
	private readonly Dictionary<WeaponAbility, GameObject> settedWeapon = new Dictionary<WeaponAbility, GameObject>();
	private WeaponAbility[]? abilities;
	private HashSet<WeaponAbility> initProvided = new HashSet<WeaponAbility>();
	private bool init = !isSetWepon;

	public bool IsDirty => false;

	private class WeponSetter
	{
		private readonly bool isSync;

		private IReadOnlyDictionary<WeaponAbility, Vector2>? setPos;

		public WeponSetter(bool isSync)
		{
			this.isSync = isSync;
		}
		public void SetFromInitWepon(
			in IReadOnlySet<WeaponAbility> init)
		{
			bool isSwordProvided = init.Contains(WeaponAbility.Sword);
			bool isHandGunProvided = init.Contains(WeaponAbility.HandGun);
			bool isFlameThrowerProvided = init.Contains(WeaponAbility.FlameThrower);

			if (this.isSync)
			{
				if (init.Contains(WeaponAbility.All) ||
					(isSwordProvided && isHandGunProvided && isFlameThrowerProvided))
				{
					return;
				}
				else if (init.Contains(WeaponAbility.SniperRifle))
				{
					if (isFlameThrowerProvided)
					{
						return;
					}
					set(WeaponAbility.FlameThrower);
				}
				else if (init.Contains(WeaponAbility.BeamRifle))
				{
					if (isSwordProvided)
					{
						return;
					}
					set(WeaponAbility.Sword);
				}
				else if (init.Contains(WeaponAbility.BeamSaber))
				{
					if (isHandGunProvided)
					{
						return;
					}
					set(WeaponAbility.HandGun);
				}
				else
				{
					if (!isSwordProvided)
					{
						set(WeaponAbility.Sword);
					}
					if (!isHandGunProvided)
					{
						set(WeaponAbility.HandGun);
					}
					if (!isFlameThrowerProvided)
					{
						set(WeaponAbility.FlameThrower);
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
						set(WeaponAbility.HandGun,
							WeaponAbility.Sword,
							WeaponAbility.FlameThrower);
						break;
					case WeaponAbility.HandGun:
						set(WeaponAbility.FlameThrower, WeaponAbility.Sword);
						break;
					case WeaponAbility.Sword:
						set(WeaponAbility.FlameThrower, WeaponAbility.HandGun);
						break;
					case WeaponAbility.FlameThrower:
						set(WeaponAbility.HandGun, WeaponAbility.Sword);
						break;
					case WeaponAbility.SniperRifle:
						set(WeaponAbility.FlameThrower);
						break;
					case WeaponAbility.BeamSaber:
						set(WeaponAbility.HandGun);
						break;
					case WeaponAbility.BeamRifle:
						set(WeaponAbility.Sword);
						break;
					default:
						break;
				}
			}
		}

		private void set(params WeaponAbility[] abilities)
		{
			if (setPos == null)
			{
				setPos = getSetPoint();
			}

			foreach (var ability in abilities)
			{
				if (!setPos.TryGetValue(ability, out var pos))
				{
					continue;
				}
				var obj = new GameObject(ability.ToString());
				obj.transform.localPosition = new Vector3(pos.x, pos.y, pos.y / 1000.0f);
				var wepon = obj.AddComponent<ScavengerWeponMapUsable>();
				wepon.WeponInfo = new(ability, this.isSync);
			}
		}
		private IReadOnlyDictionary<WeaponAbility, Vector2> getSetPoint()
		{
			var json = JsonParser.GetJObjectFromAssembly(
				"ExtremeRoles.Resources.JsonData.ScavengerWeponPoint.json");
			if (json == null)
			{
				throw new ArgumentNullException("Json data is null!!!!");
			}
			string key = Map.Name;

			var result = new Dictionary<WeaponAbility, Vector2>(3);

			JArray? posInfo = json.Get<JArray>(key);
			if (posInfo == null) { return result; }

			for (int i = 0; i < posInfo.Count; ++i)
			{
				JArray? id = posInfo.Get<JArray>(i);
				if (id == null) { continue; }

				result.Add(
					(WeaponAbility)(i + 1),
					new Vector2(
						(float)(id[0]),
						(float)(id[1])));
			}
			return result;
		}
	}

	public void Deteriorate(float deltaTime)
	{
		if (this.init || IntroCutscene.Instance)
		{
			return;
		}
		setWeaponToMap();
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{ }

	public void Deserialize(MessageReader reader, bool initialState)
	{ }

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		WeaponAbility weapon = (WeaponAbility)msgReader.ReadByte();
		if (!this.isSyncPlayer ||
			!this.settedWeapon.TryGetValue(weapon, out var obj) ||
			obj == null)
		{
			return;
		}
		UnityObject.Destroy(obj);
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

	private void setWeaponToMap()
	{
		this.init = true;

		new WeponSetter(this.isSyncPlayer)
			.SetFromInitWepon(this.initProvided);
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
