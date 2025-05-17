using System;
using System.Collections.Generic;

using ExtremeRoles.Module.CustomMonoBehaviour;

namespace ExtremeRoles.Module.GameResult;

#nullable enable

public readonly record struct RoleHistory(
	byte FromId,
	string FromRoleName,
	string TargetPrevRoleName,
	string TargetNextRoleName);


public static class RoleHistoryContainer
{
	public sealed class SummaryBuilder(
		SummaryTextBuilder builder, IReadOnlyDictionary<byte, Queue<RoleHistory>> history) : IDisposable
	{
		private readonly SummaryTextBuilder builder = builder;
		private readonly IReadOnlyDictionary<byte, Queue<RoleHistory>> history = history;

		public void Build(IReadOnlyDictionary<byte, FinalSummary.PlayerSummary> allSummary)
		{
			foreach (var (playerId, summary) in allSummary)
			{
				if (!history.TryGetValue(playerId, out var hist) ||
					hist is null)
				{
					continue;
				}

				this.builder.AppendLine($"{summary.PlayerName}");
				while (hist.TryDequeue(out var roleHistory))
				{
					appendHistory(roleHistory, allSummary);
				}
				this.builder.AppendLine();
			}
		}

		private void appendHistory(RoleHistory hist, IReadOnlyDictionary<byte, FinalSummary.PlayerSummary> allSummary)
		{
			if (!allSummary.TryGetValue(hist.FromId, out var summary))
			{
				return;
			}
			this.builder.AppendLine(
				$"<pos=3%>{hist.TargetPrevRoleName} => {hist.TargetNextRoleName}");
			this.builder.AppendLine(
				$"<pos=5%>{Tr.GetString("roleHistoryCause")}: {summary.PlayerName}({hist.FromRoleName})");
		}
		public void Dispose()
		{
			RoleHistoryContainer.history.Clear();
		}
	}

	private static readonly Dictionary<byte, Queue<RoleHistory>> history = [];

	public static SummaryBuilder CreateBuiler(SummaryTextBuilder builder)
		=> new SummaryBuilder(builder, history);

	public static void Add(byte target, RoleHistory hist)
	{
		lock (history)
		{
			if (!history.TryGetValue(target, out var roleHist))
			{
				roleHist = [];
			}
			roleHist.Enqueue(hist);
			history[target] = roleHist;
		}
	}
}