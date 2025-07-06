using System.Collections.Generic;

using UnityEngine;
using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Resources;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;

using NullException = System.ArgumentNullException;
using Scavenger = ExtremeRoles.Roles.Solo.Impostor.Scavenger;
using Ptr = System.IntPtr;


#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

public static class  ScavengerWeaponHitHelper
{
	private static readonly int collisionLayer = LayerMask.GetMask("Ship", "Objects" );

	public static bool IsHitPlayer(Collider2D other, out PlayerControl pc)
		=> other.TryGetComponent(out pc) &&
			pc != null &&
			pc.Data != null &&
			!pc.Data.IsDead &&
			!pc.Data.Disconnected &&
			!pc.inVent;

	public static bool IsHitWall(Collider2D other)
		=> collisionLayer == (collisionLayer | (1 << other.gameObject.layer));
}

[Il2CppRegister([typeof(IUsable)])]
public sealed class ScavengerWeponMapUsable : MonoBehaviour, IAmongUs.IUsable
{
	public readonly record struct Info(Scavenger.Ability Ability, bool IsSync);

	[HideFromIl2Cpp]
	public Info WeponInfo
	{
		private get => weponInfo;
		set
		{
			weponInfo = value;
			var rend = base.gameObject.TryAddComponent<SpriteRenderer>();
			rend.sprite = Scavenger.GetFromAsset<Sprite>(
				$"{weponInfo.Ability}.{ObjectPath.MapIcon}.png");

			var collider = base.gameObject.TryAddComponent<CircleCollider2D>();
			collider.isTrigger = true;
			collider.radius = 0.025f;
		}
	}
	private Info weponInfo;

	public float UsableDistance => 0.5f;

	public float PercentCool => 0.0f;

	public ImageNames UseIcon => ImageNames.UseButton;

	public ScavengerWeponMapUsable(Ptr ptr) : base(ptr) { }

	public float CanUse(NetworkedPlayerInfo pc, out bool canUse, out bool couldUse)
	{
		float num = Vector2.Distance(
			pc.Object.GetTruePosition(),
			base.transform.position);
		var scavenger = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<Scavenger>();
		couldUse = pc.IsDead || scavenger is null ? false : true;
		canUse = (couldUse && num <= this.UsableDistance);
		return num;
	}

	public void SetOutline(bool on, bool mainTarget)
	{ }

	public void Use()
	{
		var scavenger = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<Scavenger>();
		if (scavenger is null)
		{
			return;
		}

		var ability = this.WeponInfo.Ability;
		if (this.WeponInfo.IsSync)
		{
			ExtremeSystemTypeManager.RpcUpdateSystem(
				ScavengerAbilitySystem.Type,
				x =>
				{
					x.Write((byte)ScavengerAbilitySystem.Ops.PickUp);
					x.Write((byte)ability);
				});
		}
		scavenger.AddWepon(ability);
	}
}


[Il2CppRegister]
public sealed class ScavengerBulletBehaviour : MonoBehaviour
{
	public sealed record Parameter(
		string Img,
		Vector2 Size,
		float Speed,
		float Range,
		bool IsWallHack = false);

	private float range;
	private byte ignorePlayerId;
	private int id;
	private Scavenger.Ability ability;
	private Vector2 initialPosition;
	private bool isWallHack = false;

	private BoxCollider2D? collider;
	private SpriteRenderer? renderer;

	public ScavengerBulletBehaviour(Ptr ptr) : base(ptr) { }

	public static ScavengerBulletBehaviour Create(
		int id,
		Scavenger.Ability ability,
		in PlayerControl abilityPlayer,
		Vector2 direction,
		in Parameter param)
	{
		var obj = new GameObject($"Bullet_{id}");
		obj.transform.position = abilityPlayer.transform.position;
		obj.layer = Constants.LivingPlayersOnlyMask;

		var bullet = obj.AddComponent<ScavengerBulletBehaviour>();
		bullet.Initialize(
			id, ability,
			param.Img,
			param.Size,
			direction,
			param.Speed,
			param.Range,
			abilityPlayer.PlayerId,
			param.IsWallHack);

		return bullet;
	}

