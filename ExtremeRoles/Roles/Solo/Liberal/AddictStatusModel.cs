using System;
using UnityEngine;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Extension.Vector;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class AddictStatusModel(float maxTimer, float recoveryTime) : IStatusModel
{
	private static readonly Vector2 defaultPos = new Vector2(100.0f, 100.0f);

	private float maxSelfKillTimer = maxTimer;
	private float movementTimeToRecover = recoveryTime;

	public float CurrentSelfKillTimer { get; private set; } = maxTimer;
	private float currentMovementTime = 0.0f;
	private Vector2 prevPlayerPos = defaultPos;
	private bool hasExploded = false;

	public void Reset()
	{
		this.CurrentSelfKillTimer = this.maxSelfKillTimer;
		this.currentMovementTime = 0.0f;
		this.prevPlayerPos = defaultPos;
	}

	public bool UpdateTimer(PlayerControl rolePlayer, float deltaTime)
	{
		if (this.hasExploded || rolePlayer.IsInValid())
		{
			return false;
		}

		var curPos = rolePlayer.GetTruePosition();

		if (this.prevPlayerPos.IsCloseTo(defaultPos, 0.01f))
		{
			this.prevPlayerPos = curPos;
		}

		bool isMoving = rolePlayer.CanMove &&
			Minigame.Instance == null &&
			!rolePlayer.inVent &&
			this.prevPlayerPos.IsNotCloseTo(curPos);

		this.prevPlayerPos = curPos;

		if (isMoving)
		{
			this.currentMovementTime += deltaTime;
			if (this.currentMovementTime >= this.movementTimeToRecover)
			{
				this.CurrentSelfKillTimer = Math.Min(this.maxSelfKillTimer, this.CurrentSelfKillTimer + deltaTime);
			}
		}
		else
		{
			this.currentMovementTime = 0.0f;
			this.CurrentSelfKillTimer -= deltaTime;

			if (this.CurrentSelfKillTimer <= 0.0f)
			{
				this.CurrentSelfKillTimer = 0.0f;
				this.hasExploded = true;
				return true;
			}
		}

		return false;
	}
}
