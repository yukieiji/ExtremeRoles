using System.Linq;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.GameEnd;

public sealed class NeutralAliveWinChecker(PlayerStatistics statistics) : IGameEndChecker
{
	private readonly PlayerStatistics statistics = statistics;

	public bool TryCheckGameEnd(out GameOverReason reason)
	{
		reason = (GameOverReason)RoleGameOverReason.UnKnown;

		if (this.statistics.SeparatedNeutralAlive.Count != 1 ||
			this.statistics.LiberalMilitantAlive > 0)
		{
			return false;
		}

		var ((team, id), num) = this.statistics.SeparatedNeutralAlive.ElementAt(0);

		if (num < (this.statistics.TotalAlive - num))
		{
			return false;
		}

		var endReason = RoleGameOverReason.UnKnown;

		// アリス vs インポスターは絶対にインポスターが勝てないので
		// 別の殺人鬼が存在しないかつ、生存者数がアリスの生存者以下になれば勝利
		if (team is NeutralSeparateTeam.Alice)
		{
			endReason = RoleGameOverReason.AliceKillAllOther;
		}
		else if (
			// 以下は全てインポスターと勝負しても問題ないのでインポスターが生きていると勝利できない
			// アサシンがキルできないオプションのとき、ニュートラルの勝ち目が少なくなるので、勝利とする
			this.statistics.TeamImpostorAlive > 0 &&
			this.statistics.TeamImpostorAlive != this.statistics.AssassinAlive)
		{
			return false;
		}
		else
		{
			endReason = team switch
			{
				NeutralSeparateTeam.Jackal => RoleGameOverReason.JackalKillAllOther,
				NeutralSeparateTeam.Lover => RoleGameOverReason.LoverKillAllOther,
				NeutralSeparateTeam.Missionary => RoleGameOverReason.MissionaryAllAgainstGod,
				NeutralSeparateTeam.Yandere => RoleGameOverReason.YandereKillAllOther,
				NeutralSeparateTeam.Vigilante => RoleGameOverReason.VigilanteKillAllOther,
				NeutralSeparateTeam.Miner => RoleGameOverReason.MinerExplodeEverything,
				NeutralSeparateTeam.Eater => RoleGameOverReason.EaterAliveAlone,
				NeutralSeparateTeam.Traitor => RoleGameOverReason.TraitorKillAllOther,
				NeutralSeparateTeam.Queen => RoleGameOverReason.QueenKillAllOther,
				NeutralSeparateTeam.Kids => RoleGameOverReason.KidsAliveAlone,
				NeutralSeparateTeam.Tucker => RoleGameOverReason.TuckerShipIsExperimentStation,

				NeutralSeparateTeam.JackalSub => RoleGameOverReason.AllJackalWin,
				NeutralSeparateTeam.YandereSub => RoleGameOverReason.AllYandereWin,
				NeutralSeparateTeam.QueenSub => RoleGameOverReason.AllQueenWin,

				_ => RoleGameOverReason.UnKnown
			};
		}

		if (endReason is RoleGameOverReason.UnKnown)
		{
			return false;
		}

		IGameEndChecker.SetWinGameContorlId(id);
		reason = (GameOverReason)endReason;
		return true;
	}
}