	[HideFromIl2Cpp]
	public void Initialize(
		int id,
		Scavenger.Ability ability,
		in string bulletImg,
		Vector2 size,
		Vector2 direction,
		in float speed,
		in float range,
		in byte ignorePlayerId,
		in bool isWallHack)
	{

		this.collider = this.gameObject.AddComponent<BoxCollider2D>();
		this.collider.isTrigger = true;
		this.collider.size = size;

		this.range = range;
		this.ignorePlayerId = ignorePlayerId;

		this.renderer = this.gameObject.AddComponent<SpriteRenderer>();
		this.renderer.sprite = Scavenger.GetFromAsset<Sprite>($"{bulletImg}.png");

		this.initialPosition = transform.position;

		this.transform.LookAt2d(this.initialPosition + direction);

		var rb = this.gameObject.AddComponent<Rigidbody2D>();
		rb.velocity = direction.normalized * speed;
		rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		rb.isKinematic = true;

		this.id = id;
		this.ability = ability;

		this.isWallHack = isWallHack;
	}

	public void OnDestroy()
	{
		if (this.renderer != null)
		{
			Destroy(this.renderer);
		}
	}

	public void Update()
	{
		if (Vector2.Distance(this.transform.position, this.initialPosition) > this.range)
		{
			hide();
		}
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		if (PlayerControl.LocalPlayer.PlayerId != this.ignorePlayerId)
		{
			return;
		}

		if (ScavengerWeaponHitHelper.IsHitPlayer(other, out var pc) &&
			pc.PlayerId != this.ignorePlayerId)
		{
			Player.RpcUncheckMurderPlayer(
				this.ignorePlayerId,
				pc.PlayerId,
				byte.MinValue);
			hide();
		}
		else if (ScavengerWeaponHitHelper.IsHitWall(other) && !this.isWallHack)
		{
			hide();
		}
	}
	private void hide()
	{
		if (this.collider != null)
		{
			this.collider.enabled = false;
		}
		Scavenger.HideGunId(this.ability, this.id);
	}
}

[Il2CppRegister(typeof(IUsable))]
public sealed class ScavengerSwordBehaviour : MonoBehaviour
{
	public sealed class RotationInfo
	{
		public float Time { get; private set; }
		public float DeltaAnglePerSec { get; private set; }
		public bool IsActive { get; private set; }

		public RotationInfo(
			in float time,
			in float angle,
			in bool isActive)
		{
			this.Time = time;
			this.DeltaAnglePerSec = angle / time;
			this.IsActive = isActive;
		}
		public void Update(in float deltaTime)
		{
			this.Time -= deltaTime;
		}
	}

	public ImageNames UseIcon => ImageNames.UseButton;
	public float UsableDistance => 0.5f;
	public float PercentCool => 0.0f;

	private byte ignorePlayerId;

	private GameObject? anchor;
	private SpriteRenderer? renderer;

	private RotationInfo? rotationInfo;
	public ScavengerSwordBehaviour(Ptr ptr) : base(ptr) { }

	public static ScavengerSwordBehaviour Create(
		in float r,
		in PlayerControl anchorPlayer)
	{
		var obj = new GameObject($"Sword_{anchorPlayer.PlayerId}");
		obj.transform.position = anchorPlayer.transform.position - new Vector3(
			r + 0.5f, 0.0f, 0.0f);
		obj.layer = Constants.LivingPlayersOnlyMask;

		var sword = obj.AddComponent<ScavengerSwordBehaviour>();
		sword.initialize(
			new Vector2(0.815f, 0.08f),
			anchorPlayer);
		return sword;
	}

	[HideFromIl2Cpp]
	public void SetRotation(in RotationInfo rotation, bool isReset)
	{
		if (isReset && this.anchor != null)
		{
			this.anchor.transform.rotation = Quaternion.identity;
		}
		this.rotationInfo = rotation;
	}

	public float CanUse(
		NetworkedPlayerInfo pc, out bool canUse, out bool couldUse)
	{
		float num = Vector2.Distance(
			pc.Object.GetTruePosition(),
			base.transform.position);
		couldUse = pc.IsDead ? false : true;
		canUse = (couldUse && num <= this.UsableDistance) && pc.PlayerId != this.ignorePlayerId;
		return num;
	}

	public void SetOutline(bool on, bool mainTarget)
	{ }

