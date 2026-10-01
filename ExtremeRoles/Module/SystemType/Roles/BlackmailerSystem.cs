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

	private readonly Dictionary<byte, HashSet<byte>> blackmailedMap = new();

	public static bool TryGet([NotNullWhen(true)] out BlackmailerSystem? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(ExtremeSystemType.BlackmailerSystem, out system);

	public static BlackmailerSystem GetOrRegister()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<BlackmailerSystem>(ExtremeSystemType.BlackmailerSystem);

	public void RpcAddBlackmail(byte blackmailerId, byte targetPlayerId)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.BlackmailerSystem,
			x =>
			{
				x.Write((byte)Ops.AddBlackmail);
				x.Write(blackmailerId);
				x.Write(targetPlayerId);
			});
	}

	public void RpcClearBlackmail(byte blackmailerId)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.BlackmailerSystem,
			x =>
			{
				x.Write((byte)Ops.ClearBlackmail);
				x.Write(blackmailerId);
			});
	}

	public void MarkClean()
	{
		this.IsDirty = false;
	}

	public void Deteriorate(float deltaTime)
	{
		if (!GameProgressSystem.IsTaskPhase || this.blackmailedMap.Count == 0)
		{
			return;
		}

		List<byte> deadBlackmailers = new();

		foreach (byte blackmailerId in this.blackmailedMap.Keys)
		{
			var player = GameData.Instance.GetPlayerById(blackmailerId);
			if (player == null || player.IsDead || player.Disconnected)
			{
				deadBlackmailers.Add(blackmailerId);
			}
		}

		foreach (byte blackmailerId in deadBlackmailers)
		{
			clearBlackmail(blackmailerId);
			if (AmongUsClient.Instance.AmHost)
			{
				RpcClearBlackmail(blackmailerId);
			}
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(this.blackmailedMap.Count);
		foreach (var (blackmailerId, targets) in this.blackmailedMap)
		{
			writer.Write(blackmailerId);
			writer.WritePacked(targets.Count);
			foreach (byte targetId in targets)
			{
				writer.Write(targetId);
			}
		}
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.blackmailedMap.Clear();
		int bmCount = reader.ReadPackedInt32();
		for (int i = 0; i < bmCount; i++)
		{
			byte blackmailerId = reader.ReadByte();
			int targetCount = reader.ReadPackedInt32();
			var targets = new HashSet<byte>();
			for (int j = 0; j < targetCount; j++)
			{
				byte targetId = reader.ReadByte();
				targets.Add(targetId);
			}
			this.blackmailedMap[blackmailerId] = targets;
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is ResetTiming.MeetingEnd or ResetTiming.ExiledEnd)
		{
			this.blackmailedMap.Clear();
			this.IsDirty = true;
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		Ops ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.AddBlackmail:
				byte blackmailerId = msgReader.ReadByte();
				byte targetId = msgReader.ReadByte();
				addBlackmail(blackmailerId, targetId);
				break;
			case Ops.ClearBlackmail:
				byte bmIdToClear = msgReader.ReadByte();
				clearBlackmail(bmIdToClear);
				break;
		}
	}

	public bool IsBlackmailed(byte targetPlayerId)
	{
		foreach (var targets in this.blackmailedMap.Values)
		{
			if (targets.Contains(targetPlayerId))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsBlackmailedBy(byte blackmailerId, byte targetPlayerId)
		=> this.blackmailedMap.TryGetValue(blackmailerId, out var targets) && targets.Contains(targetPlayerId);

	public bool IsBlackmailed(PlayerControl player)
		=> IsBlackmailed(player.PlayerId);

	public bool IsBlackmailed(NetworkedPlayerInfo player)
		=> IsBlackmailed(player.PlayerId);

	private void addBlackmail(byte blackmailerId, byte targetPlayerId)
	{
		if (!this.blackmailedMap.TryGetValue(blackmailerId, out var targets))
		{
			targets = new HashSet<byte>();
			this.blackmailedMap[blackmailerId] = targets;
		}
		targets.Add(targetPlayerId);
		this.IsDirty = true;
	}

	private void clearBlackmail(byte blackmailerId)
	{
		if (this.blackmailedMap.Remove(blackmailerId))
		{
			this.IsDirty = true;
		}
	}
}
