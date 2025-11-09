using System;
using System.Collections.Generic;

using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.GameResult.WinnerProcessor;

namespace ExtremeRoles.Module.GameResult;

#nullable enable

public sealed class WinnerBuilder : IDisposable
{
	private readonly WinnerInitializer initializer;

	private readonly IReadOnlyList<IWinnerProcessor> processors;

	public WinnerBuilder(
		int winGameControlId,
		IReadOnlyDictionary<byte, ExtremeGameResultManager.TaskInfo> taskInfo)
	{
		var state = ExtremeRolesPlugin.ShipState;
		var finalSummaryBuilder = new PlayerSummaryBuilder(
			state.EndReason,
			state.DeadPlayerInfo,
			taskInfo);

		this.initializer = new WinnerInitializer(finalSummaryBuilder);

		this.processors = [
			new RemoveAddPlusWinnerProcessor(),
			new AddNeutralWinnerProcessor(),
			new ReplaceWinnerProcessor(winGameControlId),
			new MergeWinnerProcessor(),
			new AddGhostRoleWinnerProcessor(),
			new ModifiedWinnerProcessor()
		];
	}

	public IReadOnlyList<FinalSummary.PlayerSummary> Build(WinnerContainer tempData)
	{
		var logger = ExtremeRolesPlugin.Logger;

		logger.LogInfo("---- Start: Creating Winner ----");

		var result = this.initializer.Initialize(tempData);

		foreach (var processor in this.processors)
		{
			processor.Process(tempData, result.Winner);
		}
		logger.LogInfo("--- End: Creating Winner ----");

#if DEBUG
		logger.LogInfo(tempData.ToString());
#endif
		return result.Summary;
	}


	public void Dispose()
	{
		this.initializer.Dispose();
	}
}
