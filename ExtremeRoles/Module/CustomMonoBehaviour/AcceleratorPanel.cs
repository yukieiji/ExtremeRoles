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
	private const float offset = 32.0f;

	public AcceleratorPanel(IntPtr ptr) : base(ptr) { }

	public void Awake()
	{
		if (PlayerControl.LocalPlayer == null) { return; }

		this.collider = base.gameObject.AddComponent<BoxCollider2D>();
		this.collider.size = new Vector2(1.5f, 1.15f);
		this.collider.isTrigger = true;
	}

	public void Initialize(Vector2 vector, float speed)
	{
		this.forceVec = vector * speed * offset;
	}

	public void FixedUpdate()
	{
		PlayerControl? pc = PlayerControl.LocalPlayer;
		if (pc == null ||
			pc.inMovingPlat ||
			pc.inVent ||
			pc.onLadder ||
			this.collider == null) { return; }

		var body = pc.rigidbody2D;
		if (!body.IsTouching(this.collider)) { return; }

		body.AddForce(this.forceVec);

	}
}
