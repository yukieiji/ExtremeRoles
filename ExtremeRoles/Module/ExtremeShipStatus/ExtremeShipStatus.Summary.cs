using System.Collections.Generic;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        public enum PlayerStatus
        {
            Alive = 0,
            Exiled,
            Dead,
            Killed,

            Suicide,
            MissShot,
            Retaliate,
            Departure,
            Martyrdom,

            Explosion,

            Assassinate,
            DeadAssassinate,
            Surrender,
            Zombied,

            Disconnected,
        }

        public List<PlayerSummary> FinalSummary => this.finalSummary;
        private List<PlayerSummary> finalSummary = new List<PlayerSummary>();

        public void AddPlayerSummary(
            GameData.PlayerInfo playerInfo)
        {

            byte playerId = playerInfo.PlayerId;

            SingleRoleBase role = ExtremeRoleManager.GameRole[playerId];
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
                if (playerId == IsMarinPlayerId)
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
                else if (playerId == ExiledAssassinId)
                {
                    if (this.deadPlayerInfo.TryGetValue(
                        playerId, out DeadInfo info))
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
                        playerId, out DeadInfo info))
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
                    playerId, out DeadInfo info))
                {
                    finalStatus = info.Reason;
                }
            }

            GhostRoleBase ghostRole = null;
            ExtremeGhostRoleManager.GameRole.TryGetValue(playerId, out ghostRole);

            this.finalSummary.Add(
                new PlayerSummary
                {
                    PlayerName = playerInfo.PlayerName,
                    Role = role,
                    GhostRole = ghostRole,
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
            public GhostRoleBase GhostRole { get; set; }
            public int CompletedTask { get; set; }
            public int TotalTask { get; set; }
            public PlayerStatus StatusInfo { get; set; }
        }
    }
}
