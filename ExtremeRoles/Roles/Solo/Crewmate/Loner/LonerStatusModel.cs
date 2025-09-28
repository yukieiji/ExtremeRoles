using UnityEngine;

using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Crewmate.Loner;

public sealed class StressProgress(float range, float waitTime, StressProgress.Option option)
{
	public readonly record struct Option(bool ProgressOnTask, bool ProgressOnVentPlayer, bool ProgressOnMovingPlatPlayer);

	private readonly float range = range;
	private readonly float waitTime = waitTime;
	private readonly Option option = option;

	private float waitTimer = waitTime;

	public float StressGage { get; private set; } = 0.0f;

	public void Update(PlayerControl rolePlayer, in float deltaTime)
	{
		if (rolePlayer == null ||
			MeetingHud.Instance != null ||
			ExileController.Instance != null ||
			ShipStatus.Instance == null ||
			!ShipStatus.Instance.enabled)
		{
			this.waitTimer = waitTime;
			return;
		}

		if (Minigame.Instance != null && 
			!this.option.ProgressOnTask)
		{
			return;
		}
		
		if (this.waitTimer >= 0.0f)
		{
			this.waitTimer -= deltaTime;
			return;
		}

		float stressDeltaNum = 0.0f;
		var truePos = rolePlayer.GetTruePosition();
		foreach (var playerInfo in
			GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (isValidPlayer(rolePlayer, playerInfo))
			{
				PlayerControl target = playerInfo.Object;

				var vector = target.GetTruePosition() - truePos;
				float magnitude = vector.magnitude;
				if (magnitude <= this.range)
				{
					stressDeltaNum += deltaTime;
				}
			}
		}
		
		stressDeltaNum = stressDeltaNum <= 0.0f ? -deltaTime : stressDeltaNum;

		this.StressGage = Mathf.Max(0.0f, this.StressGage + stressDeltaNum);
	}

	private bool isValidPlayer(
		PlayerControl sourcePlayer,
		NetworkedPlayerInfo targetPlayer)
	{
		if (targetPlayer == null)
		{
			return false;
		}
		byte targetPlayerId = targetPlayer.PlayerId;
		byte sourcePlayerId = sourcePlayer.PlayerId;

		return (
			targetPlayer.PlayerId != sourcePlayer.PlayerId &&
			!targetPlayer.Disconnected &&
			!targetPlayer.IsDead &&
			targetPlayer.Object != null &&
			(this.option.ProgressOnVentPlayer || !targetPlayer.Object.inVent) &&
			(this.option.ProgressOnMovingPlatPlayer || !targetPlayer.Object.inMovingPlat) &&
			(this.option.ProgressOnMovingPlatPlayer || !targetPlayer.Object.onLadder)
		);
	}
}

public sealed class LonerStatusModel(
	float stressRange, float stressWaitTime, StressProgress.Option stressOption) : IStatusModel
{
	private readonly StressProgress stress = new StressProgress(stressRange, stressWaitTime, stressOption);

	public float StressGage => this.stress.StressGage;

	public void Update(PlayerControl rolePlayer, in float deltaTime)
	{
		this.stress.Update(rolePlayer, deltaTime);
	}
}
