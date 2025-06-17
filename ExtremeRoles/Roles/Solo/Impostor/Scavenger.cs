using ExtremeRoles.Extension.Controller;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using Hazel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityObject = UnityEngine.Object;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;


public sealed class Scavenger : SingleRoleBase, IRoleUpdate, IRoleAbility
{
	public static T GetFromAsset<T>(string path) where T : UnityObject
		=> UnityObjectLoader.LoadFromResources<T>(
			ObjectPath.GetRoleAssetPath(ExtremeRoleId.Scavenger),
			path.ToLower());

	private record struct CreateParam(string ButtonName, string Name);

	private interface IWeapon
	{
		public BehaviorBase Create(in Ability abilityType);
		public void RpcHide();
		public void RpcOps(
			in PlayerControl rolePlayer,
			in MessageReader reader);

		protected static void SimpleRpcOps(Ability type, byte bytedOps)
		{
			var local = PlayerControl.LocalPlayer;
			if (local == null)
			{
				return;
			}

			ExtremeSystemTypeManager.RpcUpdateSystem(
				ScavengerAbilitySystem.Type,
				writer =>
				{
					var pos = local.transform.position;
					writer.Write((byte)ScavengerAbilitySystem.Ops.WeponOps);
					writer.Write(local.PlayerId);
					writer.Write((byte)type);
					writer.Write(pos.x);
					writer.Write(pos.y);
					writer.Write(bytedOps);
				});
		}

		protected static Sprite getSprite(in Ability abilityType)
			=> GetFromAsset<Sprite>($"assets/roles/scavenger.{abilityType}.{ObjectPath.ButtonIcon}.png");
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

		public enum Ops : byte
		{
			Create,
			Start,
			Hide,
		}

		public void RpcOps(
			in PlayerControl rolePlayer,
			in MessageReader reader)
		{
			Ops ops = (Ops)reader.ReadByte();
			switch (ops)
			{
				case Ops.Create:
					this.startSwordChargeOnPlayer(rolePlayer);
					break;
				case Ops.Start:
					startSwordRotation();
					break;
				case Ops.Hide:
					this.hide();
					break;
			}
		}

		public BehaviorBase Create(in Ability abilityType)
		{
			var behavior = new ChargingAndActivatingCountBehaviour(
				TranslationController.Instance.GetString(abilityType.ToString()),
				IWeapon.getSprite(abilityType),
				isSwordUse,
				rpcStartSwordRotation,
				rpcStartSwordCharge,
				ChargingAndActivatingCountBehaviour.ReduceTiming.OnActive,
				isValidSword, isValidSword,
				RpcHide, RpcHide);
			behavior.ChargeTime = chargeTime;
			behavior.ActiveTime = activeTime;
			return behavior;
		}

		public void RpcHide()
		{
			if (this.showSword == null)
			{
				return;
			}
			IWeapon.SimpleRpcOps(Ability.ScavengerSword, (byte)Ops.Hide);
		}

		private bool rpcStartSwordCharge()
		{
			var local = PlayerControl.LocalPlayer;
			if (local == null)
			{
				return false;
			}
			IWeapon.SimpleRpcOps(Ability.ScavengerSword, (byte)Ops.Create);
			return true;
		}

		private bool rpcStartSwordRotation(float _)
		{
			IWeapon.SimpleRpcOps(Ability.ScavengerSword, (byte)Ops.Start);
			return true;
		}

		private void hide()
		{
			if (this.showSword == null)
			{
				return;
			}
			this.showSword.gameObject.SetActive(false);
		}

		private void startSwordChargeOnPlayer(PlayerControl rolePlayer)
		{
			// Rpc処理
			if (this.showSword == null)
			{
				this.showSword = ScavengerSwordBehaviour.Create(
					this.r, rolePlayer);
			}

			this.showSword.gameObject.SetActive(true);
			this.showSword.SetRotation(
				new ScavengerSwordBehaviour.RotationInfo(
					this.chargeTime, -45f, false),
				true);
		}

