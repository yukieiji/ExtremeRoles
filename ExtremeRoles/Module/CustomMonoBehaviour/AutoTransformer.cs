using System;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class AutoTransformerWithFixedFirstPoint : MonoBehaviour
{
	public Vector2 Vector
	{
		get
		{
			if (this.end == null)
			{
				return Vector2.zero;
			}
			Vector2 diff = this.end.position - this.start;
			return diff.normalized;
		}
	}

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

	// 画像は右横向きなので
	public void FixedUpdate()
	{
		if (this.end == null) { return; }

		this.timer += Time.fixedDeltaTime;

		if (this.timer < 0.15f) { return; }

		// 始点から終点への方向ベクトルを求める
		Vector3 direction = this.end.position - this.start;

		// 対象オブジェクトを中間点に配置する
		this.transform.position = Vector3.Lerp(this.start, this.end.position, 0.5f);

		// 指定した方向に回転
		this.transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);

		// 画像が横になってるから特定の角度でyとxを逆にする
		var angle = this.transform.rotation.eulerAngles;
		Vector2 sizeVec =
			45.0f <= angle.z && angle.z < 135.0f ||
			225.0f <= angle.z && angle.z < 315.0f ?
				new Vector2(direction.y, direction.x) : direction;

		this.transform.localScale = new Vector3(
			Mathf.Abs(sizeVec.x) / this.rect.width,
			Mathf.Abs(sizeVec.y) / this.rect.height, 1.0f);

		this.timer = 0.0f;
	}
}
