using System;
using System.Collections.Generic;
using System.Linq;

using Hazel;
using UnityEngine;
using Newtonsoft.Json.Linq;

using ExtremeRoles.Extension.Json;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;

using UnityObject = UnityEngine.Object;
using WeaponAbility = ExtremeRoles.Roles.Solo.Impostor.Scavenger.Ability;


#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class ScavengerAbilitySystem(
	in WeaponAbility initWeapon,
	in bool isSetWepon,
	in bool syncPlayer,
	in ScavengerAbilitySystem.RandomOption? randomOption) : IDirtableSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.ScavengerAbility;

	public readonly record struct RandomOption(
		bool AllowDupe,
		bool ContainAdvanced);

	public enum Ops : byte
	{
		PickUp,
		WeponOps,
	}

	private readonly WeaponAbility initAbility = initWeapon;
	private readonly bool isSyncPlayer = syncPlayer;
	private readonly RandomOption? randomOption = randomOption;
	private readonly Dictionary<WeaponAbility, GameObject> settedWeapon = new Dictionary<WeaponAbility, GameObject>();
	private WeaponAbility[]? abilities;
	private HashSet<WeaponAbility> initProvided = new HashSet<WeaponAbility>();
	private bool init = !isSetWepon;

	public bool IsDirty => false;

	private sealed class WeponSetter : IDisposable
	{
		private readonly bool isSync;

		private IReadOnlyDictionary<WeaponAbility, Vector2>? setPos;

		public WeponSetter(bool isSync)
		{
			this.isSync = isSync;
		}
		public void SetFromInitWepon(
			in Dictionary<WeaponAbility, GameObject> result,
			in IReadOnlySet<WeaponAbility> init)
		{
			bool isSwordProvided = init.Contains(WeaponAbility.ScavengerSword);
			bool isHandGunProvided = init.Contains(WeaponAbility.ScavengerHandGun);
			bool isFlameProvided = init.Contains(WeaponAbility.ScavengerFlame);

			if (this.isSync)
			{
				/*
				if (init.Contains(WeaponAbility.Aguni) ||
					(isSwordProvided && isHandGunProvided && isFlameProvided))
				{
					return;
				}
				else
				*/
				if (init.Contains(WeaponAbility.ScavengerSniperRifle))
				{
					if (isFlameProvided)
					{
						return;
					}
					set(result, WeaponAbility.ScavengerFlame);
				}
				else if (init.Contains(WeaponAbility.ScavengerBeamRifle))
				{
					if (isSwordProvided)
					{
						return;
					}
					set(result, WeaponAbility.ScavengerSword);
				}
				else if (init.Contains(WeaponAbility.ScavengerBeamSaber))
				{
					if (isHandGunProvided)
					{
						return;
					}
					set(result, WeaponAbility.ScavengerHandGun);
				}
				else
				{
					if (!isSwordProvided)
					{
						set(result, WeaponAbility.ScavengerSword);
					}
					if (!isHandGunProvided)
					{
						set(result, WeaponAbility.ScavengerHandGun);
					}
					if (!isFlameProvided)
					{
						set(result, WeaponAbility.ScavengerFlame);
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
					case WeaponAbility.ScavengerNull:
						set(result,
							WeaponAbility.ScavengerHandGun,
							WeaponAbility.ScavengerSword,
							WeaponAbility.ScavengerFlame);
						break;
					case WeaponAbility.ScavengerHandGun:
						set(result,
							WeaponAbility.ScavengerFlame, WeaponAbility.ScavengerSword);
						break;
					case WeaponAbility.ScavengerSword:
						set(result,
							WeaponAbility.ScavengerFlame, WeaponAbility.ScavengerHandGun);
						break;
					case WeaponAbility.ScavengerFlame:
						set(result,
							WeaponAbility.ScavengerHandGun, WeaponAbility.ScavengerSword);
						break;
					case WeaponAbility.ScavengerSniperRifle:
						set(result,
							WeaponAbility.ScavengerFlame);
						break;
					case WeaponAbility.ScavengerBeamSaber:
						set(result,
							WeaponAbility.ScavengerHandGun);
						break;
					case WeaponAbility.ScavengerBeamRifle:
						set(result,
							WeaponAbility.ScavengerSword);
						break;
					default:
						break;
				}
			}
		}

		private void set(in Dictionary<WeaponAbility, GameObject> result, params WeaponAbility[] abilities)
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

				ExtremeRolesPlugin.Logger.LogInfo($"Scavenger Weapon:{ability}, pos:{pos}");
				obj.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000.0f);

				var wepon = obj.AddComponent<ScavengerWeponMapUsable>();
				wepon.WeponInfo = new(ability, this.isSync);
				result.Add(ability, obj);
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

		public void Dispose()
		{ }
	}

	public void MarkClean()
	{
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
		Ops ops = (Ops)msgReader.ReadByte();

		switch (ops)
		{
			case Ops.PickUp:
				this.destroyWeponOnMap(msgReader);
				break;
			case Ops.WeponOps:
				weaponOps(msgReader);
				break;
			default:
				break;
		}
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
						x is not WeaponAbility.ScavengerNull && (
						opt.ContainAdvanced || (
							x is
								WeaponAbility.ScavengerFlame or
								WeaponAbility.ScavengerHandGun or
								WeaponAbility.ScavengerSword)
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
				return WeaponAbility.ScavengerNull;
			}
		}
	}

	private void setWeaponToMap()
	{
		this.init = true;

		using var setter = new WeponSetter(this.isSyncPlayer);
		setter.SetFromInitWepon(this.settedWeapon, this.initProvided);
	}

	private void destroyWeponOnMap(in MessageReader msgReader)
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

	private void weaponOps(
		in MessageReader msgReader)
	{
		byte playerId = msgReader.ReadByte();
		var ability = (WeaponAbility)msgReader.ReadByte();
		float x = msgReader.ReadSingle();
		float y = msgReader.ReadSingle();
		var player = Player.GetPlayerControlById(playerId);
		if (!ExtremeRoleManager.TryGetSafeCastedRole<ExtremeRoles.Roles.Solo.Impostor.Scavenger> (
				playerId, out var scavenger) ||
			player == null)
		{
			return;
		}
		player.NetTransform.SnapTo(new (x, y));
		scavenger.WeaponOps(ability, player, msgReader);
	}

	private WeaponAbility getRandomAbility()
	{
		if (this.abilities is null)
		{
			return WeaponAbility.ScavengerNull;
		}
		int index = RandomGenerator.Instance.Next(0, this.abilities.Length);
		var ability = this.abilities[index];
		return ability;
	}
}
