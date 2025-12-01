using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module.CustomMonoBehaviour;

namespace ExtremeRoles.Module.GameResult;

public static class LiberalMoneyHistory
{
	public enum Reason
	{
		AddOnTask,
		AddOnKill
	}

	public readonly record struct MoneyHistory(Reason Reason, byte PlayerId, float Amount);

	public sealed class SummaryBuilder(
		SummaryTextBuilder builder, Queue<MoneyHistory> history) : IDisposable
	{
		private readonly SummaryTextBuilder builder = builder;
		private readonly Queue<MoneyHistory> history = history;

		public void Build(IReadOnlyDictionary<byte, FinalSummary.PlayerSummary> allSummary)
		{
			while (this.history.TryDequeue(out var hist))
			{
				if (!allSummary.TryGetValue(hist.PlayerId, out var summary))
				{
					return;
				}
				this.builder.AppendLine(
					$"<pos=3%>・ {summary.PlayerName} : {Mathf.CeilToInt(hist.Amount)} ({hist.Reason})");
			}
		}

		public void Dispose()
		{
			LiberalMoneyHistory.history.Clear();
		}
	}

	private static readonly Queue<MoneyHistory> history = [];

	public static SummaryBuilder CreateBuiler(SummaryTextBuilder builder)
		=> new SummaryBuilder(builder, history);

	public static void Add(MoneyHistory hist)
	{
		lock (history)
		{
			history.Enqueue(hist);
		}
	}
}
