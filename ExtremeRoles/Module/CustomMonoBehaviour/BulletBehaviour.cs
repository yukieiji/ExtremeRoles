using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class BulletBehaviour : MonoBehaviour
{
	private float distanceTraveled = 0f;

	private BoxCollider2D? collider;
	private readonly int collisionLayer = Constants.ShipAndAllObjectsMask;

	private Vector2 direction; // 方向ベクトルA
	private float speed = 0; // 速度ベクトルB
	private float range; // 射程C
	private SpriteRenderer? rend;
	private byte ignorePlayerId;

	public void Awake()
	{
		this.collider = this.gameObject.AddComponent<BoxCollider2D>();
	}

	public void Initialize(
		in string bulletImg,
		in Vector2 size,
		in Vector2 direction,
		in float speed,
		in float range,
		in byte ignorePlayerId)
	{
		if (this.collider == null)
		{
			return;
		}
		this.collider.size = size;
		this.direction = direction;
		this.speed = speed;
		this.range = range;
		this.ignorePlayerId = ignorePlayerId;

		this.rend = this.gameObject.AddComponent<SpriteRenderer>();
		this.rend.sprite = Loader.CreateSpriteFromResources(
			bulletImg);
	}

	public void Update()
	{
		if (this.collider == null || this.rend == null)
		{
			return;
		}

		// ベクトルBに基づいて移動
		Vector2 xyPos = this.direction.normalized * speed * Time.deltaTime;
		this.transform.Translate(new Vector3(xyPos.x, xyPos.y, xyPos.y / 1000.0f));

		// 移動した距離を更新
		this.distanceTraveled += speed * Time.deltaTime;

		// 射程を超えた場合は消滅
		if (distanceTraveled >= range)
		{
			Destroy(this.gameObject);
		}
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		// レイヤーDのオブジェクトと衝突した場合は消滅
		if (this.collisionLayer == (collisionLayer | (1 << other.gameObject.layer)))
		{
			Destroy(this.gameObject);
		}

		// オブジェクトEとの当たり判定が発生する
		foreach (PlayerControl pc in CachedPlayerControl.AllPlayerControls)
		{
			if (pc == null ||
				pc.Data == null ||
				pc.Data.IsDead ||
				pc.Data.Disconnected ||
				pc.PlayerId == pc.PlayerId ||
				pc.inVent ||
				pc.Collider != other)
			{
				continue;
			}

			// 衝突処理を記述
			// 例えば、ダメージを与える、エフェクトを再生するなど
		}
	}
}
