using ExtremeRoles.Performance;
using System;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class AcceleratorPanel : MonoBehaviour
{
	private BoxCollider2D? collider;
	private Vector2 forceVec;

	public AcceleratorPanel(IntPtr ptr) : base(ptr) { }

	public void Awake()
	{
		if (CachedPlayerControl.LocalPlayer == null) { return; }

		this.collider = base.gameObject.AddComponent<BoxCollider2D>();
		this.collider.size = new Vector2(1.5f, 1.15f);
		this.collider.isTrigger = true;
	}

	public void Initialize(Vector2 vector, float speed)
	{
		this.forceVec = vector * speed;
	}

	public void FixedUpdate()
	{
		if (CachedPlayerControl.LocalPlayer == null ||
			this.collider == null) { return; }

		var body = CachedPlayerControl.LocalPlayer.PlayerControl.rigidbody2D;

		body.velocity += this.forceVec;

	}
}
