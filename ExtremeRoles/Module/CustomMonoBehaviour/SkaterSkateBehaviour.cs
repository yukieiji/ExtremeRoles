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

	public record struct Parameter(float Friction, float Acceleration, float MaxSpeed);

	[HideFromIl2Cpp]
	public Vector2 PrevForce { get; private set; }

	private float speed;
	private float frictionMulti;
	private float maxSpeed;

	public SkaterSkateBehaviour(IntPtr ptr) : base(ptr) { }


	[HideFromIl2Cpp]
	public void Initialize(in Parameter param)
	{
		this.speed = param.Acceleration * SpeedOffset * Time.fixedDeltaTime;
		this.frictionMulti = (1 - param.Friction) * Time.fixedDeltaTime;
		this.maxSpeed = param.MaxSpeed * SpeedOffset;
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

		Vector2 forceVector =
			directionVector == Vector2.zero ?
			this.PrevForce * this.frictionMulti :
			this.PrevForce + (directionVector * this.speed);

		Vector2 clampedVector = Vector2.ClampMagnitude(forceVector, this.maxSpeed);

		this.PrevForce = clampedVector;

		pc.rigidbody2D.AddForce(clampedVector);
	}
}
