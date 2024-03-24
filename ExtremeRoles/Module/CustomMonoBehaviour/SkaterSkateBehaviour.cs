using System;

using UnityEngine;

using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class SkaterSkateBehaviour : MonoBehaviour
{
	public const float SpeedOffset = 32.0f;

	public record struct Parameter(float Friction, float Acceleration, float MaxSpeed, float? E=null);

	[HideFromIl2Cpp]
	public Vector2 PrevForce { get; private set; } = Vector2.zero;

	private float speed;
	private float frictionMulti;
	private float? e;
	private float maxSpeed;
	private readonly Vector2 offset = new Vector2(0.275f, 0.5f);
	private Vector2 prevPos = Vector2.zero;

	public SkaterSkateBehaviour(IntPtr ptr) : base(ptr) { }


	[HideFromIl2Cpp]
	public void Initialize(in Parameter param)
	{
		this.speed = param.Acceleration * SpeedOffset * Time.fixedDeltaTime;
		this.frictionMulti = (1 - (param.Friction * Time.fixedDeltaTime));
		this.maxSpeed = param.MaxSpeed * SpeedOffset;
		this.e = param.E;
	}

	public void Reset()
	{
		this.PrevForce = Vector2.zero;
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
			this.Reset();
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

		Vector2 forceVector =
			directionVector == Vector2.zero ?
			this.PrevForce * this.frictionMulti :
			this.PrevForce + (directionVector * this.speed);

		var rigidBody = pc.rigidbody2D;
		Vector2 curPos = pc.transform.position;

		if (this.e.HasValue &&
			this.PrevForce != Vector2.zero &&
			(
				PhysicsHelpers.AnythingBetween(
					curPos, curPos + (this.PrevForce.normalized * offset),
					Constants.ShipAndObjectsMask, false) ||
				(curPos - this.prevPos).normalized == Vector2.zero
			))
		{
			forceVector = -forceVector * this.e.Value;
		}
		Vector2 clampedVector = Vector2.ClampMagnitude(forceVector, this.maxSpeed);
		this.prevPos = curPos;
		this.PrevForce = clampedVector;

		rigidBody.AddForce(clampedVector);
	}
}
