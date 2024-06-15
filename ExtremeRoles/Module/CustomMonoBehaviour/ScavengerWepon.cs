using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Resources;
using ExtremeRoles.Performance;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;

using NullException = System.ArgumentNullException;
using Scavenger = ExtremeRoles.Roles.Solo.Impostor.Scavenger;


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

	public Info WeponInfo
	{
		private get => weponInfo;
		set
		{
			weponInfo = value;
			var rend = base.gameObject.TryAddComponent<SpriteRenderer>();
			rend.sprite = Scavenger.GetFromAsset<Sprite>(
				$"assets/roles/scavenger.{weponInfo.Ability}.{Path.MapIcon}.png");

			var collider = base.gameObject.TryAddComponent<CircleCollider2D>();
			collider.isTrigger = true;
			collider.radius = 0.025f;
		}
	}
	private Info weponInfo;

	public float UsableDistance => 0.5f;

	public float PercentCool => 0.0f;

	public ImageNames UseIcon => ImageNames.UseButton;

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
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

	public static ScavengerBulletBehaviour Create(
		int id,
		Scavenger.Ability ability,
		in PlayerControl abilityPlayer,
		in Vector2 direction,
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

	public void Initialize(
		int id,
		Scavenger.Ability ability,
		in string bulletImg,
		in Vector2 size,
		in Vector2 direction,
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
		this.renderer.sprite = Scavenger.GetFromAsset<Sprite>(
			$"assets/roles/scavenger.{bulletImg}.png");

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
		if (CachedPlayerControl.LocalPlayer.PlayerId != this.ignorePlayerId)
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
	public ScavengerSwordBehaviour(System.IntPtr ptr) : base(ptr) { }

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

	public void SetRotation(in RotationInfo rotation, bool isReset)
	{
		if (isReset && this.anchor != null)
		{
			this.anchor.transform.rotation = Quaternion.identity;
		}
		this.rotationInfo = rotation;
	}

	public float CanUse(
		GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
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
			CachedPlayerControl.LocalPlayer.PlayerId,
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
			CachedPlayerControl.LocalPlayer.PlayerId != this.ignorePlayerId)
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
		in Vector2 size,
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
			$"assets/roles/scavenger.{Scavenger.Ability.Sword}.png");

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
		in float fireDeadSecond,
		in PlayerControl anchorPlayer)
	{
		var gameObj = Scavenger.GetFromAsset<GameObject>(
			"assets/roles/scavenger.flame.prefab");
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
			fireSecond,
			fireDeadSecond);

		return flame;
	}

	private ParticleSystem? fire;
	private ScavengerFlameHitBehaviour? hitBehaviour;
	private bool isStart = false;
	private bool prevFlip = false;

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
		float FireSecond,
		float DeadCountDown);

	private sealed class PlayerDeadTimerContainer(float deadTime)
	{
		private readonly Dictionary<byte, float> playerDeadTimes = new Dictionary<byte, float>();
		private readonly Dictionary<byte, PlayerControl> pcs = new Dictionary<byte, PlayerControl>();
		private readonly Dictionary<byte, Vector2> prevPos = new Dictionary<byte, Vector2>();
		private readonly float deadTime = deadTime;

		private const float blockTime = 0.1f;
		private float blockTimer = 0.1f;

		public bool IsContain(byte playerId) => this.playerDeadTimes.ContainsKey(playerId);

		public void Update()
		{
			this.blockTimer += Time.deltaTime;
			if (this.blockTimer < blockTime)
			{
				return;
			}
			this.blockTimer = 0.0f;

			foreach (var (id, pc) in pcs)
			{
				var cur = pc.GetTruePosition();
				var prev = prevPos[id];
				Increse(id, cur == prev ? blockTime : -blockTime);
				this.prevPos[id] = cur;
			}
		}

		public void Increse(byte playerId, float addTime)
		{
			float newTime = this.playerDeadTimes[playerId] + addTime;
			if (newTime >= this.deadTime)
			{
				// 焼死
				Player.RpcUncheckMurderPlayer(
					playerId, playerId, byte.MinValue);
				// エフェクト非表示処理

				this.pcs.Remove(playerId);
				this.playerDeadTimes.Remove(playerId);
			}
			else if (newTime < 0.0f)
			{
				this.pcs.Remove(playerId);
				this.playerDeadTimes.Remove(playerId);
			}
			else
			{
				this.playerDeadTimes[playerId] = newTime;
			}
		}

		public void Add(byte playerId)
		{
			var pc = Player.GetPlayerControlById(playerId);
			if (pc == null)
			{
				return;
			}
			this.playerDeadTimes[playerId] = Time.deltaTime * 2.0f;
			this.pcs[playerId] = pc;
			this.prevPos[playerId] = pc.GetTruePosition();
		}
	}

	public HitInfo? Info
	{
		get => this.info;
		set
		{
			if (value is null)
			{
				throw new NullException("value is null");
			}
			this.info = value;
			this.playerDeadTimer = new PlayerDeadTimerContainer(this.info.DeadCountDown);
		}
	}

	private readonly Dictionary<byte, float> playerTimes = new Dictionary<byte, float>();
	private PlayerDeadTimerContainer? playerDeadTimer;
	private HitInfo? info;

	public void LateUpdate()
	{
		foreach (byte key in playerTimes.Keys)
		{
			if (!this.playerTimes.TryGetValue(key, out float time))
			{
				continue;
			}
			float newTime = time - Time.deltaTime;
			if (newTime < 0.0f)
			{
				this.playerTimes.Remove(key);
			}
			this.playerTimes[key] = newTime;
		}
		this.playerDeadTimer?.Update();
	}

	public void OnTriggerStay2D(Collider2D other)
	{
		if (this.playerDeadTimer is null ||
			this.Info is null ||
			this.Info.IgnorePlayer == null ||
			!ScavengerWeaponHitHelper.IsHitPlayer(other, out var pc) ||
			pc.PlayerId == this.Info.IgnorePlayer.PlayerId ||
			PhysicsHelpers.AnythingBetween(
				this.Info.IgnorePlayer.transform.position,
				base.transform.position,
				Constants.ShipAndObjectsMask, false))
		{
			return;
		}

		if (!this.playerTimes.TryGetValue(pc.PlayerId, out float time))
		{
			time = 0.0f;
		}

		if (this.playerDeadTimer.IsContain(pc.PlayerId))
		{
			this.playerDeadTimer.Increse(pc.PlayerId, Time.deltaTime);
			return;
		}
		else if (time >= this.Info.FireSecond)
		{
			this.playerDeadTimer.Add(pc.PlayerId);

			// Rpcのエフェクト追加処理

			this.playerTimes.Remove(pc.PlayerId);
			return;
		}

		// Updateで減らす処理を入れてるので2倍で進める
		this.playerTimes[pc.PlayerId] = time + (Time.deltaTime * 2.0f);
	}
}