	public void Use()
	{
		Player.RpcUncheckMurderPlayer(
			PlayerControl.LocalPlayer.PlayerId,
			this.ignorePlayerId,
			byte.MinValue);
		this.gameObject.SetActive(false);
	}

	public void Update()
	{
		if (this.anchor == null ||
			this.rotationInfo is null)
		{
			return;
		}

		if (this.rotationInfo.Time <= 0)
		{
			if (this.rotationInfo.IsActive)
			{
				this.gameObject.SetActive(false);
			}
			else
			{
				this.rotationInfo = null;
				return;
			}
		}

		float deltaTime = Time.deltaTime;

		this.anchor.transform.Rotate(Vector3.forward, this.rotationInfo.DeltaAnglePerSec * deltaTime);
		this.rotationInfo.Update(deltaTime);
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		if (this.rotationInfo is null ||
			!this.rotationInfo.IsActive ||
			PlayerControl.LocalPlayer.PlayerId != this.ignorePlayerId)
		{
			return;
		}

		if (ScavengerWeaponHitHelper.IsHitPlayer(other, out var pc) &&
			pc.PlayerId != this.ignorePlayerId)
		{
			Player.RpcUncheckMurderPlayer(
				this.ignorePlayerId,
				pc.PlayerId,
				byte.MinValue);
		}
		else if (ScavengerWeaponHitHelper.IsHitWall(other))
		{
			this.gameObject.SetActive(false);
		}
	}

	public void OnDestroy()
	{
		if (this.anchor != null)
		{
			Destroy(this.anchor);
		}
		if (this.renderer != null)
		{
			Destroy(this.renderer);
		}
	}

	private void initialize(
		Vector2 size,
		in PlayerControl anchorPlayer)
	{
		this.anchor = new GameObject("SwordAnchor");
		this.anchor.transform.position = anchorPlayer.transform.position;
		this.anchor.transform.SetParent(anchorPlayer.transform);

		this.transform.SetParent(this.anchor.transform);

		var collider = this.gameObject.AddComponent<BoxCollider2D>();
		collider.isTrigger = true;
		collider.size = size;

		this.ignorePlayerId = anchorPlayer.PlayerId;

		this.renderer = this.gameObject.AddComponent<SpriteRenderer>();
		this.renderer.sprite = Scavenger.GetFromAsset<Sprite>(
			$"{Scavenger.Ability.ScavengerSword}.png");

		var rb = this.gameObject.AddComponent<Rigidbody2D>();
		rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		rb.isKinematic = true;
	}
}

[Il2CppRegister]
public sealed class ScavengerFlameBehaviour : MonoBehaviour
{

	public static ScavengerFlameBehaviour Create(
		in float fireSecond,
		in PlayerControl anchorPlayer)
	{
		var gameObj = Scavenger.GetFromAsset<GameObject>(
			ObjectPath.ScavengerFlame);
		var obj = Instantiate(gameObj);
		obj.layer = Constants.LivingPlayersOnlyMask;
		obj.transform.position = anchorPlayer.transform.position;
		obj.transform.SetParent(anchorPlayer.transform);
		obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		obj.SetActive(true);
		if (!obj.TryGetComponent<ScavengerFlameBehaviour>(out var flame) ||
			flame.hitBehaviour == null)
		{
			throw new NullException("Flame Missing!!!!!");
		}

		flame.hitBehaviour.Info = new ScavengerFlameHitBehaviour.HitInfo(
			anchorPlayer,
			fireSecond);

		return flame;
	}

	private ParticleSystem? fire;
	private ScavengerFlameHitBehaviour? hitBehaviour;
	private bool isStart = false;
	private bool prevFlip = false;

	public ScavengerFlameBehaviour(Ptr ptr) : base(ptr) { }

	public void Awake()
	{
		if (base.transform.TryGetComponent<ParticleSystem>(out var particle))
		{
			this.fire = particle;
			this.fire.Stop();
		}
		var colison = base.transform.Find("Collider");
		if (colison != null &&
			colison.TryGetComponent<ScavengerFlameHitBehaviour>(out var hitBehaviour))
		{
			this.hitBehaviour = hitBehaviour;
		}
		this.isStart = false;
	}

	public void Start()
	{
		if (this.hitBehaviour != null)
		{
			this.hitBehaviour.Reset();
		}
	}

