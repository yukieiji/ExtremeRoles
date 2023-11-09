using System;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class AutoTransformerWithFixedFirstPoint : MonoBehaviour
{
	private Vector3 start;
	private Transform? end;
	private Rect rect;

	private float timer = 0.0f;

	public AutoTransformerWithFixedFirstPoint(IntPtr ptr) : base(ptr) { }

	public void Initialize(Vector3 start, Transform end, SpriteRenderer rend)
	{
		this.start = start;
		this.end = end;

		this.rect = rend.sprite.textureRect;
		float unit = rend.sprite.pixelsPerUnit;

		this.rect.width  /= unit;
		this.rect.height /= unit;
	}

	public void FixedUpdate()
	{
		if (this.end == null) { return; }

		this.timer += Time.fixedDeltaTime;

		if (this.timer < 0.15f) { return; }

		// 始点から終点への方向ベクトルを求める
		Vector3 direction = this.end.position - this.start;
		this.transform.localScale = new Vector3(direction.x / this.rect.width, direction.y / this.rect.height, 1.0f);

		// 対象オブジェクトを中間点に配置する
		this.transform.position = Vector2.Lerp(this.start, this.end.position, 0.5f);

		this.timer = 0.0f;
	}
}
