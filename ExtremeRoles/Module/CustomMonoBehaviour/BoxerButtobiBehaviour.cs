using System;

using UnityEngine;

using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Helper;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class BoxerButtobiBehaviour : MonoBehaviour
{
	public const float SpeedOffset = 64.0f;

	public readonly record struct Parameter(float Acceleration, float E);

	[HideFromIl2Cpp]
	public Vector2 PrevForce { get; private set; } = Vector2.zero;

	private float speed;
	private float killSpeed;
	private float detaTimeSpeedOfsset;
	private float e;
	private readonly Vector2 offset = new Vector2(0.275f, 0.5f);
	private Vector2 prevPos = Vector2.zero;

	public BoxerButtobiBehaviour(IntPtr ptr) : base(ptr) { }

	[HideFromIl2Cpp]
	public void Initialize(Vector2 prevForce, float killSpeed, in Parameter param)
	{
		this.speed = param.Acceleration * Time.fixedDeltaTime * SpeedOffset;
		this.killSpeed = killSpeed * SpeedOffset;
		this.e = param.E;
		this.PrevForce = prevForce;
	}

	[HideFromIl2Cpp]
	public void Initialize(Vector2 direction, float speed, float killSpeed, in Parameter param)
	{
		this.Initialize(speed * SpeedOffset * direction, killSpeed, param);
	}

	public void FixedUpdate()
	{
		PlayerControl pc = PlayerControl.LocalPlayer;

		if (pc == null ||
			pc.Data == null ||
			pc.Data.Disconnected ||
			pc.Data.IsDead ||
			!pc.CanMove ||
			pc.inMovingPlat ||
			pc.onLadder ||
			MeetingHud.Instance != null ||
			ExileController.Instance != null ||
			this.PrevForce.sqrMagnitude <= 0.01f)
		{
			Destroy(this);
			return;
		}

		Vector2 directionVector = Vector2.zero;
		var player = KeyboardJoystick.player;

		if (player.GetButton(40))
		{
			directionVector.x++;
		}
		if (player.GetButton(39))
		{
			directionVector.x--;
		}
		if (player.GetButton(44))
		{
			directionVector.y++;
		}
		if (player.GetButton(42))
		{
			directionVector.y--;
		}

		Vector2 forceVector =
			this.PrevForce + (this.detaTimeSpeedOfsset * directionVector) + (this.PrevForce.normalized * this.speed);

		var rigidBody = pc.rigidbody2D;
		Vector2 curPos = pc.transform.position;

		if (this.PrevForce != Vector2.zero &&
			(
				PhysicsHelpers.AnythingBetween(
					curPos, curPos + (this.PrevForce.normalized * offset),
					Constants.ShipAndObjectsMask, false) ||
				(curPos - this.prevPos).normalized == Vector2.zero
			))
		{
			forceVector = -forceVector * this.e;
			playerKill(pc.PlayerId);
		}

		this.prevPos = curPos;
		this.PrevForce = forceVector;

		rigidBody.AddForce(forceVector);
	}

	private void playerKill(byte killer)
	{
		if (this.PrevForce.magnitude > this.killSpeed)
		{
			Player.RpcUncheckMurderPlayer(killer, killer, byte.MaxValue);
		}
	}
}
