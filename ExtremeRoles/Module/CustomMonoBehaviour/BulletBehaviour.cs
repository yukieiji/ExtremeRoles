using UnityEngine;

using ExtremeRoles.Resources;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class BulletBehaviour : MonoBehaviour
{
	private readonly int collisionLayer = LayerMask.GetMask(new string[] { "Objects", "ShortObjects" });

	private float range; // 射程C
	private byte ignorePlayerId;
	private Vector2 initialPosition;

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
			this.removeThis();
		}
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		ExtremeRolesPlugin.Logger.LogInfo("Hitting!!");
		ExtremeRolesPlugin.Logger.LogInfo($"ObjName:{other}");

		// レイヤーDのオブジェクトと衝突した場合は消滅
		if (other.TryGetComponent<PlayerControl>(out var pc) &&
			pc != null &&
			pc.Data != null &&
			!pc.Data.IsDead &&
			!pc.Data.Disconnected &&
			pc.PlayerId != this.ignorePlayerId &&
			!pc.inVent)
		{
			ExtremeRolesPlugin.Logger.LogInfo("Hit Player!!!");
		}
		else if (this.collisionLayer == (this.collisionLayer | (1 << other.gameObject.layer)))
		{
			ExtremeRolesPlugin.Logger.LogInfo("Hit Wall");
			this.removeThis();
		}
	}

	private void removeThis()
	{
		Destroy(this.gameObject);
	}
}