	public void FixedUpdate()
	{
		if (!this.isStart ||
			this.fire == null ||
			this.hitBehaviour == null ||
			this.hitBehaviour.Info == null ||
			this.hitBehaviour.Info.IgnorePlayer == null)
		{
			return;
		}

		bool isFlip = this.hitBehaviour.Info.IgnorePlayer.cosmetics.FlipX;
		if (this.prevFlip == isFlip)
		{
			return;
		}
		this.changeRotate(isFlip);
		this.prevFlip = isFlip;
	}

	public void OnEnable()
	{
		if (this.hitBehaviour != null)
		{
			this.hitBehaviour.enabled = true;
		}
		this.StartCharge();
	}

	public void OnDisable()
	{
		if (this.hitBehaviour != null)
		{
			this.hitBehaviour.enabled = false;
		}
		if (this.fire != null)
		{
			this.isStart = false;
			this.fire.Stop();
		}
	}

	public void StartCharge()
	{
		this.isStart = false;
		if (this.fire != null)
		{
			this.fire.Play();
			this.transform.rotation = Quaternion.Euler(0.0f, 90.0f, 180.0f);
		}
	}

	public void Fire()
	{
		if (this.isStart ||
			this.fire == null ||
			this.hitBehaviour == null ||
			this.hitBehaviour.Info == null ||
			this.hitBehaviour.Info.IgnorePlayer == null)
		{
			return;
		}

		this.prevFlip = this.hitBehaviour.Info.IgnorePlayer.cosmetics.FlipX;
		this.changeRotate(this.prevFlip);

		this.isStart = true;
	}
	private void changeRotate(bool isFlip)
	{
		if (isFlip)
		{
			this.transform.rotation = Quaternion.Euler(0, 180.0f, 0.0f);
		}
		else
		{
			this.transform.rotation = Quaternion.Euler(0, 0.0f, 0.0f);
		}
	}
}

[Il2CppRegister]
public sealed class ScavengerFlameHitBehaviour : MonoBehaviour
{
	public sealed record class HitInfo(
		PlayerControl IgnorePlayer,
		float FireSecond);


	[HideFromIl2Cpp]
	public HitInfo? Info
	{
		get => this.info;
		set
		{
			if (value is null)
			{
				throw new NullException("value is null");
			}
			if (!ExtremeRoleManager.TryGetSafeCastedRole<Scavenger>(
				value.IgnorePlayer.PlayerId, out var scavenger))
			{
				throw new NullException("Scavenger is null");
			}

			this.info = value;
			this.frame = scavenger.FlameWepon;
		}
	}



	private Dictionary<byte, float> playerTimes = new Dictionary<byte, float>();
	private Scavenger.Flame? frame;
	private Dictionary<byte, ScavengerFlameFire> cacheFire = new Dictionary<byte, ScavengerFlameFire>();
	private HitInfo? info;

	public ScavengerFlameHitBehaviour(Ptr ptr) : base(ptr) { }

	public void Reset()
	{
		this.playerTimes.Clear();
		this.cacheFire.Clear();
	}

	public void LateUpdate()
	{
		if (PlayerControl.LocalPlayer == null ||
			this.Info == null ||
			this.Info.IgnorePlayer == null ||
			this.Info.IgnorePlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId ||
			this.frame == null)
		{
			return;
		}

		int itemNum = this.playerTimes.Count;
		if (itemNum != 0)
		{
			Dictionary<byte, float> newPlayerTime = new Dictionary<byte, float>(itemNum);
			foreach (var (playerId, time) in this.playerTimes)
			{
				float newTime = time - Time.deltaTime;
				if (newTime < 0.0f)
				{
					continue;
				}
				newPlayerTime[playerId] = time;
			}
			this.playerTimes = newPlayerTime;
		}
	}

