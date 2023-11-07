using System;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class AutoTransformer : MonoBehaviour
{
	private Transform? start;
	private Transform? end;

	private float timer;

	public AutoTransformer(IntPtr ptr) : base(ptr) { }

	public void Initialize(Transform start, Transform end)
	{
		this.start = start;
		this.end = end;
	}

	public void FixedUpdate()
	{
		if (this.start == null || this.end == null) { return; }

		this.timer += Time.fixedDeltaTime;

		if (this.timer < 0.15f) { return; }

		// 始点と終点の中間点を求める
		Vector2 middlePoint = (start.position + end.position) / 2f;

		// 対象オブジェクトを中間点に配置する
		base.transform.position = middlePoint;

		// 始点から終点への方向ベクトルを求める
		Vector3 direction = end.position - start.position;

		// オブジェクトの拡大率を設定する（始点から終点までの距離に基づく）
		float scaleFactor = direction.magnitude;
		base.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

		// オブジェクトの回転を設定する（方向ベクトルに基づく）
		base.transform.rotation = Quaternion.LookRotation(direction.normalized);

		this.timer = 0.0f;
	}
}
