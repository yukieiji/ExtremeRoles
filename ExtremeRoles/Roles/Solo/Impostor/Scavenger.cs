using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;




using ExtremeRoles.Resources;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.ButtonAutoActivator;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using UnityObject = UnityEngine.Object;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;


public sealed class Scavenger : SingleRoleBase, IRoleUpdate, IRoleAbility
{
	private record struct CreateParam(string Name, string Path);

	private interface IWeapon
	{
		public BehaviorBase Create(in CreateParam param);
		public void Hide();
	}

	private sealed class Sword(
		float chargeTime,
		float activeTime,
		float r) : IWeapon
	{
		private SwordBehaviour? showSword;
		private readonly float r = r;
		private readonly float chargeTime = chargeTime;
		private readonly float activeTime = activeTime;

		public BehaviorBase Create(in CreateParam param)
		{
			var behavior = new ChargingAndActivatingCountBehaviour(
				param.Name,
				Loader.CreateSpriteFromResources(param.Path),
				isSwordUse,
				startSwordRotation,
				startSwordCharge,
				ChargingAndActivatingCountBehaviour.ReduceTiming.OnActive,
				isValidSword, isValidSword,
				Hide, Hide);
			behavior.ChargeTime = chargeTime;
			behavior.ActiveTime = activeTime;
			return behavior;
		}

		public void Hide()
		{
			if (this.showSword == null)
			{
				return;
			}
			this.showSword.gameObject.SetActive(false);
		}

		private bool startSwordCharge()
		{
			// Rpc処理
			if (this.showSword == null)
			{
				this.showSword = createSword(
					CachedPlayerControl.LocalPlayer);
			}

			this.showSword.gameObject.SetActive(true);
			this.showSword.SetRotation(
				new SwordBehaviour.RotationInfo(
					this.chargeTime, -45f, false),
				true);

			return true;
		}

		private bool startSwordRotation(float chargeGauge)
		{
			// Rpc処理
			if (this.showSword == null)
			{
				return false;
			}
			this.showSword.SetRotation(
				new SwordBehaviour.RotationInfo(
					this.activeTime, 365f, true),
				false);
			return true;
		}

		private bool isSwordUse(bool isCharge, float chargeGauge)
		{
			bool isCommonUse = IRoleAbility.IsCommonUse();
			if (!isCommonUse)
			{
				return false;
			}
			if (!isCharge)
			{
				return true;
			}
			return chargeGauge == 1.0f;
		}
		private bool isValidSword()
			=> this.showSword != null && this.showSword.gameObject.active;

		private SwordBehaviour createSword(
			in PlayerControl rolePlayer)
			=> SwordBehaviour.Create(
				this.r, rolePlayer);
	}

	private sealed class FlameThrower(float fireSecond, float fireDeadSecond) : IWeapon
	{
		private readonly float fireSecond = fireSecond;
		private readonly float fireDeadSecond = fireDeadSecond;

		private FlameThrowerBehaviour? flame;

		public BehaviorBase Create(in CreateParam param)
		{
			var behavior = new ChargingAndActivatingCountBehaviour(
				param.Name,
				Loader.CreateSpriteFromResources(param.Path),
				isFireThrowerUse,
				startSwordRotation,
				startSwordCharge,
				ChargingAndActivatingCountBehaviour.ReduceTiming.OnActive,
				IRoleAbility.IsCommonUse,
				IRoleAbility.IsCommonUse,
				Hide, Hide);
			return behavior;
		}

		public void Hide()
		{
			if (this.flame == null)
			{
				return;
			}
			this.flame.gameObject.SetActive(false);
		}

		private bool startSwordCharge()
		{
			// Rpc処理
			if (this.flame == null)
			{
				this.flame = createSword(
					CachedPlayerControl.LocalPlayer);
			}

			this.flame.StartCharge();
			this.flame.gameObject.SetActive(true);

			return true;
		}

		private bool startSwordRotation(float _)
		{
			// Rpc処理
			if (this.flame == null)
			{
				return false;
			}
			this.flame.Fire();
			return true;
		}

		private bool isFireThrowerUse(bool _, float __)
			=> IRoleAbility.IsCommonUse();

