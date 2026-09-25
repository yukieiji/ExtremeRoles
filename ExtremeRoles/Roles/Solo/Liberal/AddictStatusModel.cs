using System;
using UnityEngine;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class AddictStatusModel(float maxTimer, float recoveryTime) : IStatusModel
{
	private static readonly Vector2 defaultPos = new Vector2(100.0f, 100.0f);

	public float MaxSelfKillTimer { get; } = maxTimer;
	public float MovementTimeToRecover { get; } = recoveryTime;

	public float CurrentSelfKillTimer { get; internal set; } = maxTimer;
	public float CurrentMovementTime { get; internal set; } = 0.0f;
	public Vector2 PrevPlayerPos { get; internal set; } = defaultPos;
	public bool HasExploded { get; internal set; } = false;

	public void Reset()
	{
		this.CurrentSelfKillTimer = this.MaxSelfKillTimer;
		this.CurrentMovementTime = 0.0f;
		this.PrevPlayerPos = defaultPos;
	}

	public bool UpdateTimer(PlayerControl rolePlayer, float deltaTime)
	{
		if (this.HasExploded || rolePlayer.IsInValid())
		{
			return false;
		}

		var curPos = rolePlayer.GetTruePosition();

		float initDx = this.PrevPlayerPos.x - defaultPos.x;
		float initDy = this.PrevPlayerPos.y - defaultPos.y;
		if ((initDx * initDx + initDy * initDy) <= 0.01f)
		{
			this.PrevPlayerPos = curPos;
		}

		float dx = this.PrevPlayerPos.x - curPos.x;
		float dy = this.PrevPlayerPos.y - curPos.y;
		bool isMoving = rolePlayer.CanMove &&
			Minigame.Instance == null &&
			!rolePlayer.inVent &&
			(dx * dx + dy * dy) > 0.001f;

		this.PrevPlayerPos = curPos;

		if (isMoving)
		{
			this.CurrentMovementTime += deltaTime;
			if (this.CurrentMovementTime >= this.MovementTimeToRecover)
			{
				this.CurrentSelfKillTimer = Math.Min(this.MaxSelfKillTimer, this.CurrentSelfKillTimer + deltaTime);
			}
		}
		else
		{
			this.CurrentMovementTime = 0.0f;
			this.CurrentSelfKillTimer -= deltaTime;

			if (this.CurrentSelfKillTimer <= 0.0f)
			{
				this.CurrentSelfKillTimer = 0.0f;
				this.HasExploded = true;
				return true;
			}
		}

		return false;
	}
}