		private void startSwordRotation()
		{
			if (this.showSword == null)
			{
				return;
			}

			this.showSword.SetRotation(
				new ScavengerSwordBehaviour.RotationInfo(
					this.activeTime, 365f, true),
				false);
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
	}

	public sealed class Flame(float fireSecond, float fireDeadSecond) : IWeapon
	{
		private readonly float fireSecond = fireSecond;
		private readonly float fireDeadSecond = fireDeadSecond;

		public enum Ops
		{
			Create,
			Start,
			Hide,
			FireStart,
			FireEnd,
		}

		private ScavengerFlameBehaviour? flame;
		private readonly Dictionary<byte, ScavengerFlameFire> allFire = new();

		public BehaviorBase Create(in Ability abilityType)
		{
			var behavior = new ChargingAndActivatingCountBehaviour(
				TranslationController.Instance.GetString(abilityType.ToString()),
				IWeapon.getSprite(abilityType),
				isFireThrowerUse,
				rpcStartFlameFire,
				rpcStartFlameCharge,
				ChargingAndActivatingCountBehaviour.ReduceTiming.OnActive,
				IRoleAbility.IsCommonUse,
				IRoleAbility.IsCommonUse,
				RpcHide, RpcHide);
			return behavior;
		}

		public void RpcOps(in PlayerControl rolePlayer, in MessageReader reader)
		{
			Ops ops = (Ops)reader.ReadByte();
			switch (ops)
			{
				case Ops.Create:
					startFlameCharge(rolePlayer);
					break;
				case Ops.Start:
					startFlameFire();
					break;
				case Ops.Hide:
					this.hide();
					break;
				case Ops.FireStart:
					byte startTargetPlayerId = reader.ReadByte();
					fireStart(startTargetPlayerId, rolePlayer.PlayerId);
					break;
				case Ops.FireEnd:
					byte endTargetPlayerId = reader.ReadByte();
					fireEnd(endTargetPlayerId);
					break;
				default:
					break;
			}
		}

		public bool TryGetFire(byte playerId, [NotNullWhen(true)] out ScavengerFlameFire? fire)
			=> this.allFire.TryGetValue(playerId, out fire) && fire != null;

		public void RpcHide()
		{
			if (this.flame == null)
			{
				return;
			}
			IWeapon.SimpleRpcOps(Ability.ScavengerFlame, (byte)Ops.Hide);
		}

		private bool rpcStartFlameCharge()
		{
			var local = PlayerControl.LocalPlayer;
			if (local == null)
			{
				return false;
			}
			IWeapon.SimpleRpcOps(Ability.ScavengerFlame, (byte)Ops.Create);
			return true;
		}

		private void startFlameCharge(PlayerControl player)
		{
			// Rpc処理
			if (this.flame == null)
			{
				this.flame = ScavengerFlameBehaviour.Create(
					this.fireSecond, player);
			}

			this.flame.StartCharge();
			this.flame.gameObject.SetActive(true);
		}

		private bool rpcStartFlameFire(float _)
		{
			IWeapon.SimpleRpcOps(Ability.ScavengerFlame, (byte)Ops.Start);
			return true;
		}

		private void startFlameFire()
		{
			if (this.flame == null)
			{
				return;
			}
			this.flame.Fire();
		}

		private void fireStart(byte playerId, byte rolePlayerId)
		{
			if (!this.allFire.TryGetValue(playerId, out var fire) ||
				fire == null)
			{
				var player = Player.GetPlayerControlById(playerId);
				if (player == null)
				{
					return;
				}

				var obj = UnityObject.Instantiate(
					GetFromAsset<GameObject>("Scavenger.FlameFire.prefab"),
					player.transform);
				fire = obj.GetComponent<ScavengerFlameFire>();
				fire.DeadTime = this.fireDeadSecond;
				fire.IgnorePlayerId = rolePlayerId;
				fire.TargetPlayer = player;
				this.allFire.Add(playerId, fire);
			}
			fire.gameObject.SetActive(true);
		}
		private void fireEnd(byte playerId)
		{
			if (!this.allFire.TryGetValue(playerId, out var fire) ||
				fire == null)
			{
				return;
			}
			fire.gameObject.SetActive(false);
		}