		private FlameThrowerBehaviour createSword(
			in PlayerControl rolePlayer)
			=> FlameThrowerBehaviour.Create(
				this.fireSecond, this.fireDeadSecond, rolePlayer);
	}

	private sealed class BeamSaber(
		float range,
		bool isAutoDetect) : IWeapon
	{
		private readonly float range = range;
		private readonly bool isIgnoreAutoDetect = !isAutoDetect;
		private byte targetPlayerId;
		private Vector2 chargePos = Vector2.zero;
		private readonly List<PlayerControl> cacheResult = new List<PlayerControl>();

		public BehaviorBase Create(in CreateParam param)
			=> new ChargingCountBehaviour(
				param.Name,
				Loader.CreateSpriteFromResources(param.Path),
				isIaiOk,
				tryIai,
				startIai,
				ChargingCountBehaviour.ReduceTiming.OnActive,
				iaiCheck);

		public void Hide()
		{ }

		private bool iaiCheck()
		{
			var curPos = CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();
			return curPos == this.chargePos;
		}

		private bool startIai()
		{
			this.cacheResult.Clear();
			this.targetPlayerId = byte.MaxValue;
			this.chargePos = CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();
			return true;
		}

		private bool tryIai(float chargeGauge)
		{
			if (this.targetPlayerId == byte.MaxValue)
			{
				return this.isIgnoreAutoDetect;
			}


			Player.RpcUncheckMurderPlayer(
				CachedPlayerControl.LocalPlayer.PlayerId,
				this.targetPlayerId,
				byte.MinValue);
			//　おとならす

			return true;
		}

		private bool isIaiOk(bool isCharge, float chargeGauge)
		{
			this.targetPlayerId = byte.MaxValue;

			bool isCommonUse = IRoleAbility.IsCommonUse();

			if (!isCommonUse)
			{
				return false;
			}

			if (!isCharge)
			{
				return true;
			}

			float searchRange = this.range * chargeGauge * chargeGauge;

			var allPlayer = GameData.Instance.AllPlayers;
			this.cacheResult.Clear();

			if (!ShipStatus.Instance)
			{
				return false;
			}

			PlayerControl pc = CachedPlayerControl.LocalPlayer;
			Vector2 truePosition = pc.GetTruePosition();
			var role = ExtremeRoleManager.GetLocalPlayerRole();

			foreach (GameData.PlayerInfo playerInfo in
				GameData.Instance.AllPlayers.GetFastEnumerator())
			{
				if (!Player.IsValidPlayer(role, pc, playerInfo))
				{
					continue;
				}
				PlayerControl target = playerInfo.Object;

				Vector2 vector = target.GetTruePosition() - truePosition;
				float magnitude = vector.magnitude;
				if (magnitude <= searchRange)
				{
					this.cacheResult.Add(target);
				}
			}

			this.cacheResult.Sort(delegate (PlayerControl a, PlayerControl b)
			{
				float magnitude2 = (a.GetTruePosition() - truePosition).magnitude;
				float magnitude3 = (b.GetTruePosition() - truePosition).magnitude;
				if (magnitude2 > magnitude3)
				{
					return 1;
				}
				if (magnitude2 < magnitude3)
				{
					return -1;
				}
				return 0;
			});

			if (this.cacheResult.Count <= 0)
			{
				return this.isIgnoreAutoDetect;
			}
			this.targetPlayerId = this.cacheResult[0].PlayerId;
			return true;
		}
	}

