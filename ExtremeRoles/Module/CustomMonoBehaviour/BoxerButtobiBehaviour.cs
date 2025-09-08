using System;

using UnityEngine;

using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles.Solo.Impostor;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class BoxerButtobiBehaviour : MonoBehaviour
{
	public enum CollisionPlayerMode
	{
		Reflect,
		ReflectWith,
		Kill,
		WithKill
	}

	public const float SpeedOffset = 32.0f;

	public readonly record struct Parameter(float Acceleration, float E, CollisionPlayerMode CollisionPlayerMode);

	[HideFromIl2Cpp]
	public Vector2 PrevForce { get; private set; } = Vector2.zero;

	private float speed;
	private float killSpeed;
	private float deltaTimeSpeedOffset;
	private float e;
	private readonly Vector2 offset = new Vector2(0.275f, 0.5f);
	private Vector2 prevPos = Vector2.zero;
	private CollisionPlayerMode playerMode;
	private byte rolePlayerId;

	public BoxerButtobiBehaviour(IntPtr ptr) : base(ptr) { }

	[HideFromIl2Cpp]
	public void Initialize(byte rolePlayerId, Vector2 prevForce, float killSpeed, in Parameter param)
	{
		this.rolePlayerId = rolePlayerId;
		this.deltaTimeSpeedOffset = SpeedOffset * Time.fixedDeltaTime;
		this.speed = param.Acceleration * this.deltaTimeSpeedOffset;
		this.killSpeed = killSpeed * this.deltaTimeSpeedOffset;
		this.e = param.E;
		this.playerMode = param.CollisionPlayerMode;
		this.PrevForce = prevForce;
	}

	[HideFromIl2Cpp]
	public void Initialize(byte rolePlayerId, Vector2 direction, float speed, float killSpeed, in Parameter param)
	{
		this.Initialize(rolePlayerId, speed * SpeedOffset * Time.fixedDeltaTime * direction, killSpeed, param);
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
			ExileController.Instance != null)
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
			this.PrevForce + (this.deltaTimeSpeedOffset * directionVector) + (this.PrevForce.normalized * this.speed);

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
		}

		this.prevPos = curPos;
		this.PrevForce = forceVector;

		rigidBody.AddForce(forceVector);
	}
	public void OnTriggerEnter2D(Collider2D other)
	{
		byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;
		if (ScavengerWeaponHitHelper.IsHitPlayer(other, out var pc))
		{
			byte hitPlayer = pc.PlayerId;
			if (hitPlayer == localPlayerId)
			{
				return;
			}

			switch (this.playerMode)
			{
				case CollisionPlayerMode.Reflect:
					return;
				case CollisionPlayerMode.ReflectWith:
					var targetForce = -this.PrevForce;
					using (var op = RPCOperator.CreateCaller(RPCOperator.Command.BoxerRpcOps))
					{
						op.WriteByte((byte)Boxer.RpcOps.Reflection);
						op.WriteByte(this.rolePlayerId);
						op.WriteByte(hitPlayer);
						op.WriteFloat(targetForce.x);
						op.WriteFloat(targetForce.y);
					}
					return;
				case CollisionPlayerMode.Kill:
					playerKill(localPlayerId, localPlayerId);
					break;
				case CollisionPlayerMode.WithKill:
					playerKill(localPlayerId, hitPlayer);
					playerKill(localPlayerId, localPlayerId);
					break;
				default:
					break;
			}
			Destroy(this);
		}
		else
		{
			playerKill(localPlayerId);
			Destroy(this);
		}
	}

	private void playerKill(byte killer, byte? target=null)
	{
		target ??= killer;

		if (this.PrevForce.magnitude > this.killSpeed)
		{
			Player.RpcUncheckMurderPlayer(killer, target.Value, byte.MaxValue);
		}
	}
}
