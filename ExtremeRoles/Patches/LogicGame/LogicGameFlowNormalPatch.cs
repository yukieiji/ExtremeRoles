using System.Linq;

using HarmonyLib;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Patches.LogicGame;

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
public static class LogicGameFlowNormalIsGameOverDueToDeathPatch
{
    public static void Postfix(ref bool __result)
    {
        __result = false;
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public static class LogicGameFlowNormalCheckEndCriteriaPatch
{
    public static bool Prefix()
    {
        if (!GameData.Instance)
		{
			return false;
		}
        if (TutorialManager.InstanceExists)
		{
			return true;
		}

		checkExRWin();

		return false;
    }

	private static void checkExRWin()
	{
		// InstanceExists | Don't check Custom Criteria when in Tutorial
		if (HudManager.Instance.IsIntroDisplayed ||
			ExtremeRolesPlugin.ShipState.IsDisableWinCheck)
		{
			return;
		}

		if (OnemanMeetingSystemManager.TryGetActiveSystem(out var system))
		{
			isOnemanMeetingWin(system);
			return;
		}
		else if (isSabotageWin())
		{
			return;
		}
		else if (isTaskWin())
		{
			return;
		}

		var statistics = PlayerStatistics.Create();

		if (isSpecialRoleWin(statistics))
		{
			return;
		}
		else if (isNeutralSpecialWin())
		{
			return;
		}
		else if (isNeutralAliveWin(statistics))
		{
			return;
		}
		else if (statistics.SeparatedNeutralAlive.Count != 0)
		{
			return;
		}
		else if (isImpostorWin(statistics))
		{
			return;
		}
		else if (isCrewmateWin(statistics))
		{
			return;
		}
	}

    private static void gameIsEnd(
        GameOverReason reason,
        bool trigger = false)
    {
        ShipStatus.Instance.enabled = false;
        GameManager.Instance.RpcEndGame(reason, trigger);
		GameProgressSystem.Current = GameProgressSystem.Progress.None;
	}

    private static bool isCrewmateWin(PlayerStatistics statistics)
    {
        if ((
				statistics.TeamCrewmateAlive > 0 &&
				statistics.TeamImpostorAlive == 0 &&
				statistics.SeparatedNeutralAlive.Count == 0
			)
			||
			(
				statistics.TeamCrewmateAlive <= 0 &&
				statistics.TeamImpostorAlive <= 0 &&
				statistics.SeparatedNeutralAlive.Count <= 0 &&
				statistics.TotalAlive == statistics.TeamNeutralAlive
			))
        {
            gameIsEnd(GameOverReason.CrewmatesByVote);
            return true;
        }
        return false;
    }

    private static bool isImpostorWin(
        PlayerStatistics statistics)
    {
        if (statistics.TeamImpostorAlive >= (statistics.TotalAlive - statistics.TeamImpostorAlive) &&
            statistics.SeparatedNeutralAlive.Count == 0)
        {
            GameOverReason endReason = GameData.LastDeathReason switch
            {
                DeathReason.Exile => GameOverReason.ImpostorsByVote,
                DeathReason.Kill  => GameOverReason.ImpostorsByKill,
                _                 => GameOverReason.CrewmateDisconnect,
            };
            gameIsEnd(endReason);
            return true;
        }

        return false;

    }
    private static void isOnemanMeetingWin(OnemanMeetingSystemManager meeting)
    {
		// 会議中に終了するのは困るので・・・
		if (MeetingHud.Instance != null ||
			ExileController.Instance != null)
		{
			return;
		}

		if (meeting.TryGetGameEndReason(out var reson))
        {
            gameIsEnd((GameOverReason)reson);
        }
    }

    private static bool isNeutralAliveWin(
        PlayerStatistics statistics)
    {
        if (statistics.SeparatedNeutralAlive.Count != 1)
		{
			return false;
		}

        var ((team, id), num) = statistics.SeparatedNeutralAlive.ElementAt(0);

        if (num < (statistics.TotalAlive - num))
		{
			return false;
		}

		RoleGameOverReason endReason = RoleGameOverReason.UnKnown;

		// アリス vs インポスターは絶対にインポスターが勝てないので
		// 別の殺人鬼が存在しないかつ、生存者数がアリスの生存者以下になれば勝利
		if (team is NeutralSeparateTeam.Alice)
		{
			endReason = RoleGameOverReason.AliceKillAllOther;
		}
		else if (
			// 以下は全てインポスターと勝負しても問題ないのでインポスターが生きていると勝利できない
			// アサシンがキルできないオプションのとき、ニュートラルの勝ち目が少なくなるので、勝利とする
			statistics.TeamImpostorAlive > 0 &&
			statistics.TeamImpostorAlive != statistics.AssassinAlive)
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

		setWinGameContorlId(id);
		gameIsEnd((GameOverReason)endReason);
		return true;
	}

    private static bool isNeutralSpecialWin()
    {

        if (ExtremeGameModeManager.Instance.ShipOption.DisableNeutralSpecialForceEnd) { return false; }

        foreach (var role in ExtremeRoleManager.GameRole.Values)
        {

            if (!(role.IsWin && role.IsNeutral()))
            {
				continue;
            }

			setWinGameContorlId(role.GameControlId);

			GameOverReason endReason = (GameOverReason)(role.Core.Id switch
			{
				ExtremeRoleId.Alice => RoleGameOverReason.AliceKilledByImposter,
				ExtremeRoleId.TaskMaster => RoleGameOverReason.TaskMasterGoHome,
				ExtremeRoleId.Jester => RoleGameOverReason.JesterMeetingFavorite,
				ExtremeRoleId.Eater => RoleGameOverReason.EaterAllEatInTheShip,
				ExtremeRoleId.Umbrer => RoleGameOverReason.UmbrerBiohazard,
				ExtremeRoleId.Hatter => RoleGameOverReason.HatterEndlessTeaTime,
				ExtremeRoleId.Artist => RoleGameOverReason.ArtistShipToArt,
				_ => RoleGameOverReason.UnKnown,
			});
			gameIsEnd(endReason);
			return true;
		}

        return false;
    }

    private static bool isSpecialRoleWin(
        PlayerStatistics statistics)
    {
        if (statistics.SpecialWinCheckRoleAlive.Count == 0)
		{
			return false;
		}
        foreach (var (id, checker) in statistics.SpecialWinCheckRoleAlive)
        {
            if (checker.IsWin(statistics))
            {
                setWinGameContorlId(id);
                gameIsEnd((GameOverReason)checker.Reason);
                return true;
            }
        }
        return false;
    }

    private static bool isSabotageWin()
    {

        var systems = ShipStatus.Instance.Systems;

        if (systems == null) { return false; };

        const GameOverReason gameOverReason = GameOverReason.ImpostorsBySabotage;

        ISystemType systemType;
        if (systems.TryGetValue(SystemTypes.LifeSupp, out systemType))
        {
            LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
            if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
            {
                gameIsEnd(gameOverReason);
                lifeSuppSystemType.Countdown = 10000f;
                return true;
            }
        }

		// 公式だとFor文を回してるけどO(n)かかるのはちょっとってことで直接
        if (systems.TryGetValue(SystemTypes.Reactor, out systemType) ||
            systems.TryGetValue(SystemTypes.Laboratory, out systemType) ||
			systems.TryGetValue(SystemTypes.HeliSabotage, out systemType))
        {
            ICriticalSabotage criticalSystem = systemType.TryCast<ICriticalSabotage>();

            if (criticalSystem != null &&
                criticalSystem.Countdown < 0f)
            {
                gameIsEnd(gameOverReason);
                criticalSystem.ClearSabotage();
                return true;
            }
        }

		var typeMng = ExtremeSystemTypeManager.Instance;

		if (typeMng.TryGet<TeroristTeroSabotageSystem>(
				TeroristTeroSabotageSystem.SystemType, out var teroSabo) &&
			teroSabo.ExplosionTimer < 0.0f)
		{
			teroSabo.Clear();
			gameIsEnd((GameOverReason)RoleGameOverReason.TeroristoTeroWithShip);
			return false;
		}

        return false;
    }
    private static bool isTaskWin()
    {
		var gameData = GameData.Instance;
		if (gameData == null)
		{
			return false;
		}

		gameData.RecomputeTaskCounts();

		if (gameData.TotalTasks > 0 &&
			gameData.CompletedTasks >= gameData.TotalTasks)
        {
            gameIsEnd(GameOverReason.CrewmatesByTask);
            return true;
        }
        return false;
    }

    private static void setWinGameContorlId(int id)
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.SetWinGameControlId))
        {
            caller.WriteInt(id);
        }
        RPCOperator.SetWinGameControlId(id);
    }
}
