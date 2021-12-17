using System;

using HarmonyLib;

using ExtremeRoles.Module;

namespace ExtremeRoles.Patches
{

    // ゲームが終了した瞬間の処理
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class AmongUsClientOnGameEndPatch
    {
        public static GameOverReason EndReason;
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            EndReason = endGameResult.GameOverReason;
            if ((int)endGameResult.GameOverReason >= 10)
            {
                endGameResult.GameOverReason = GameOverReason.ImpostorByKill;
            }
        }
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {

            foreach (var playerInfo in GameData.Instance.AllPlayers)
            {

                var role = Roles.ExtremeRoleManager.GameRole[playerInfo.PlayerId];
                var(completedTask, totalTask) = Helper.Task.GetTaskInfo(playerInfo);

                var finalStatus = PlayerDataContainer.PlayerStatus.Alive;
                if (playerInfo.Disconnected) { finalStatus = PlayerDataContainer.PlayerStatus.Disconnected; }
                else if (playerInfo.IsDead) { finalStatus = PlayerDataContainer.PlayerStatus.Dead; }
                else if (
                    (EndReason == GameOverReason.ImpostorBySabotage) && 
                    (!playerInfo.Role.IsImpostor)) { finalStatus = PlayerDataContainer.PlayerStatus.Dead; }

                PlayerDataContainer.EndGameAddStatus(
                    playerInfo, finalStatus, role, totalTask, completedTask);
            }
        }
    }
}