		private void hide()
		{
			if (this.flame == null)
			{
				return;
			}
			this.flame.gameObject.SetActive(false);
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
				TranslationController.Instance.GetString(abilityType.ToString()),
				IWeapon.getSprite(abilityType),
				isIaiOk,
				tryIai,
				startIai,
				ChargingCountBehaviour.ReduceTiming.OnActive);

		public void RpcHide()
		{ }

		private bool startIai()
		{
			this.cacheResult.Clear();
			this.targetPlayerId = byte.MaxValue;
			this.chargePos = PlayerControl.LocalPlayer.GetTruePosition();
			return true;
		}

		private bool tryIai(float chargeGauge)
		{
			if (this.targetPlayerId == byte.MaxValue)
			{
				return this.isIgnoreAutoDetect;
			}


			Player.RpcUncheckMurderPlayer(
				PlayerControl.LocalPlayer.PlayerId,
				this.targetPlayerId,
				byte.MinValue);
			Sound.PlaySound(Sound.Type.Kill, 0.7f);

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

			var curPos = PlayerControl.LocalPlayer.GetTruePosition();
			if (curPos != this.chargePos)
			{
				return false;
			}

			float searchRange = this.range * chargeGauge * chargeGauge;

			var allPlayer = GameData.Instance.AllPlayers;
			this.cacheResult.Clear();

			if (!ShipStatus.Instance)
			{
				return false;
			}

			PlayerControl pc = PlayerControl.LocalPlayer;
			Vector2 truePosition = pc.GetTruePosition();
			var role = ExtremeRoleManager.GetLocalPlayerRole();

			foreach (var playerInfo in
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

		public void RpcOps(in PlayerControl rolePlayer, in MessageReader reader)
		{ }
	}

	private sealed class Gun(
		in ScavengerBulletBehaviour.Parameter param) : IWeapon
	{
		private readonly ScavengerBulletBehaviour.Parameter pram = param;
		private readonly Dictionary<int, ScavengerBulletBehaviour> bullet = new();
		private int id = 0;
		private Vector2 playerDirection;
		private Ability type;

		public enum Ops
		{
			Create,
			HideToId,
			HideAll,
		}

		public void RpcOps(
			in PlayerControl rolePlayer,
			in MessageReader reader)
		{
			Ops ops = (Ops)reader.ReadByte();
			switch (ops)
			{
				case Ops.Create:
					int id = reader.ReadPackedInt32();
					float x = reader.ReadSingle();
					float y = reader.ReadSingle();
					this.createBullet(id, new (x, y), rolePlayer);
					break;
				case Ops.HideToId:
					int hideId = reader.ReadPackedInt32();
					this.hide(hideId);
					break;
				case Ops.HideAll:
					this.hide();
					break;
			}
		}

		public BehaviorBase Create(in Ability abilityType)
		{
			this.type = abilityType;
			return new CountBehavior(
				TranslationController.Instance.GetString(abilityType.ToString()),
				IWeapon.getSprite(abilityType),
				isUse,
				ability,
				forceAbilityOff: RpcHide);
		}

		public void RpcHide()
		{
			var local = PlayerControl.LocalPlayer;
			if (local == null || this.bullet.Count <= 0)
			{
				return;
			}

			ExtremeSystemTypeManager.RpcUpdateSystem(
				ScavengerAbilitySystem.Type,
				writer =>
				{
					var pos = local.transform.position;
					writer.Write((byte)ScavengerAbilitySystem.Ops.WeponOps);
					writer.Write(local.PlayerId);
					writer.Write((byte)this.type);
					writer.Write(pos.x);
					writer.Write(pos.y);
					writer.Write((byte)Ops.HideAll);
				});
		}

		public void RpcHideToId(int id)
		{
			var local = PlayerControl.LocalPlayer;
			if (local == null)
			{
				return;
			}

			ExtremeSystemTypeManager.RpcUpdateSystem(
				ScavengerAbilitySystem.Type,
				writer =>
				{
					var pos = local.transform.position;
					writer.Write((byte)ScavengerAbilitySystem.Ops.WeponOps);
					writer.Write(local.PlayerId);
					writer.Write((byte)this.type);
					writer.Write(pos.x);
					writer.Write(pos.y);
					writer.Write((byte)Ops.HideToId);
					writer.WritePacked(id);
				});
		}

		private void hide(int id)
		{
			if (this.bullet.TryGetValue(id, out var bullet) &&
				bullet != null &&
				bullet.gameObject != null)
			{
				UnityObject.Destroy(bullet.gameObject);
			}
			this.bullet.Remove(id);
		}

		private void hide()
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
					(PlayerControl.LocalPlayer.cosmetics.FlipX ? -1 : 1);
			}

			var local = PlayerControl.LocalPlayer;
			if (local == null)
			{
				return false;
			}

			ExtremeSystemTypeManager.RpcUpdateSystem(
				ScavengerAbilitySystem.Type,
				writer =>
				{
					var pos = local.transform.position;
					writer.Write((byte)ScavengerAbilitySystem.Ops.WeponOps);
					writer.Write(local.PlayerId);
					writer.Write((byte)this.type);
					writer.Write(pos.x);
					writer.Write(pos.y);
					writer.Write((byte)Ops.Create);
					writer.WritePacked(this.id);
					writer.Write(this.playerDirection.x);
					writer.Write(this.playerDirection.y);
				});
			++this.id;
			return true;
		}

