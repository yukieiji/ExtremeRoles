using UnityEngine;

using ExtremeRoles.Resources;
using ExtremeRoles.Performance;

using ExtremeRoles.Helper;

using Act = System.Action;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

public static class  WeaponHitHelper
{
	private static readonly int collisionLayer = LayerMask.GetMask(new string[] { "Ship", "Objects" });

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

[Il2CppRegister]
public sealed class BulletBehaviour : MonoBehaviour
{
	public sealed record Parameter(
		string Img,
		Vector2 Size,
		float Speed,
		float Range);

	private float range; // 射程C
	private byte ignorePlayerId;
	private Vector2 initialPosition;

	public static BulletBehaviour Create(
		int id,
		in PlayerControl abilityPlayer,
		in Vector2 direction,
		in Parameter param)
	{
		var obj = new GameObject($"Bullet_{id}");
		obj.transform.position = abilityPlayer.transform.position;
		obj.layer = Constants.LivingPlayersOnlyMask;

		var bullet = obj.AddComponent<BulletBehaviour>();
		bullet.Initialize(
			param.Img,
			param.Size,
			direction,
			param.Speed,
			param.Range,
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
		if (CachedPlayerControl.LocalPlayer.PlayerId != this.ignorePlayerId)
		{
			return;
		}

		if (WeaponHitHelper.IsHitPlayer(other, out var pc) &&
			pc.PlayerId != this.ignorePlayerId)
		{
			Player.RpcUncheckMurderPlayer(
				this.ignorePlayerId,
				pc.PlayerId,
				byte.MinValue);
			Destroy(this.gameObject);
		}
		else if (WeaponHitHelper.IsHitWall(other))
		{
			Destroy(this.gameObject);
		}
	}
}

[Il2CppRegister(typeof(IUsable))]
public sealed class SwordBehaviour : MonoBehaviour
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

	private RotationInfo? rotationInfo;
	public SwordBehaviour(System.IntPtr ptr) : base(ptr) { }

	public static SwordBehaviour Create(
		in string img,
		in Vector2 size,
		in PlayerControl anchorPlayer)
	{
		var obj = new GameObject($"Sword_{anchorPlayer.PlayerId}");
		obj.transform.position = anchorPlayer.transform.position - new Vector3(
			size.x / 2 + 0.5f, 0.0f, 0.0f);
		obj.layer = Constants.LivingPlayersOnlyMask;

		var sword = obj.AddComponent<SwordBehaviour>();
		sword.initialize(
			img,
			size,
			anchorPlayer);
		return sword;
	}

	public void SetRotation(in RotationInfo rotation, bool isReset)
	{
		this.rotationInfo = rotation;
		if (isReset && this.anchor != null)
		{
			this.anchor.transform.Rotate(Vector3.forward, 0.0f);
		}
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
			this.rotationInfo is null)
		{
			return;
		}

		if (this.rotationInfo.Time <= 0)
		{
			if (this.rotationInfo.IsActive)
			{
				Destroy(this.gameObject);
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
			!this.rotationInfo.IsActive)
		{
			return;
		}

		if (WeaponHitHelper.IsHitPlayer(other, out var pc) &&
			pc.PlayerId != this.ignorePlayerId)
		{
			ExtremeRolesPlugin.Logger.LogInfo("Hit Player!!!");
		}
		else if (WeaponHitHelper.IsHitWall(other))
		{
			ExtremeRolesPlugin.Logger.LogInfo("Hit Wall");
			this.gameObject.SetActive(false);
		}
	}

	public void OnDestroy()
	{
		if (this.anchor != null)
		{
			Destroy(this.anchor);
		}
	}

	private void initialize(
		in string bulletImg,
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

		var rend = this.gameObject.AddComponent<SpriteRenderer>();
		rend.sprite = Loader.CreateSpriteFromResources(
			bulletImg);

		var rb = this.gameObject.AddComponent<Rigidbody2D>();
		rb.isKinematic = true;
	}
}