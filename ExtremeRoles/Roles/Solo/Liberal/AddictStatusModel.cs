using UnityEngine;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Liberal;

public class AddictStatusModel(float maxTimer, float recoveryTime) : IStatusModel
{
	private static Vector2 defaultPos => new Vector2(100.0f, 100.0f);

	public float MaxSelfKillTimer { get; } = maxTimer;
	public float MovementTimeToRecover { get; } = recoveryTime;

	public float CurrentSelfKillTimer { get; set; } = maxTimer;
	public float CurrentMovementTime { get; set; } = 0.0f;
	public Vector2 PrevPlayerPos { get; set; } = defaultPos;
	public bool HasExploded { get; set; } = false;

	public void Reset()
	{
		this.CurrentSelfKillTimer = this.MaxSelfKillTimer;
		this.CurrentMovementTime = 0.0f;
		this.PrevPlayerPos = defaultPos;
		this.HasExploded = false;
	}
}
