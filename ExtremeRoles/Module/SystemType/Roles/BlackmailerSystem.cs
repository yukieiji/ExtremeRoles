using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Hazel;
using ExtremeRoles.Module.Interface;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class BlackmailerSystem : IExtremeSystemType
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
				ClearBlackmail(bmIdToClear);
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

	private void addBlackmail(byte blackmailerId, byte targetPlayerId)
	{
		if (!this.blackmailedMap.TryGetValue(blackmailerId, out var targets))
		{
			targets = new HashSet<byte>();
			this.blackmailedMap[blackmailerId] = targets;
		}
		targets.Add(targetPlayerId);
	}

	public void ClearBlackmail(byte blackmailerId)
	{
		this.blackmailedMap.Remove(blackmailerId);
	}
}