	public void OnTriggerStay2D(Collider2D other)
	{
		if (this.frame is null ||
			this.Info is null ||
			this.Info.IgnorePlayer == null ||
			this.Info.IgnorePlayer.Data == null ||
			this.Info.IgnorePlayer.Data.IsDead ||
			!ScavengerWeaponHitHelper.IsHitPlayer(other, out var pc) ||
			pc.PlayerId == this.Info.IgnorePlayer.PlayerId ||
			PhysicsHelpers.AnythingBetween(
				this.Info.IgnorePlayer.transform.position,
				pc.transform.position,
				Constants.ShipAndAllObjectsMask, false))
		{
			return;
		}

		byte playerId = pc.PlayerId;

		if (!this.playerTimes.TryGetValue(playerId, out float time))
		{
			time = 0.0f;
		}

		float deltaTime = Time.deltaTime;
		if (this.cacheFire.TryGetValue(playerId, out var fire) &&
			fire != null)
		{
			if (fire.gameObject.activeSelf)
			{
				fire.Increse(deltaTime);
			}
			else
			{
				this.cacheFire.Remove(playerId);
			}
			return;
		}
		else if (time >= this.Info.FireSecond)
		{
			ExtremeSystemTypeManager.RpcUpdateSystem(
				ScavengerAbilitySystem.Type,
				writer =>
				{
					var pos = this.Info.IgnorePlayer.transform.position;
					writer.Write((byte)ScavengerAbilitySystem.Ops.WeponOps);
					writer.Write(this.Info.IgnorePlayer.PlayerId);
					writer.Write((byte)Scavenger.Ability.ScavengerFlame);
					writer.Write(pos.x);
					writer.Write(pos.y);
					writer.Write((byte)Scavenger.Flame.Ops.FireStart);
					writer.Write(playerId);
				});
			if (this.frame.TryGetFire(playerId, out var newFire))
			{
				this.cacheFire[playerId] = newFire;
			}
			this.playerTimes.Remove(playerId);
			return;
		}

		// Updateで減らす処理を入れてるので2倍で進める
		this.playerTimes[playerId] = time + (deltaTime * 2.0f);
	}
}

[Il2CppRegister]
public sealed class ScavengerFlameFire : MonoBehaviour
{
	public float DeadTime { private get; set; }
	public byte IgnorePlayerId { private get; set; }

	public PlayerControl? TargetPlayer
	{
		private get => targetPlayer;
		set
		{
			if (value == null)
			{
				return;
			}

			this.timer = Time.deltaTime * 2.0f;
			this.targetPlayer = value;
			this.prevPos = value.GetTruePosition();
		}
	}

	private float timer;
	private Vector2 prevPos;
	private PlayerControl? targetPlayer;
	private ParticleSystem? fire;

	public void Awake()
	{
		if (this.gameObject.TryGetComponent<ParticleSystem>(out var particle))
		{
			this.fire = particle;
			this.fire.Play();
		}

	}

	public void OnEnable()
	{
		if (this.fire != null)
		{
			this.fire.Play();
		}
	}

	public void LateUpdate()
	{
		if (this.fire == null ||
			this.TargetPlayer == null ||
			PlayerControl.LocalPlayer == null ||
			PlayerControl.LocalPlayer.PlayerId != this.IgnorePlayerId)
		{
			return;
		}
		if (MeetingHud.Instance != null ||
			ExileController.Instance != null)
		{
			this.gameObject.SetActive(false);
			return;
		}
		var cur = this.TargetPlayer.GetTruePosition();
		this.Increse(cur == this.prevPos ? Time.deltaTime : -Time.deltaTime);
		this.prevPos = cur;
	}

	public void Increse(float addTime)
	{
		if (this.TargetPlayer == null)
		{
			return;
		}

		float newTime = this.timer + addTime;
		if (newTime > this.DeadTime)
		{
			disable();
			Player.RpcUncheckMurderPlayer(
				this.TargetPlayer.PlayerId,
				this.TargetPlayer.PlayerId,
				byte.MinValue);
		}
		else if (newTime < 0.0f)
		{
			disable();
		}
		else
		{
			this.timer = newTime;
		}
	}

	public void OnDisable()
	{
		if (this.fire != null)
		{
			this.fire.Stop();
		}
	}

	private void disable()
	{
		var local = PlayerControl.LocalPlayer;
		if (local == null || this.TargetPlayer == null)
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
				writer.Write((byte)Scavenger.Ability.ScavengerFlame);
				writer.Write(pos.x);
				writer.Write(pos.y);
				writer.Write((byte)Scavenger.Flame.Ops.FireEnd);
				writer.Write(this.TargetPlayer.PlayerId);
			});
	}
}