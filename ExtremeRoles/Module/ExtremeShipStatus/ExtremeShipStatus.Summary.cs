using System.Collections.Generic;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        public List<PlayerSummary> FinalSummary => this.finalSummary;
        private List<PlayerSummary> finalSummary = new List<PlayerSummary>();

        public void AddPlayerSummary(
            GameData.PlayerInfo playerInfo)
        {

            SingleRoleBase role = ExtremeRoleManager.GameRole[playerInfo.PlayerId];
            var (completedTask, totalTask) = Helper.GameSystem.GetTaskInfo(playerInfo);
            // IsImpostor
            PlayerStatus finalStatus = PlayerStatus.Alive;

            if (this.reason == GameOverReason.ImpostorBySabotage &&
                !role.IsImpostor())
            {
                finalStatus = PlayerStatus.Dead;
            }
            else if (this.reason == (GameOverReason)RoleGameOverReason.AssassinationMarin)
            {
                if (playerInfo.PlayerId == IsMarinPlayerId)
                {
                    if (playerInfo.IsDead || playerInfo.Disconnected)
                    {
                        finalStatus = PlayerStatus.DeadAssassinate;
                    }
                    else
                    {
                        finalStatus = PlayerStatus.Assassinate;
                    }
                }
                else if (playerInfo.PlayerId == ExiledAssassinId)
                {
                    if (this.deadPlayerInfo.TryGetValue(
                        playerInfo.PlayerId, out DeadInfo info))
                    {
                        finalStatus = info.Reason;
                    }
                }
                else if (!role.IsImpostor())
                {
                    finalStatus = PlayerStatus.Surrender;
                }
            }
            else if (this.reason == (GameOverReason)RoleGameOverReason.UmbrerBiohazard)
            {
                if (role.Id != ExtremeRoleId.Umbrer &&
                    !playerInfo.IsDead &&
                    !playerInfo.Disconnected)
                {
                    finalStatus = PlayerStatus.Zombied;
                }
                else
                {
                    if (deadPlayerInfo.TryGetValue(
                        playerInfo.PlayerId, out DeadInfo info))
                    {
                        finalStatus = info.Reason;
                    }
                }
            }
            else if (playerInfo.Disconnected)
            {
                finalStatus = PlayerStatus.Disconnected;
            }
            else
            {
                if (this.deadPlayerInfo.TryGetValue(
                    playerInfo.PlayerId, out DeadInfo info))
                {
                    finalStatus = info.Reason;
                }
            }

            this.finalSummary.Add(
                new PlayerSummary
                {
                    PlayerName = playerInfo.PlayerName,
                    Role = role,
                    StatusInfo = finalStatus,
                    TotalTask = totalTask,
                    CompletedTask = EndReason == GameOverReason.HumansByTask ? totalTask : completedTask,
                });
        }

        private void resetPlayerSummary()
        {
            this.finalSummary.Clear();
        }

        public sealed class PlayerSummary
        {
            public string PlayerName { get; set; }
            public SingleRoleBase Role { get; set; }
            public int CompletedTask { get; set; }
            public int TotalTask { get; set; }
            public PlayerStatus StatusInfo { get; set; }
        }
    }
}
