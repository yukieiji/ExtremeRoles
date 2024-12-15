using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Module.SystemType.SecurityDummySystem;

public sealed class SecurityLogDummySystem : ISecurityDummySystem
{
	private readonly HashSet<byte> target = new HashSet<byte>();
	private readonly List<SecurityLogBehaviour.SecurityLogEntry> dummyLogEntry = new List<SecurityLogBehaviour.SecurityLogEntry>();
	private readonly List<SecurityLogBehaviour.SecurityLogEntry> noRemoveLogEntry = new List<SecurityLogBehaviour.SecurityLogEntry>();

	private SecurityLogBehaviour? logger;

	public void Add(params byte[] players)
	{
		lock (target)
		{
			foreach (byte playerId in players)
			{
				this.target.Add(playerId);
			}
		}
	}

	private const float xScale = 0.0001f;

	public void Begin()
	{
		if (!this.tryGetLogger(out var logger))
		{
			return;
		}

		foreach (var player in PlayerCache.AllPlayerControl)
		{
			if (player == null ||
				player.Data == null ||
				player.Data.Disconnected ||
				!this.target.Contains(player.PlayerId))
			{
				continue;
			}

			logger.HasNew = true;
			var pos = (SecurityLogBehaviour.SecurityLogLocations)RandomGenerator.Instance.Next(2);
			var dummyEntry = new SecurityLogBehaviour.SecurityLogEntry(player.PlayerId, pos);
			this.dummyLogEntry.Add(dummyEntry);
			logger.LogEntries.Add(dummyEntry);
			if (logger.LogEntries.Count > 20)
			{
				this.noRemoveLogEntry.Add(logger.LogEntries[0]);
				logger.LogEntries.RemoveAt(0);
			}
		}
	}

	public void Clear()
	{
		this.Close();
	}

	public void Close()
	{
		if (!this.tryGetLogger(out var logger))
		{
			return;
		}
		foreach (var entry in this.dummyLogEntry)
		{
			logger.LogEntries.Remove(entry);
		}
		this.dummyLogEntry.Clear();
		foreach (var entry in this.noRemoveLogEntry)
		{
			logger.LogEntries.Insert(0, entry);
		}
		while (logger.LogEntries.Count > 20)
		{
			logger.LogEntries.RemoveAt(0);
		}
	}

	public void Remove(params byte[] players)
	{
		lock (target)
		{
			foreach (byte playerId in players)
			{
				this.target.Remove(playerId);
			}
		}
	}

	private bool tryGetLogger([NotNullWhen(true)] out SecurityLogBehaviour? logger)
	{
		if (this.logger != null)
		{
			logger = this.logger;
			return true;
		}
		if (!CachedShipStatus.Instance.TryGetComponent(out this.logger))
		{
			logger = null;
			return false;
		}
		logger = this.logger;
		return logger != null;
	}
}