	private sealed class NormalGun(
		in Ability type,
		in BulletBehaviour.Parameter param) : IWeapon
	{
		private Ability type = type;
		private readonly BulletBehaviour.Parameter pram = param;
		private readonly Dictionary<int, BulletBehaviour> bullet = new();
		private int id = 0;
		private Vector2 playerDirection;

		public BehaviorBase Create(in CreateParam param)
		{
			return new CountBehavior(
			   param.Name,
			   Loader.CreateSpriteFromResources(param.Path),
			   isUse,
			   ability,
			   forceAbilityOff: Hide);
		}

		public void Hide()
		{
			foreach (var ballet in this.bullet.Values)
			{
				if (ballet == null ||
					ballet.gameObject == null)
				{
					continue;
				}
				UnityObject.Destroy(ballet.gameObject);
			}
			this.bullet.Clear();
		}

		public void Hide(int id)
		{
			if (this.bullet.TryGetValue(id, out var bullet) &&
				bullet != null &&
				bullet.gameObject != null)
			{
				UnityObject.Destroy(bullet.gameObject);
			}
			this.bullet.Remove(id);
		}

		public void CreateBullet(
			int mngId,
			in Vector2 direction,
			in PlayerControl? rolePlayer)
		{
			if (rolePlayer == null)
			{
				throw new ArgumentNullException("RolePlayer is null");
			}

			var bullet = BulletBehaviour.Create(
				mngId,
				rolePlayer,
				direction,
				this.pram);

			this.bullet.Add(mngId, bullet);
		}

		private bool isUse()
		{
			this.playerDirection = Vector2.zero;

			if (KeyboardJoystick.player.GetButton(40))
			{
				this.playerDirection.x = this.playerDirection.x + 1f;
			}
			if (KeyboardJoystick.player.GetButton(39))
			{
				this.playerDirection.x = this.playerDirection.x - 1f;
			}
			if (KeyboardJoystick.player.GetButton(44))
			{
				this.playerDirection.y = this.playerDirection.y + 1f;
			}
			if (KeyboardJoystick.player.GetButton(42))
			{
				this.playerDirection.y = this.playerDirection.y - 1f;
			}

			return IRoleAbility.IsCommonUse();
		}

		private bool ability()
		{
			if (this.playerDirection == Vector2.zero)
			{
				this.playerDirection.x = this.playerDirection.x +
					(CachedPlayerControl.LocalPlayer.PlayerControl.cosmetics.FlipX ? -1 : 1);
			}
			// Rpc処理
			CreateBullet(
				this.id,
				this.playerDirection,
				CachedPlayerControl.LocalPlayer);

			++this.id;

			return true;
		}
	}

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

	public enum Option
	{
		InitAbility,

		HandGunCount,
		HandGunSpeed,
		HandGunRange,

		FlameThrowerCount,
		FlameThrowerChargeTime,
		FlameThrowerActiveTime,
		FlameThrowerFireSecond,
		FlameThrowerDeadSecond,

		SwordCount,
		SwordChargeTime,
		SwordActiveTime,
		SwordR,

		SniperRifleCount,
		SniperRifleSpeed,

		BeamRifleCount,
		BeamRifleSpeed,
		BeamRifleRange,

		BeamSaberCount,
		BeamSaberChargeTime,
		BeamSaberRange,
		BeamSaberAutoDetect,
	}

	public enum Ability : byte
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
	private IReadOnlyDictionary<Ability, IWeapon>? weapon;


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
		var initMode = (Ability)OptionManager.Instance.GetValue<int>(
			this.GetRoleOptionId(Option.InitAbility));
		this.createWeapon();

		BehaviorBase init = this.getAbilityBehavior(initMode);