		private void createBullet(
			int mngId,
			Vector2 direction,
			in PlayerControl? rolePlayer)
		{
			if (rolePlayer == null)
			{
				throw new ArgumentNullException("RolePlayer is null");
			}

			var bullet = ScavengerBulletBehaviour.Create(
				mngId,
				this.type,
				rolePlayer,
				direction,
				this.pram);

			this.bullet.Add(mngId, bullet);
		}
	}

	/*
	private sealed class Aguni : IWeapon
	{
		// TODO: ちゃんとしたやつに変更する
		private MonoBehaviour? aguniBehaviour;

		private Vector2 chargePos = Vector2.zero;

		public enum Ops : byte
		{
			Charge,
			Fire,
			Hide,
		}

		public BehaviorBase Create(in Ability abilityType)
		{
			var behavior = new ChargingAndActivatingCountBehaviour(
				$"{abilityType}ButtonName",
				IWeapon.getSprite(abilityType),
				isAguniFire,
				rpcStartAguniFire,
				rpcStartAguniCharge,
				ChargingAndActivatingCountBehaviour.ReduceTiming.OnActive,
				isAguniChargeCheck, null,
				RpcHide, RpcHide);
			behavior.ActiveTime = 5.0f;
			return behavior;
		}

		private bool isAguniChargeCheck()
		{
			var curPos = PlayerControl.LocalPlayer.GetTruePosition();
			return curPos == this.chargePos;
		}

		public void RpcHide()
		{
			IWeapon.SimpleRpcOps(Ability.Aguni, (byte)Ops.Hide);
		}

		public void RpcOps(in PlayerControl rolePlayer, in MessageReader reader)
		{
			Ops ops = (Ops)reader.ReadByte();
			switch (ops)
			{
				case Ops.Charge:
					this.startAguniCharge(rolePlayer);
					break;
				case Ops.Fire:
					fireAguni();
					break;
				case Ops.Hide:
					this.hide();
					break;
				default:
					break;
			}
		}

		private bool isAguniFire(bool isCharge, float chargeGauge)
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
			var curPos = PlayerControl.LocalPlayer.GetTruePosition();
			return curPos == this.chargePos;
		}

		private bool rpcStartAguniFire(float _)
		{
			IWeapon.SimpleRpcOps(Ability.Aguni, (byte)Ops.Fire);
			return true;
		}

		private bool rpcStartAguniCharge()
		{
			var local = PlayerControl.LocalPlayer;
			if (local == null)
			{
				return false;
			}
			this.chargePos = local.GetTruePosition();
			IWeapon.SimpleRpcOps(Ability.Aguni, (byte)Ops.Charge);
			return true;
		}

		private void startAguniCharge(PlayerControl player)
		{
			// Rpc処理
			if (this.aguniBehaviour == null)
			{

			}

			// チャージ開始(0いれる)
			this.aguniBehaviour.gameObject.SetActive(true);
		}

		private void fireAguni()
		{
			if (this.aguniBehaviour == null)
			{
				return;
			}
			// 撃つ
		}

		private void hide()
		{
			if (this.aguniBehaviour == null)
			{
				return;
			}
			this.aguniBehaviour.gameObject.SetActive(false);
		}
	}
	*/

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
		AllowAdvancedWeapon,

