using ExtremeRoles.Performance;
using System;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class AcceleratorPanel : MonoBehaviour
{
	public float MaxSpeed { private get; set; }
	public float AddSpeed { private get; set; }

	private Vector3 angle;
	private BoxCollider2D? collider;
	private Rigidbody2D? rigidbody;

	public AcceleratorPanel(IntPtr ptr) : base(ptr) { }

	public void Awake()
	{
		if (CachedPlayerControl.LocalPlayer == null) { return; }

		this.collider = base.gameObject.AddComponent<BoxCollider2D>();
		this.collider.isTrigger = true;
		this.collider.size = new Vector2(1.5f, 1.15f);
		this.collider.isTrigger = true;

		RectTransform rectTransform = GetComponent<RectTransform>();
		var rect = rectTransform.rect;

		this.collider.size = new Vector2(rect.width, rect.height);
		this.angle = base.transform.rotation.eulerAngles;
		this.rigidbody = CachedPlayerControl.LocalPlayer.PlayerControl.rigidbody2D;
	}
	public void FixUpdate()
	{
		if (CachedPlayerControl.LocalPlayer == null ||
			this.rigidbody == null ||
			this.collider == null) { return; }

		//物体の移動速度のベルトコンベア方向の成分だけを取り出す
		float objectSpeed = Vector3.Dot(
			this.rigidbody.velocity,
			this.angle);

		//目標値以下なら加速する
		if (objectSpeed < Mathf.Abs(this.MaxSpeed))
		{
			this.rigidbody.AddForce(this.angle * this.AddSpeed, ForceMode2D.Impulse);
		}
	}
}