		this.Button = new ExtremeMultiModalAbilityButton(
			[ init ],
			new RoleButtonActivator(),
			KeyCode.F);
	}

	public void RoleAbilityInit()
	{ }

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
		CreateSelectionOption(
			Option.InitAbility,
			Enum.GetValues<Ability>()
				.Select(x => x.ToString())
				.ToArray(),
			parentOps);

		CreateFloatOption(
			RoleAbilityCommonOption.AbilityCoolTime,
			IRoleAbilityMixin.DefaultCoolTime,
			IRoleAbilityMixin.MinCoolTime,
			IRoleAbilityMixin.MaxCoolTime,
			IRoleAbilityMixin.Step,
			parentOps,
			format: OptionUnit.Second);

		CreateIntOption(
			Option.HandGunCount,
			1, 0, 10, 1, parentOps);
		CreateFloatOption(
			Option.HandGunSpeed,
			10.0f, 0.5f, 15.0f, 0.5f, parentOps);
		CreateFloatOption(
			Option.HandGunRange,
			3.5f, 0.1f, 5.0f, 0.1f, parentOps);

		CreateIntOption(
			Option.FlameThrowerCount,
			1, 0, 10, 1, parentOps);
		CreateFloatOption(
			Option.FlameThrowerChargeTime,
			2.0f, 0.1f, 5.0f, 0.1f, parentOps,
			format: OptionUnit.Second);
		CreateFloatOption(
			Option.FlameThrowerActiveTime,
			25.0f, 5.0f, 120.0f, 0.5f, parentOps,
			format: OptionUnit.Second);
		CreateFloatOption(
			Option.FlameThrowerFireSecond,
			3.5f, 0.1f, 5.0f, 0.1f, parentOps,
			format: OptionUnit.Second);
		CreateFloatOption(
			Option.FlameThrowerDeadSecond,
			3.5f, 0.1f, 5.0f, 0.1f, parentOps,
			format: OptionUnit.Second);

		CreateIntOption(
			Option.SwordCount,
			1, 0, 10, 1, parentOps);
		CreateFloatOption(
			Option.SwordChargeTime,
			3.0f, 0.5f, 30.0f, 0.5f, parentOps,
			format: OptionUnit.Second);
		CreateFloatOption(
			Option.SwordActiveTime,
			15.0f, 0.5f, 60.0f, 0.5f, parentOps,
			format: OptionUnit.Second);
		CreateFloatOption(
			Option.SwordR,
			1.0f, 0.25f, 5.0f, 0.25f, parentOps);

		CreateIntOption(
			Option.SniperRifleCount,
			1, 0, 10, 1, parentOps);
		CreateFloatOption(
			Option.SniperRifleSpeed,
			50.0f, 25.0f, 75.0f, 0.5f, parentOps);

		CreateIntOption(
			Option.BeamRifleCount,
			1, 0, 10, 1, parentOps);
		CreateFloatOption(
			Option.BeamRifleSpeed,
			20.0f, 0.5f, 30.0f, 0.5f, parentOps);
		CreateFloatOption(
			Option.BeamRifleSpeed,
			7.0f, 0.1f, 10.0f, 0.1f, parentOps);


		CreateIntOption(
			Option.BeamSaberCount,
			1, 0, 10, 1, parentOps);
		CreateIntOption(
			Option.BeamSaberChargeTime,
			5, 1, 60, 1, parentOps,
			format: OptionUnit.Second);
		CreateFloatOption(
			Option.BeamSaberRange,
			3.5f, 0.1f, 7.5f, 0.1f, parentOps);
		CreateBoolOption(
			Option.BeamSaberAutoDetect,
			false, parentOps);
	}

	protected override void RoleSpecificInit()
	{

	}

	private void createWeapon()
	{
		var mng = OptionManager.Instance;

		this.weapon = new Dictionary<Ability, IWeapon>()
		{
			{
				Ability.HandGun,
				new NormalGun(
					Ability.HandGun,
					new (
						Path.BulletImg,
						new Vector2(0.025f, 0.05f),
						mng.GetValue<float>(
							this.GetRoleOptionId(Option.HandGunSpeed)),
						mng.GetValue<float>(
							this.GetRoleOptionId(Option.HandGunRange))))
			},
			{
				Ability.FlameThrower,
				new FlameThrower(
					mng.GetValue<float>(
						this.GetRoleOptionId(Option.FlameThrowerFireSecond)),
					mng.GetValue<float>(
						this.GetRoleOptionId(Option.FlameThrowerDeadSecond)))
			},
			{
				Ability.Sword,
				new Sword(
					mng.GetValue<float>(
						this.GetRoleOptionId(Option.SwordChargeTime)),
					mng.GetValue<float>(
						this.GetRoleOptionId(Option.SwordActiveTime)),
					mng.GetValue<float>(
						this.GetRoleOptionId(Option.SwordR)))
			},
			{
				Ability.SniperRifle,
				new NormalGun(
					Ability.SniperRifle,
					new (
						Path.BulletImg,
						new Vector2(0.025f, 0.05f),
						mng.GetValue<float>(
							this.GetRoleOptionId(Option.SniperRifleSpeed)),
						128.0f))
			},
			{
				Ability.BeamRifle,
				new NormalGun(
					Ability.BeamRifle,
					new (
						Path.BulletImg,
						new Vector2(0.05f, 0.05f),
						mng.GetValue<float>(
							this.GetRoleOptionId(Option.BeamRifleSpeed)),
						mng.GetValue<float>(
							this.GetRoleOptionId(Option.BeamRifleRange)),
						true))
			},
			{
				Ability.BeamSaber,
				new BeamSaber(
					mng.GetValue<float>(
						this.GetRoleOptionId(Option.BeamSaberRange)),
					mng.GetValue<bool>(
						this.GetRoleOptionId(Option.BeamSaberAutoDetect)))
			}
		};
	}

	private CreateParam createParam(in Ability ability)
		=> ability switch
		{
			_ => new("", Path.TestButton)
		};

	private BehaviorBase getAbilityBehavior(in Ability ability)
	{
		BehaviorBase? result;

		ExtremeRolesPlugin.Logger.LogInfo(ability.ToString());

		if (this.weapon is null ||
			!this.weapon.TryGetValue(ability, out var weapon))
		{
			result = new NullBehaviour();
		}
		else
		{
			var weaponParam = createParam(ability);
			result = weapon.Create(weaponParam);
			this.loadAbilityOption(result, ability);
		}
		ExtremeRolesPlugin.Logger.LogInfo(result.ToString());
		return result;
	}

	private void loadAbilityOption(in BehaviorBase behavior, in Ability ability)
	{
		var mng = OptionManager.Instance;

		float coolTime = mng.GetValue<float>(
			this.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime));

		behavior.SetCoolTime(coolTime);

		switch (ability)
		{
			case Ability.HandGun:
				if (behavior is not CountBehavior handGun)
				{
					throw new ArgumentException("HandGun Behavior is not CountBehavior");
				}
				handGun.SetAbilityCount(
					mng.GetValue<int>(
						this.GetRoleOptionId(Option.HandGunCount)));
				break;
			case Ability.FlameThrower:
				if (behavior is not ChargingAndActivatingCountBehaviour flameThrower)
				{
					throw new ArgumentException("FlameThrower Behavior is not CountBehavior");
				}
				flameThrower.ActiveTime = mng.GetValue<float>(
					this.GetRoleOptionId(Option.FlameThrowerActiveTime));
				flameThrower.ChargeTime = mng.GetValue<float>(
					this.GetRoleOptionId(Option.FlameThrowerChargeTime));
				flameThrower.SetAbilityCount(
					mng.GetValue<int>(
						this.GetRoleOptionId(Option.FlameThrowerCount)));
				break;
			case Ability.Sword:
				if (behavior is not ICountBehavior countBehavior)
				{
					throw new ArgumentException("Sword Behavior is not CountBehavior");
				}
				countBehavior.SetAbilityCount(
					mng.GetValue<int>(
						this.GetRoleOptionId(Option.SwordCount)));
				break;
			case Ability.SniperRifle:
				if (behavior is not CountBehavior sniperRifle)
				{
					throw new ArgumentException("SniperRifle Behavior is not CountBehavior");
				}
				sniperRifle.SetAbilityCount(
					mng.GetValue<int>(
						this.GetRoleOptionId(Option.SniperRifleCount)));
				break;
			case Ability.BeamRifle:
				if (behavior is not CountBehavior beamRifle)
				{
					throw new ArgumentException("BeamRile Behavior is not CountBehavior");
				}
				beamRifle.SetAbilityCount(
					mng.GetValue<int>(
						this.GetRoleOptionId(Option.BeamRifleCount)));
				break;
			case Ability.BeamSaber:
				if (behavior is not ChargingCountBehaviour beamSaber)
				{
					throw new ArgumentException("BeamSaber Behavior is not ChargingCountBehaviour");
				}

				beamSaber.SetAbilityCount(
					mng.GetValue<int>(
						this.GetRoleOptionId(Option.BeamSaberCount)));
				beamSaber.ChargeTime = mng.GetValue<int>(
					this.GetRoleOptionId(Option.BeamSaberChargeTime));

				break;
			case Ability.All:
				// behavior.SetCount(3);
				break;
		}
	}
}
