using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using TMPro;

using ExtremeRoles.Resources;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.ButtonAutoActivator;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using UnityObject = UnityEngine.Object;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;


public sealed class Scavenger : SingleRoleBase, IRoleUpdate, IRoleAbility
{
	private record struct CreateParam(string ButtonName, string Name);

	private interface IWeapon
	{
		public BehaviorBase Create(in Ability abilityType);
		public void Hide();

		protected static Sprite getSprite(in Ability abilityType)
			=> Loader.GetUnityObjectFromPath<Sprite>(
				"F:\\Documents\\UnityProject\\UnityAsset\\ExtremeRoles\\scavenger.asset",
				$"assets/roles/scavenger.{abilityType}.{Path.ButtonIcon}.png");
	}

	private sealed class Sword(
		float chargeTime,
		float activeTime,
		float r) : IWeapon
	{
		private ScavengerSwordBehaviour? showSword;
		private readonly float r = r;
		private readonly float chargeTime = chargeTime;
		private readonly float activeTime = activeTime;

		public BehaviorBase Create(in Ability abilityType)
		{
			var behavior = new ChargingAndActivatingCountBehaviour(
				$"{abilityType}ButtonName",
				IWeapon.getSprite(abilityType),
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
				new ScavengerSwordBehaviour.RotationInfo(
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
				new ScavengerSwordBehaviour.RotationInfo(
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

		private ScavengerSwordBehaviour createSword(
			in PlayerControl rolePlayer)
			=> ScavengerSwordBehaviour.Create(
				this.r, rolePlayer);
	}

	private sealed class Flame(float fireSecond, float fireDeadSecond) : IWeapon
	{
		private readonly float fireSecond = fireSecond;
		private readonly float fireDeadSecond = fireDeadSecond;

		private ScavengerFlameBehaviour? flame;

		public BehaviorBase Create(in Ability abilityType)
		{
			var behavior = new ChargingAndActivatingCountBehaviour(
				$"{abilityType}ButtonName",
				IWeapon.getSprite(abilityType),
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

		private bool isFireThrowerUse(bool isCharge, float chargeGauge)
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

		private ScavengerFlameBehaviour createSword(
			in PlayerControl rolePlayer)
			=> ScavengerFlameBehaviour.Create(
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

		public BehaviorBase Create(in Ability abilityType)
			=> new ChargingCountBehaviour(
				$"{abilityType}ButtonName",
				IWeapon.getSprite(abilityType),
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
		in ScavengerBulletBehaviour.Parameter param) : IWeapon
	{
		private readonly ScavengerBulletBehaviour.Parameter pram = param;
		private readonly Dictionary<int, ScavengerBulletBehaviour> bullet = new();
		private int id = 0;
		private Vector2 playerDirection;
		private Ability type;

		public BehaviorBase Create(in Ability abilityType)
		{
			this.type = abilityType;
			return new CountBehavior(
			   $"{abilityType}ButtonName",
				IWeapon.getSprite(abilityType),
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

			var bullet = ScavengerBulletBehaviour.Create(
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
		IsRandomInitAbility,

		AllowDupe,
		AllowAdvancedWepon,

		InitAbility,

		IsSetWepon,

		SyncWeapon,

		HandGunCount,
		HandGunSpeed,
		HandGunRange,

		FlameCount,
		FlameChargeTime,
		FlameActiveTime,
		FlameFireSecond,
		FlameDeadSecond,

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

		WeaponMixTime,
	}

	public enum Ability : byte
	{
		// 何もないよ
		Null,

		// 落ちてるやつ
		HandGun,
		Flame,
		Sword,

		// HandGun + Sword
		SniperRifle,
		// HandGun + Flame
		BeamRifle,
		// Flame + Sword
		BeamSaber,

		// Flame + Sword + HandGun
		All,
	}

	public Ability InitAbility { get; private set; }

	private IReadOnlyDictionary<Ability, IWeapon>? weapon;

	private ExtremeMultiModalAbilityButton? internalButton;

	private HashSet<Ability> curAbility = new HashSet<Ability>();
	private TextMeshPro? abilityText;
	private Vector2 prevPlayerPos;
	private float timer;
	private float weaponMixTime;
	private static Vector2 defaultPos => new Vector2(100.0f, 100.0f);

	public Scavenger() : base(
		ExtremeRoleId.Scavenger,
		ExtremeRoleType.Impostor,
		ExtremeRoleId.Scavenger.ToString(),
		Palette.ImpostorRed,
		true, false, true, true)
	{ }

	// 全体の武器に関するシステムを構築させておく
	public override SingleRoleBase Clone()
	{
		var newRole = base.Clone();
		var system = ExtremeSystemTypeManager.Instance.CreateOrGet(
			ScavengerAbilityProviderSystem.Type,
			() =>
			{
				var mng = OptionManager.Instance;

				ScavengerAbilityProviderSystem.RandomOption? randOpt = mng.GetValue<bool>(
					this.GetRoleOptionId(Option.IsRandomInitAbility)) ?
					new(mng.GetValue<bool>(
							this.GetRoleOptionId(Option.AllowDupe)),
						mng.GetValue<bool>(
							this.GetRoleOptionId(Option.AllowAdvancedWepon))) : null;

				return new ScavengerAbilityProviderSystem(
					(Ability)mng.GetValue<int>(
						this.GetRoleOptionId(Option.InitAbility)),
					mng.GetValue<bool>(
						this.GetRoleOptionId(Option.IsSetWepon)),
					mng.GetValue<bool>(
						this.GetRoleOptionId(Option.SyncWeapon)),
					randOpt);
			});

		if (newRole is Scavenger scavenger)
		{
			scavenger.InitAbility = system.GetInitWepon();
		}
		return newRole;
	}

	public void CreateAbility()
	{
		this.createWeapon();
		this.curAbility = new HashSet<Ability>();

		if (this.InitAbility is not Ability.Null)
		{
			this.curAbility.Add(this.InitAbility);
		}

		BehaviorBase init = this.getAbilityBehavior(this.InitAbility);

		this.Button = new ExtremeMultiModalAbilityButton(
			[ init ],
			new RoleButtonActivator(),
			KeyCode.F);
	}

	public void AddWepon(in Ability ability)
	{
		if (this.internalButton is null)
		{
			return;
		}
		BehaviorBase newBehavior = this.getAbilityBehavior(ability);
		this.internalButton.Add(newBehavior);
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

		if (rolePlayer == null)
		{
			return;
		}

		// 能力合成
		if (this.prevPlayerPos == defaultPos)
		{
			this.prevPlayerPos = rolePlayer.GetTruePosition();
		}
		var curPos = rolePlayer.GetTruePosition();

		if (this.internalButton?.MultiModalAbilityNum <= 1 ||
			this.prevPlayerPos != curPos ||
			Key.IsAltDown() ||
			IntroCutscene.Instance != null ||
			MeetingHud.Instance != null ||
			ExileController.Instance != null)
		{
			if (this.abilityText != null)
			{
				this.abilityText.gameObject.SetActive(false);
			}
			this.timer = this.weaponMixTime;
			return;
		}

		if (this.abilityText == null)
		{
			this.abilityText = UnityObject.Instantiate(
				FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
				Camera.main.transform, false);
			this.abilityText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
			this.abilityText.enableWordWrapping = false;
			this.abilityText.color = Palette.EnabledColor;
		}

		this.abilityText.text =
			string.Format(
				Translation.GetString("WeaponMixTimeRemain"),
				Mathf.CeilToInt(this.timer));
		this.timer -= Time.fixedDeltaTime;

		if (this.timer < 0.0f)
		{
			this.mixWeapon();
		}
	}

	protected override void CreateSpecificOption(IOptionInfo parentOps)
	{
		CreateFloatOption(
			RoleAbilityCommonOption.AbilityCoolTime,
			IRoleAbilityMixin.DefaultCoolTime,
			IRoleAbilityMixin.MinCoolTime,
			IRoleAbilityMixin.MaxCoolTime,
			IRoleAbilityMixin.Step,
			parentOps,
			format: OptionUnit.Second);

		var randomWepon = CreateBoolOption(
			Option.IsRandomInitAbility,
			false, parentOps);

		CreateBoolOption(
			Option.AllowDupe,
			false, randomWepon);
		CreateBoolOption(
			Option.AllowAdvancedWepon,
			false, randomWepon);

		CreateSelectionOption(
			Option.InitAbility,
			Enum.GetValues<Ability>()
				.Select(x => x.ToString())
				.ToArray(),
			randomWepon,
			invert: true,
			enableCheckOption: parentOps);

		var mapSetOps = CreateBoolOption(
			Option.IsSetWepon,
			true, parentOps);

		CreateBoolOption(
			Option.SyncWeapon,
			true, mapSetOps,
			invert: true,
			enableCheckOption: parentOps);

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
			Option.FlameCount,
			1, 0, 10, 1, parentOps);
		CreateFloatOption(
			Option.FlameChargeTime,
			2.0f, 0.1f, 5.0f, 0.1f, parentOps,
			format: OptionUnit.Second);
		CreateFloatOption(
			Option.FlameActiveTime,
			25.0f, 5.0f, 120.0f, 0.5f, parentOps,
			format: OptionUnit.Second);
		CreateFloatOption(
			Option.FlameFireSecond,
			3.5f, 0.1f, 5.0f, 0.1f, parentOps,
			format: OptionUnit.Second);
		CreateFloatOption(
			Option.FlameDeadSecond,
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
			7.0f, 0.1f, 10.0f, 0.1f, parentOps);
		CreateFloatOption(
			Option.BeamRifleRange,
			20.0f, 0.5f, 30.0f, 0.5f, parentOps);


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

		CreateFloatOption(
			Option.WeaponMixTime,
			3.0f, 0.5f, 25.0f, 0.5f, parentOps,
			format: OptionUnit.Second);
	}

	protected override void RoleSpecificInit()
	{
		this.weaponMixTime = OptionManager.Instance.GetValue<float>(
			this.GetRoleOptionId(Option.WeaponMixTime));
	}

	private void createWeapon()
	{
		var mng = OptionManager.Instance;

		this.weapon = new Dictionary<Ability, IWeapon>()
		{
			{
				Ability.HandGun,
				new NormalGun(
					new (
						Path.ScavengerBulletImg,
						new Vector2(0.025f, 0.05f),
						mng.GetValue<float>(
							this.GetRoleOptionId(Option.HandGunSpeed)),
						mng.GetValue<float>(
							this.GetRoleOptionId(Option.HandGunRange))))
			},
			{
				Ability.Flame,
				new Flame(
					mng.GetValue<float>(
						this.GetRoleOptionId(Option.FlameFireSecond)),
					mng.GetValue<float>(
						this.GetRoleOptionId(Option.FlameDeadSecond)))
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
					new (
						Path.ScavengerBulletImg,
						new Vector2(0.025f, 0.05f),
						mng.GetValue<float>(
							this.GetRoleOptionId(Option.SniperRifleSpeed)),
						128.0f))
			},
			{
				Ability.BeamRifle,
				new NormalGun(
					new (
						Path.ScavengerBulletImg,
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

	private BehaviorBase getAbilityBehavior(in Ability ability)
	{
		BehaviorBase? result;

		ExtremeRolesPlugin.Logger.LogInfo($"Init Weapon: {ability}");

		if (this.weapon is null ||
			!this.weapon.TryGetValue(ability, out var weapon))
		{
			result = new NullBehaviour();
		}
		else
		{
			result = weapon.Create(ability);
			this.loadAbilityOption(result, ability);
		}
		return result;
	}

	private void mixWeapon()
	{
		// 最終進化系
		if (this.curAbility.Count == 3 ||
			this.curAbility.Contains(Ability.BeamSaber) ||
			this.curAbility.Contains(Ability.BeamRifle) ||
			this.curAbility.Contains(Ability.SniperRifle))
		{
			replaceToWeapon(Ability.All);
		}
		else if (
			this.curAbility.Contains(Ability.HandGun) &&
			this.curAbility.Contains(Ability.Sword))
		{
			replaceToWeapon(Ability.SniperRifle);
		}
		else if (
			this.curAbility.Contains(Ability.HandGun) &&
			this.curAbility.Contains(Ability.Flame))
		{
			replaceToWeapon(Ability.BeamRifle);
		}
		else if (
			this.curAbility.Contains(Ability.Sword) &&
			this.curAbility.Contains(Ability.Flame))
		{
			replaceToWeapon(Ability.BeamSaber);
		}
		else
		{
			throw new ArgumentException("Invalid Ability");
		}
	}
	private void replaceToWeapon(in Ability ability)
	{
		if (this.internalButton is null)
		{
			return;
		}
		this.curAbility.Clear();
		this.curAbility.Add(ability);
		var newAbility = getAbilityBehavior(ability);
		this.internalButton.ClearAndAnd(newAbility);
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
			case Ability.Flame:
				if (behavior is not ChargingAndActivatingCountBehaviour flame)
				{
					throw new ArgumentException("Flame Behavior is not CountBehavior");
				}
				flame.ActiveTime = mng.GetValue<float>(
					this.GetRoleOptionId(Option.FlameActiveTime));
				flame.ChargeTime = mng.GetValue<float>(
					this.GetRoleOptionId(Option.FlameChargeTime));
				flame.SetAbilityCount(
					mng.GetValue<int>(
						this.GetRoleOptionId(Option.FlameCount)));
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