		InitAbility,

		IsSetWeapon,

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

		AguniCount,
		AguniChargeTime,

		WeaponMixTime,
	}

	public enum Ability : byte
	{
		// 何もないよ
		ScavengerNull,

		// 落ちてるやつ
		ScavengerHandGun,
		ScavengerFlame,
		ScavengerSword,

		// HandGun + Sword
		ScavengerSniperRifle,
		// HandGun + Flame
		ScavengerBeamRifle,
		// Flame + Sword
		ScavengerBeamSaber,

		// Flame + Sword + HandGun
		// Aguni,
	}

	public Ability InitAbility { get; private set; }
	public Flame FlameWepon
	{
		get
		{
			if (this.weapon is null ||
				!this.weapon.TryGetValue(Ability.ScavengerFlame, out var wepon) ||
				wepon is not Flame flame)
			{
				throw new InvalidOperationException("Flame wepon is null");
			}
			return flame;
		}
	}

	private IReadOnlyDictionary<Ability, IWeapon>? weapon;

	private ExtremeMultiModalAbilityButton? internalButton;

	private HashSet<Ability> curAbility = new HashSet<Ability>();
	private TextMeshPro? abilityText;
	private Vector2? prevPlayerPos;
	private float timer;
	private float weaponMixTime;

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
			ScavengerAbilitySystem.Type,
			() =>
			{
				var loader = this.Loader;

				ScavengerAbilitySystem.RandomOption? randOpt = loader.GetValue<Option, bool>(Option.IsRandomInitAbility) ?
					new(loader.GetValue<Option, bool>(Option.AllowDupe),
						loader.GetValue<Option, bool>(Option.AllowAdvancedWeapon)) : null;

				return new ScavengerAbilitySystem(
					(Ability)loader.GetValue<Option, int>(Option.InitAbility),
					loader.GetValue<Option, bool>(Option.IsSetWeapon),
					loader.GetValue<Option, bool>(Option.SyncWeapon),
					randOpt);
			});

		if (newRole is Scavenger scavenger)
		{
			scavenger.InitAbility = system.GetInitWepon();
		}
		return newRole;
	}

	public static void HideGunId(
		in Ability gunType,
		int id)
	{
		if (!ExtremeRoleManager.TryGetSafeCastedLocalRole<Scavenger>(out var scavenger) ||
			scavenger.weapon is null ||
			scavenger.weapon.TryGetValue(gunType, out var weapon) ||
			weapon is not Gun gun)
		{
			return;
		}
		gun.RpcHideToId(id);
	}

	public void CreateAbility()
	{
		this.createWeapon();
		this.curAbility = new HashSet<Ability>();

		if (this.InitAbility is not Ability.ScavengerNull)
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
		if (this.internalButton.Behavior is NullBehaviour)
		{
			this.internalButton.OnMeetingEnd();
			this.internalButton.ClearAndAnd(newBehavior);
		}
		else
		{
			this.internalButton.Add(newBehavior);
		}
		this.internalButton.SetButtonShow(true);
		this.curAbility.Add(ability);
	}

	public void WeaponOps(
		in Ability ability,
		in PlayerControl rolePlayer,
		in MessageReader reader)
	{
		if (this.weapon is null ||
			!this.weapon.TryGetValue(ability, out var weapon) ||
			weapon is null)
		{
			return;
		}
		weapon.RpcOps(rolePlayer, reader);
	}

	public void RoleAbilityInit()
	{
		if (this.weapon is null)
		{
			this.createWeapon();
		}
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{}

	public override void RolePlayerKilledAction(
		PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		if (rolePlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId)
		{
			return;
		}
		this.reset();
	}

	public void ResetOnMeetingStart()
	{
		this.reset();
	}

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
		if (!this.prevPlayerPos.HasValue)
		{
			this.prevPlayerPos = rolePlayer.GetTruePosition();
		}
		var curPos = rolePlayer.GetTruePosition();

		if (this.internalButton is null ||
			this.internalButton.MultiModalAbilityNum <= 1 ||
			!this.internalButton.IsAbilityReady() ||
			this.prevPlayerPos.Value != curPos ||
			!Key.IsAltDown() ||
			IntroCutscene.Instance != null ||
			MeetingHud.Instance != null ||
			ExileController.Instance != null)
		{
			this.prevPlayerPos = curPos;
			if (this.abilityText != null)
			{
				this.abilityText.gameObject.SetActive(false);
			}
			this.timer = this.weaponMixTime;
			return;
		}

		this.prevPlayerPos = curPos;
		if (this.abilityText == null)
		{
			this.abilityText = UnityObject.Instantiate(
				HudManager.Instance.KillButton.cooldownTimerText,
				Camera.main.transform, false);
			this.abilityText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
			this.abilityText.enableWordWrapping = false;
			this.abilityText.color = Palette.EnabledColor;
		}

		this.abilityText.text = TranslationController.Instance.GetString(
			"WeaponMixTimeRemain", Mathf.CeilToInt(this.timer));
		this.abilityText.gameObject.SetActive(true);
		this.timer -= Time.deltaTime;

		if (this.timer < 0.0f)
		{
			this.mixWeapon();
			this.timer = this.weaponMixTime;
		}
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateFloatOption(
			RoleAbilityCommonOption.AbilityCoolTime,
			IRoleAbility.DefaultCoolTime,
			IRoleAbility.MinCoolTime,
			IRoleAbility.MaxCoolTime,
			IRoleAbility.Step,
			format: OptionUnit.Second);

		var randomWepon = factory.CreateBoolOption(
			Option.IsRandomInitAbility,
			false);

		factory.CreateBoolOption(
			Option.AllowDupe,
			false, randomWepon);
		factory.CreateBoolOption(
			Option.AllowAdvancedWeapon,
			false, randomWepon);

		factory.CreateSelectionOption(
			Option.InitAbility,
			Enum.GetValues<Ability>()
				.Select(x => x.ToString())
				.ToArray(),
			randomWepon,
			invert: true);

		var mapSetOps = factory.CreateBoolOption(
			Option.IsSetWeapon, true);

		factory.CreateBoolOption(
			Option.SyncWeapon,
			true, mapSetOps,
			invert: true);

		factory.CreateIntOption(
			Option.HandGunCount,
			1, 0, 10, 1);
		factory.CreateFloatOption(
			Option.HandGunSpeed,
			10.0f, 0.5f, 15.0f, 0.5f);
		factory.CreateFloatOption(
			Option.HandGunRange,
			3.5f, 0.1f, 5.0f, 0.1f);

		factory.CreateIntOption(
			Option.FlameCount,
			1, 0, 10, 1);
		factory.CreateFloatOption(
			Option.FlameChargeTime,
			2.0f, 0.1f, 5.0f, 0.1f,
			format: OptionUnit.Second);
		factory.CreateFloatOption(
			Option.FlameActiveTime,
			25.0f, 5.0f, 120.0f, 0.5f,
			format: OptionUnit.Second);
		factory.CreateFloatOption(
			Option.FlameFireSecond,
			3.5f, 0.1f, 5.0f, 0.1f,
			format: OptionUnit.Second);
		factory.CreateFloatOption(
			Option.FlameDeadSecond,
			3.5f, 0.1f, 5.0f, 0.1f,
			format: OptionUnit.Second);

		factory.CreateIntOption(
			Option.SwordCount,
			1, 0, 10, 1);
		factory.CreateFloatOption(
			Option.SwordChargeTime,
			3.0f, 0.5f, 30.0f, 0.5f,
			format: OptionUnit.Second);
		factory.CreateFloatOption(
			Option.SwordActiveTime,
			15.0f, 0.5f, 60.0f, 0.5f,
			format: OptionUnit.Second);
		factory.CreateFloatOption(
			Option.SwordR,
			1.0f, 0.25f, 5.0f, 0.25f);

		factory.CreateIntOption(
			Option.SniperRifleCount,
			1, 0, 10, 1);
		factory.CreateFloatOption(
			Option.SniperRifleSpeed,
			50.0f, 25.0f, 75.0f, 0.5f);

		factory.CreateIntOption(
			Option.BeamRifleCount,
			1, 0, 10, 1);
		factory.CreateFloatOption(
			Option.BeamRifleSpeed,
			7.0f, 0.1f, 10.0f, 0.1f);
		factory.CreateFloatOption(
			Option.BeamRifleRange,
			20.0f, 0.5f, 30.0f, 0.5f);


		factory.CreateIntOption(
			Option.BeamSaberCount,
			1, 0, 10, 1);
		factory.CreateIntOption(
			Option.BeamSaberChargeTime,
			5, 1, 60, 1,
			format: OptionUnit.Second);
		factory.CreateFloatOption(
			Option.BeamSaberRange,
			3.5f, 0.1f, 7.5f, 0.1f);
		factory.CreateBoolOption(
			Option.BeamSaberAutoDetect,
			false);

		/*
		factory.CreateIntOption(
			Option.AguniCount,
			1, 0, 10, 1);
		factory.CreateIntOption(
			Option.AguniChargeTime,
			5, 1, 60, 1,
			format: OptionUnit.Second);
		*/
		factory.CreateFloatOption(
			Option.WeaponMixTime,
			3.0f, 0.5f, 25.0f, 0.5f,
			format: OptionUnit.Second);
	}

	protected override void RoleSpecificInit()
	{
		this.weaponMixTime = this.Loader.GetValue<Option, float>(Option.WeaponMixTime);
	}

	private void createWeapon()
	{
		var loader = this.Loader;

		this.weapon = new Dictionary<Ability, IWeapon>()
		{
			{
				Ability.ScavengerHandGun,
				new Gun(
					new (
						ObjectPath.ScavengerBulletImg,
						new Vector2(0.025f, 0.05f),
						loader.GetValue<Option, float>(Option.HandGunSpeed),
						loader.GetValue<Option, float>(Option.HandGunRange)))
			},
			{
				Ability.ScavengerFlame,
				new Flame(
					loader.GetValue<Option, float>(Option.FlameFireSecond),
					loader.GetValue<Option, float>(Option.FlameDeadSecond))
			},
			{
				Ability.ScavengerSword,
				new Sword(
					loader.GetValue<Option, float>(Option.SwordChargeTime),
					loader.GetValue<Option, float>(Option.SwordActiveTime),
					loader.GetValue<Option, float>(Option.SwordR))
			},
			{
				Ability.ScavengerSniperRifle,
				new Gun(
					new (
						ObjectPath.ScavengerBulletImg,
						new Vector2(0.025f, 0.05f),
						loader.GetValue<Option, float>(Option.SniperRifleSpeed),
						128.0f))
			},
			{
				Ability.ScavengerBeamRifle,
				new Gun(
					new (
						ObjectPath.ScavengerBeamImg,
						new Vector2(0.05f, 0.05f),
						loader.GetValue<Option, float>(Option.BeamRifleSpeed),
						loader.GetValue<Option, float>(Option.BeamRifleRange),
						true))
			},
			{
				Ability.ScavengerBeamSaber,
				new BeamSaber(
					loader.GetValue<Option, float>(Option.BeamSaberRange),
					loader.GetValue<Option, bool>(Option.BeamSaberAutoDetect))
			},
			/*
			{
				Ability.Aguni,
				new Aguni()
			}
			*/
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

		float coolTime = this.Loader.GetValue<RoleAbilityCommonOption, float>(
			RoleAbilityCommonOption.AbilityCoolTime);
		result.SetCoolTime(coolTime);

		return result;
	}

	private void mixWeapon()
	{
		/*
		// 最終進化系
		if (this.curAbility.Count == 3 ||
			this.curAbility.Contains(Ability.BeamSaber) ||
			this.curAbility.Contains(Ability.BeamRifle) ||
			this.curAbility.Contains(Ability.SniperRifle))
		{
			replaceToWeapon(Ability.Aguni);
		}
		else */
		if (
			this.curAbility.Contains(Ability.ScavengerHandGun) &&
			this.curAbility.Contains(Ability.ScavengerSword))
		{
			replaceToWeapon(Ability.ScavengerSniperRifle);
		}
		else if (
			this.curAbility.Contains(Ability.ScavengerHandGun) &&
			this.curAbility.Contains(Ability.ScavengerFlame))
		{
			replaceToWeapon(Ability.ScavengerBeamRifle);
		}
		else if (
			this.curAbility.Contains(Ability.ScavengerSword) &&
			this.curAbility.Contains(Ability.ScavengerFlame))
		{
			replaceToWeapon(Ability.ScavengerBeamSaber);
		}
		else
		{
			ExtremeRolesPlugin.Logger.LogWarning("Invalid Ability");
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
		var loader = this.Loader;

		switch (ability)
		{
			case Ability.ScavengerHandGun:
				if (behavior is not CountBehavior handGun)
				{
					throw new ArgumentException("HandGun Behavior is not CountBehavior");
				}
				handGun.SetAbilityCount(
					loader.GetValue<Option, int>(
						Option.HandGunCount));
				break;
			case Ability.ScavengerFlame:
				if (behavior is not ChargingAndActivatingCountBehaviour flame)
				{
					throw new ArgumentException("Flame Behavior is not CountBehavior");
				}
				flame.ActiveTime = loader.GetValue<Option, float>(Option.FlameActiveTime);
				flame.ChargeTime = loader.GetValue<Option, float>(Option.FlameChargeTime);
				flame.SetAbilityCount(
					loader.GetValue<Option, int>(Option.FlameCount));
				break;
			case Ability.ScavengerSword:
				if (behavior is not ICountBehavior countBehavior)
				{
					throw new ArgumentException("Sword Behavior is not CountBehavior");
				}
				countBehavior.SetAbilityCount(
					loader.GetValue<Option, int>(Option.SwordCount));
				break;
			case Ability.ScavengerSniperRifle:
				if (behavior is not CountBehavior sniperRifle)
				{
					throw new ArgumentException("SniperRifle Behavior is not CountBehavior");
				}
				sniperRifle.SetAbilityCount(
					loader.GetValue<Option, int>(Option.SniperRifleCount));
				break;
			case Ability.ScavengerBeamRifle:
				if (behavior is not CountBehavior beamRifle)
				{
					throw new ArgumentException("BeamRile Behavior is not CountBehavior");
				}
				beamRifle.SetAbilityCount(
					loader.GetValue<Option, int>(Option.BeamRifleCount));
				break;
			case Ability.ScavengerBeamSaber:
				if (behavior is not ChargingCountBehaviour beamSaber)
				{
					throw new ArgumentException("BeamSaber Behavior is not ChargingCountBehaviour");
				}

				beamSaber.SetAbilityCount(
					loader.GetValue<Option, int>(Option.BeamSaberCount));
				beamSaber.ChargeTime = loader.GetValue<Option, int>(Option.BeamSaberChargeTime);

				break;
			/*
			case Ability.Aguni:
				if (behavior is not ChargingAndActivatingCountBehaviour aguni)
				{
					throw new ArgumentException("Aguni Behavior is not ChargingAndActivatingCountBehaviour");
				}
				aguni.ChargeTime = loader.GetValue<Option, float>(Option.AguniChargeTime);

				break;
			*/
			default:
				break;
		}
		if (this.internalButton != null &&
			this.internalButton.Transform.TryGetComponent<ActionButton>(out var button))
		{
			behavior.Initialize(button);
		}
	}

	private void reset()
	{
		if (this.abilityText != null)
		{
			this.abilityText.gameObject.SetActive(true);
		}
		if (this.weapon is null)
		{
			return;
		}
		foreach (var weapon in this.weapon.Values)
		{
			weapon.RpcHide();
		}
	}
}
