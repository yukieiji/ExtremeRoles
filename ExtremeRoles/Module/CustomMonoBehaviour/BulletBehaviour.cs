using UnityEngine;

using ExtremeRoles.Resources;
using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

public static class  WeaponHitHelper
{
	private static readonly int collisionLayer = LayerMask.GetMask(new string[] { "Ship", "Objects" });

	public static void OnHitCollider2D(
		GameObject obj, Collider2D other,
		byte localPlayerId,
		bool isPermeatePlayer = false,
		bool isHitWall = true)
	{
		ExtremeRolesPlugin.Logger.LogInfo("Hitting!!");
		ExtremeRolesPlugin.Logger.LogInfo($"ObjName:{other}");
		if (CachedPlayerControl.LocalPlayer.PlayerId != localPlayerId)
		{
			return;
		}

		// レイヤーDのオブジェクトと衝突した場合は消滅
		if (other.TryGetComponent<PlayerControl>(out var pc) &&
			pc != null &&
			pc.Data != null &&
			!pc.Data.IsDead &&
			!pc.Data.Disconnected &&
			pc.PlayerId != localPlayerId &&
			!pc.inVent)
		{
			ExtremeRolesPlugin.Logger.LogInfo("Hit Player!!!");
			if (isPermeatePlayer)
			{
				// Destory Rpc
			}
		}
		else if (
			isHitWall &&
			collisionLayer == (collisionLayer | (1 << other.gameObject.layer)))
		{
			ExtremeRolesPlugin.Logger.LogInfo("Hit Wall");
			// Destory Rpc
		}
	}
}

[Il2CppRegister]
public sealed class BulletBehaviour : MonoBehaviour
{
	private float range; // 射程C
	private byte ignorePlayerId;
	private Vector2 initialPosition;

	public static BulletBehaviour Create(
		int id,
		in string bulletImg,
		in Vector2 size,
		in Vector2 direction,
		in float speed,
		in float range,
		in PlayerControl abilityPlayer)
	{
		var obj = new GameObject($"Bullet_{id}");
		obj.transform.position = abilityPlayer.transform.position;
		obj.layer = Constants.LivingPlayersOnlyMask;

		var bullet = obj.AddComponent<BulletBehaviour>();
		bullet.Initialize(
				bulletImg,
				size,
				direction,
				speed,
				range,
				abilityPlayer.PlayerId);
		return bullet;
	}

	public void Initialize(
		in string bulletImg,
		in Vector2 size,
		in Vector2 direction,
		in float speed,
		in float range,
		in byte ignorePlayerId)
	{

		var collider = this.gameObject.AddComponent<BoxCollider2D>();
		collider.isTrigger = true;
		collider.size = size;

		this.range = range;
		this.ignorePlayerId = ignorePlayerId;

		var rend = this.gameObject.AddComponent<SpriteRenderer>();
		rend.sprite = Loader.CreateSpriteFromResources(
			bulletImg);

		this.initialPosition = transform.position;

		var rb = this.gameObject.AddComponent<Rigidbody2D>();
		rb.velocity = direction.normalized * speed;
		rb.isKinematic = true;
	}

	public void Update()
	{
		if (Vector2.Distance(this.transform.position, this.initialPosition) > this.range)
		{
			Destroy(this.gameObject);
		}
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		WeaponHitHelper.OnHitCollider2D(
			this.gameObject,
			other,
			this.ignorePlayerId,
			false,
			true);
	}
}

[Il2CppRegister(typeof(IUsable))]
public sealed class SwordBehaviour : MonoBehaviour
{
	private sealed class RotationInfo
	{
		public float Time { get; private set; }
		public float DeltaAnglePerSec { get; private set; }

		public RotationInfo(
			in float time,
			in float angle)
		{
			this.Time = time;
			this.DeltaAnglePerSec = angle / time;
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

	private RotationInfo? activeRotationInfo;
	private RotationInfo? chargeRotationInfo;

	public SwordBehaviour(System.IntPtr ptr) : base(ptr) { }

	public static SwordBehaviour Create(
		int id,
		in string bulletImg,
		in Vector2 size,
		in float rotationTime,
		in float chargeRotationTime,
		in PlayerControl anchorPlayer)
	{
		var obj = new GameObject($"Sword_{id}");
		obj.transform.position = anchorPlayer.transform.position + new Vector3(
			size.x / 2 - 0.5f, 0.0f, 0.0f);
		obj.layer = Constants.LivingPlayersOnlyMask;

		var sword = obj.AddComponent<SwordBehaviour>();
		sword.Initialize(
				bulletImg,
				size,
				rotationTime,
				chargeRotationTime,
				anchorPlayer);
		return sword;
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
		// キル
		Destroy(this.gameObject);
	}

	public void Update()
	{
		if (this.anchor == null ||
			this.activeRotationInfo is null ||
			this.chargeRotationInfo is null)
		{
			return;
		}

		if (this.activeRotationInfo.Time <= 0)
		{
			Destroy(this.gameObject);
		}

		float deltaTime = Time.deltaTime;

		var curRotationInfo = this.chargeRotationInfo.Time > 0 ? this.chargeRotationInfo : this.activeRotationInfo;
		this.anchor.transform.Rotate(Vector3.forward, curRotationInfo.DeltaAnglePerSec * deltaTime);
		curRotationInfo.Update(deltaTime);
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		if (this.chargeRotationInfo is null ||
			this.activeRotationInfo is null ||
			this.chargeRotationInfo.Time > 0 ||
			this.activeRotationInfo.Time <= 0)
		{
			return;
		}

		WeaponHitHelper.OnHitCollider2D(
			this.gameObject,
			other,
			this.ignorePlayerId,
			false,
			true);
	}

	public void OnDestroy()
	{
		if (this.anchor != null)
		{
			Destroy(this.anchor);
		}
	}

	public void Initialize(
		in string bulletImg,
		in Vector2 size,
		in float rotationTime,
		in float chargeRotationTime,
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

		var rend = this.gameObject.AddComponent<SpriteRenderer>();
		rend.sprite = Loader.CreateSpriteFromResources(
			bulletImg);

		var rb = this.gameObject.AddComponent<Rigidbody2D>();
		rb.isKinematic = true;

		this.activeRotationInfo = new RotationInfo(rotationTime, 360);
		this.chargeRotationInfo = new RotationInfo(chargeRotationTime, -45);
	}
}