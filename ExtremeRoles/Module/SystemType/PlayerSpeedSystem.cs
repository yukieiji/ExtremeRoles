using System;
using System.Collections.Generic;
using Hazel;
using ExtremeRoles.Module.Interface;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.SystemType;

public sealed class PlayerSpeedSystem : IExtremeSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.PlayerSpeedSystem;

	private enum Op : byte
	{
		Set,
		Remove,
		ClearPlayer
	}

	private readonly Dictionary<byte, Dictionary<string, float>> speedModifiers = new();

	public static PlayerSpeedSystem Get()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<PlayerSpeedSystem>(Type);

	public void SetSpeed(byte playerId, string speedId, float multiplier)
	{
		if (!this.speedModifiers.TryGetValue(playerId, out var playerMods))
		{
			playerMods = new Dictionary<string, float>();
			this.speedModifiers[playerId] = playerMods;
		}
		playerMods[speedId] = multiplier;
	}

	public void RemoveSpeed(byte playerId, string speedId)
	{
		if (this.speedModifiers.TryGetValue(playerId, out var playerMods))
		{
			playerMods.Remove(speedId);
			if (playerMods.Count == 0)
			{
				this.speedModifiers.Remove(playerId);
			}
		}
	}

	public void ClearPlayerSpeed(byte playerId)
	{
		this.speedModifiers.Remove(playerId);
	}

	public static void RpcSetSpeed(byte targetPlayerId, string speedId, float multiplier)
	{
		Get().SetSpeed(targetPlayerId, speedId, multiplier);
		ExtremeSystemTypeManager.RpcUpdateSystem(Type, writer =>
		{
			writer.Write((byte)Op.Set);
			writer.Write(targetPlayerId);
			writer.Write(speedId);
			writer.Write(multiplier);
		});
	}

	public static void RpcRemoveSpeed(byte targetPlayerId, string speedId)
	{
		Get().RemoveSpeed(targetPlayerId, speedId);
		ExtremeSystemTypeManager.RpcUpdateSystem(Type, writer =>
		{
			writer.Write((byte)Op.Remove);
			writer.Write(targetPlayerId);
			writer.Write(speedId);
		});
	}

	public static void RpcClearPlayerSpeed(byte targetPlayerId)
	{
		Get().ClearPlayerSpeed(targetPlayerId);
		ExtremeSystemTypeManager.RpcUpdateSystem(Type, writer =>
		{
			writer.Write((byte)Op.ClearPlayer);
			writer.Write(targetPlayerId);
		});
	}

	public bool TryGetVelocity(byte playerId, out float velocity)
	{
		velocity = 1.0f;
		if (!this.speedModifiers.TryGetValue(playerId, out var playerMods) || playerMods.Count == 0)
		{
			return false;
		}

		float combined = 1.0f;
		foreach (float mod in playerMods.Values)
		{
			combined *= mod;
		}

		velocity = combined;
		return true;
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (resetPlayer != null)
		{
			this.speedModifiers.Remove(resetPlayer.PlayerId);
		}
		else
		{
			this.speedModifiers.Clear();
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		Op op = (Op)msgReader.ReadByte();
		switch (op)
		{
			case Op.Set:
				{
					byte targetPlayerId = msgReader.ReadByte();
					string speedId = msgReader.ReadString();
					float multiplier = msgReader.ReadSingle();
					SetSpeed(targetPlayerId, speedId, multiplier);
					break;
				}
			case Op.Remove:
				{
					byte targetPlayerId = msgReader.ReadByte();
					string speedId = msgReader.ReadString();
					RemoveSpeed(targetPlayerId, speedId);
					break;
				}
			case Op.ClearPlayer:
				{
					byte targetPlayerId = msgReader.ReadByte();
					ClearPlayerSpeed(targetPlayerId);
					break;
				}
		}
	}
}
