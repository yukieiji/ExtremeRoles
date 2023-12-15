using ExtremeRoles.Performance;
using System;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class AcceleratorPanel : MonoBehaviour
{
	private Rigidbody2D? body;
	private BoxCollider2D? collider;
	private Vector2 forceVec;

	public AcceleratorPanel(IntPtr ptr) : base(ptr) { }

	public void Awake()
	{
		if (CachedPlayerControl.LocalPlayer == null) { return; }

		this.collider = base.gameObject.AddComponent<BoxCollider2D>();
		this.collider.isTrigger = true;
		this.collider.size = new Vector2(1.5f, 1.15f);
		this.collider.isTrigger = true;

		this.body = CachedPlayerControl.LocalPlayer.PlayerControl.rigidbody2D;
	}

	public void Initialize(Vector2 vector, float speed)
	{
		this.forceVec = vector * speed;
	}

	public void FixedUpdate()
	{
		if (CachedPlayerControl.LocalPlayer == null ||
			this.body == null ||
			this.collider == null ||
			!this.body.IsTouching(this.collider)) { return; }

		this.body.velocity += this.forceVec;

	}
}
