using System;
using System.Collections.Generic;

using ExtremeRoles.Module.CustomMonoBehaviour;

namespace ExtremeRoles.Module.GameResult;

public readonly record struct RoleHistory(
	byte FromId,
	string FromRoleName,
	string TargetPrevRoleName,
	string TargetNextRoleName);


public sealed class RoleHistoryContainer(SummaryTextBuilder builder) : IDisposable
{
	private static readonly Dictionary<byte, Queue<RoleHistory>> history = [];
	private readonly SummaryTextBuilder builder = builder;

	public void BuildSummaryText(IReadOnlyDictionary<byte, FinalSummary.PlayerSummary> allSummary)
	{
		foreach (var (playerId, summary) in allSummary)
		{
			if (!history.TryGetValue(playerId, out var hist) &&
				hist is not null)
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
			$"<pos=10%>{hist.TargetPrevRoleName} => {hist.TargetNextRoleName}");
		this.builder.AppendLine(
			$"<pos=15%>(原因 : {summary.PlayerName} {hist.FromRoleName})");
	}

	public void Dispose()
	{
		history.Clear();
	}
}