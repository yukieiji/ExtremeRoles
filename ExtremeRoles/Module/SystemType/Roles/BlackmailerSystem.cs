using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Hazel;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Resources;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class BlackmailerSystem : IDirtableSystemType
{
	public bool IsDirty { get; set; } = false;

	public enum Ops : byte
	{
		AddBlackmail,
		ClearBlackmail,
	}

	private readonly HashSet<byte> blackmailedPlayers = new();

	public static bool TryGet([NotNullWhen(true)] out BlackmailerSystem? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(ExtremeSystemType.BlackmailerSystem, out system);

	public static BlackmailerSystem GetOrRegister()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<BlackmailerSystem>(ExtremeSystemType.BlackmailerSystem);

	public void RpcAddBlackmail(byte targetPlayerId)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.BlackmailerSystem,
			x =>
			{
				x.Write((byte)Ops.AddBlackmail);
				x.Write(targetPlayerId);
			});
	}

	public void RpcClearBlackmail()
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.BlackmailerSystem,
			x =>
			{
				x.Write((byte)Ops.ClearBlackmail);
			});
	}

	public void MarkClean()
	{
		this.IsDirty = false;
	}

	public void Deteriorate(float deltaTime)
	{
		// 役職者（ブラックメーラー）が生存しているか確認。死亡していたら即解除
		if (!GameProgressSystem.IsTaskPhase)
		{
			return;
		}

		if (this.blackmailedPlayers.Count > 0)
		{
			bool isBlackmailerAlive = false;
			foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
			{
				if (player == null || player.IsDead || player.Disconnected)
				{
					continue;
				}
				if (ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) &&
					role.Core.Id is ExtremeRoleId.Blackmailer)
				{
					isBlackmailerAlive = true;
					break;
				}
			}

			if (!isBlackmailerAlive)
			{
				clearBlackmail();
				if (AmongUsClient.Instance.AmHost)
				{
					RpcClearBlackmail();
				}
			}
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(this.blackmailedPlayers.Count);
		foreach (byte id in this.blackmailedPlayers)
		{
			writer.Write(id);
		}
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.blackmailedPlayers.Clear();
		int count = reader.ReadPackedInt32();
		for (int i = 0; i < count; i++)
		{
			byte id = reader.ReadByte();
			this.blackmailedPlayers.Add(id);
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is ResetTiming.MeetingEnd or ResetTiming.ExiledEnd)
		{
			clearBlackmail();
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		Ops ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.AddBlackmail:
				byte target = msgReader.ReadByte();
				addBlackmail(target);
				break;
			case Ops.ClearBlackmail:
				clearBlackmail();
				break;
		}
	}

	public bool HasBlackmailedAny => this.blackmailedPlayers.Count > 0;

	public bool IsBlackmailed(byte playerId)
		=> this.blackmailedPlayers.Contains(playerId);

	public bool IsBlackmailed(PlayerControl player)
		=> IsBlackmailed(player.PlayerId);

	public bool IsBlackmailed(NetworkedPlayerInfo player)
		=> IsBlackmailed(player.PlayerId);

	private void addBlackmail(byte targetPlayerId)
	{
		this.blackmailedPlayers.Add(targetPlayerId);
		this.IsDirty = true;
	}

	private void clearBlackmail()
	{
		this.blackmailedPlayers.Clear();
		this.IsDirty = true;
	}
}
