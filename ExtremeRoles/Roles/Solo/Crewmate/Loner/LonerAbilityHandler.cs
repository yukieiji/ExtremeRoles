using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.API.Interface.Ability;

namespace ExtremeRoles.Roles.Solo.Crewmate.Loner;

public sealed class ArrowController(int arrowNum, bool isShowOnVentPlayer)
{
	private readonly int arrowNum = arrowNum;
	private readonly bool isShowOnVentPlayer = isShowOnVentPlayer;

	private readonly Dictionary<byte, Arrow> arrow = new Dictionary<byte, Arrow>(GameData.Instance.AllPlayers.Count);
	private readonly SortedList<float, PlayerControl> cache = new SortedList<float, PlayerControl>(GameData.Instance.AllPlayers.Count);

	public void Hide()
	{
		foreach (var player in arrow.Values)
		{
			player.SetActive(false);
		}
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (this.arrowNum == 0 ||
			rolePlayer == null ||
			MeetingHud.Instance != null ||
			ExileController.Instance != null ||
			ShipStatus.Instance == null ||
			!ShipStatus.Instance.enabled)
		{
			return;
		}

		this.cache.Clear();
		var truePos = rolePlayer.GetTruePosition();
		foreach (var playerInfo in
			GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (!isValidPlayer(rolePlayer, playerInfo))
			{
				if (playerInfo != null &&
					this.arrow.TryGetValue(playerInfo.PlayerId, out var arrow) &&
					arrow is not null)
				{
					arrow.SetActive(false);
				}
				continue;
			}

			PlayerControl target = playerInfo.Object;

			var vector = target.GetTruePosition() - truePos;
			float magnitude = vector.magnitude;
			if (this.cache.ContainsKey(magnitude))
			{
				continue;
			}
			this.cache.Add(magnitude, target);
		}

		int num = 0;
		foreach (var player in this.cache.Values)
		{
			num++;
			byte playerId = player.PlayerId;
			if (!this.arrow.TryGetValue(playerId, out var arrow) ||
				arrow is null)
			{
				arrow = new Arrow(ColorPalette.LonerMidnightblue);
				this.arrow[playerId] = arrow;
			}
			arrow.SetActive(num <= this.arrowNum);
			arrow.UpdateTarget(player.GetTruePosition());
		}
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
			(this.isShowOnVentPlayer || !targetPlayer.Object.inVent)
		);
	}
}

public sealed class LonerAbilityHandler(
	float maxStressGage,
	int arrowNum, bool arrowIsShowOnVentPlayer,
	LonerStatusModel status) : IAbility
{
	public float MaxStressGage { get; } = maxStressGage;
	private readonly LonerStatusModel status = status;
	private readonly ArrowController arrow = new ArrowController(arrowNum, arrowIsShowOnVentPlayer);

	public void Update(PlayerControl rolePlayer)
    {
		if (IntroCutscene.Instance != null)
		{
			return;
		}

		if (rolePlayer == null ||
			rolePlayer.Data == null ||
			rolePlayer.Data.IsDead ||
			rolePlayer.Data.Disconnected)
		{
			this.arrow.Hide();
			return;
		}

		this.status.Update(rolePlayer, Time.deltaTime);
		this.arrow.Update(rolePlayer);

		if (this.status.StressGage < this.MaxStressGage)
		{
			return;
		}
		byte playerId = rolePlayer.PlayerId;
		Player.RpcUncheckMurderPlayer(
			playerId, playerId,
			byte.MinValue);
		ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(playerId, Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus.Despair);
	}
	public void Reset()
	{
		this.arrow.Hide();
	}
}
