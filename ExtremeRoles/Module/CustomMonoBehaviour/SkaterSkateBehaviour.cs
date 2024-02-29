using ExtremeRoles.Performance;
using System;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class SkaterSkateBehaviour : MonoBehaviour
{
	private const float offset = 32.0f;

	public Vector2 PrevForce { get; private set; }

	private float speed;
	private float friction;
	private float maxSpeed;

	public SkaterSkateBehaviour(IntPtr ptr) : base(ptr) { }

	public void Initialize(float friction, float speed, float maxSpeed)
	{
		this.speed = speed * offset * Time.fixedDeltaTime;
		this.friction = friction * Time.fixedDeltaTime;
		this.maxSpeed = maxSpeed * offset;
	}

	public void FixedUpdate()
	{
		PlayerControl pc = CachedPlayerControl.LocalPlayer;

		if (pc == null ||
			pc.Data == null ||
			pc.Data.Disconnected ||
			pc.Data.IsDead ||
			!pc.CanMove ||
			pc.inMovingPlat ||
			pc.onLadder ||
			MeetingHud.Instance != null ||
			ExileController.Instance != null)
		{
			this.PrevForce = Vector2.zero;
			return;
		}

		Vector2 directionVector = Vector2.zero;

		if (KeyboardJoystick.player.GetButton(40))
		{
			directionVector.x = directionVector.x + 1f;
		}
		if (KeyboardJoystick.player.GetButton(39))
		{
			directionVector.x = directionVector.x - 1f;
		}
		if (KeyboardJoystick.player.GetButton(44))
		{
			directionVector.y = directionVector.y + 1f;
		}
		if (KeyboardJoystick.player.GetButton(42))
		{
			directionVector.y = directionVector.y - 1f;
		}

		Vector2 addForce =
			directionVector == Vector2.zero ?
			-(this.PrevForce * this.friction) : directionVector * this.speed;

		Vector2 forceVector = this.PrevForce + addForce;
		Vector2 clampedVector = Vector2.ClampMagnitude(forceVector, this.maxSpeed);

		this.PrevForce = clampedVector;

		CachedPlayerControl.LocalPlayer.PlayerControl.rigidbody2D.AddForce(clampedVector);
	}
}
