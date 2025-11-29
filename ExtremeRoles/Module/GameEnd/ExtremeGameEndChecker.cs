using System.Collections.Generic;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;

#nullable enable

namespace ExtremeRoles.Module.GameEnd;

public sealed class ExtremeGameEndChecker
{
	private readonly IReadOnlyList<IGameEndChecker> checkers;
	private readonly PlayerStatistics statistics = new PlayerStatistics();

	public ExtremeGameEndChecker()
	{
		List<IGameEndChecker> checkers = [
			new OnemanMeetingEndChecker(),
			new SabotageEndChecker(),
			new TaskEndChecker(),
		];

		LiberalMoneyBankSystem? system = null;
		if (ExtremeSystemTypeManager.Instance.TryGet(LiberalMoneyBankSystem.SystemType, out system))
		{
			checkers.Add(new LiberalMoneyWinChecker(system));
		}

		checkers.Add(new SpecialRoleWinChecker(this.statistics));

		if (!ExtremeGameModeManager.Instance.ShipOption.DisableNeutralSpecialForceEnd)
		{
			checkers.Add(new NeutralSpecialWinChecker());
		}
		checkers.Add(new NeutralAliveWinChecker(this.statistics));

		checkers.Add(new ImpostorAliveWinChecker(this.statistics));
		checkers.Add(new CrewmateAliveWinChecker(this.statistics));

		if (system is not null)
		{
			checkers.Add(new LiberalAliveWinChecker(this.statistics));
		}

		this.checkers = checkers;
	}

	public void Check()
	{
		this.statistics.Update();

		foreach (var check in this.checkers)
		{
			if (!check.TryCheckGameEnd(out var reason))
			{
				continue;
			}
			gameIsEnd(reason);
			check.CleanUp();
			break;
		}
	}

	private static void gameIsEnd(
		GameOverReason reason)
	{
		ShipStatus.Instance.enabled = false;
		GameManager.Instance.RpcEndGame(reason, false);
		GameProgressSystem.Current = GameProgressSystem.Progress.None;
	}
}
