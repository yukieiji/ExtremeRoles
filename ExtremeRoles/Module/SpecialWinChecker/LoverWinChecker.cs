using System;
using System.Collections.Generic;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Solo.Neutral;

namespace ExtremeRoles.Module.SpecialWinChecker
{
    internal class LoverWinChecker : IWinChecker
    {
        public RoleGameOverReason Reason => RoleGameOverReason.ShipFallInLove;

        private int loverNum;
        private int nonTasker;
        private HashSet<ExtremeRoleType> roles = new HashSet<ExtremeRoleType>();
        private List<byte> aliveLover = new List<byte> ();

        public LoverWinChecker()
        {
            this.roles.Clear();
            this.aliveLover.Clear();
            this.loverNum = 0;
            this.nonTasker = 0;
        }

        public void AddAliveRole(
            byte playerId,
            SingleRoleBase role)
        {
            if (this.loverNum == 0)
            {
                this.loverNum = OptionHolder.AllOption[((MultiAssignRoleBase)role).GetManagerOptionId(
                    CombinationRoleCommonOption.AssignsNum)].GetValue();
            }
            if (!role.HasTask)
            {
                ++this.nonTasker;
            }
            this.roles.Add(role.Team);
            this.aliveLover.Add(playerId);
        }

        public bool IsWin(GameDataContainer.PlayerStatistics statistics)
        {
            int aliveNum = this.aliveLover.Count;

            if (aliveNum == 0) { return false; }
            if (roles.Count == 1)
            {
                if (roles.Contains(ExtremeRoleType.Crewmate) || 
                    roles.Contains(ExtremeRoleType.Impostor))
                {
                    return false;
                }
            }
            if (aliveNum < this.loverNum) { return false; }
            if (aliveNum == this.nonTasker) { return false; }
            if (aliveNum < statistics.TotalAlive - aliveNum) { return false; }

            foreach (var playerId in this.aliveLover)
            {

                var lover = (Lover)ExtremeRoleManager.GameRole[playerId];

                if (lover.CanHasAnotherRole)
                {

                    switch(lover.AnotherRole.Id)
                    {
                        case ExtremeRoleId.Sidekick:
                            var sidekick = (Sidekick)lover.AnotherRole;
                            var jackalPlayer = Helper.Player.GetPlayerControlById(sidekick.JackalPlayerId).Data;
                            if (!jackalPlayer.IsDead && 
                                !jackalPlayer.Disconnected && 
                                statistics.TeamImpostorAlive <= 0 &&
                                statistics.SeparatedNeutralAlive.Count == 2) // ジャッカルとサイドキックされたニュートラルラバーのみ
                            {
                                return true;
                            }
                            break;
                        case ExtremeRoleId.Jackal:
                            if (statistics.TeamImpostorAlive <= 0 && statistics.SeparatedNeutralAlive.Count == 1)
                            {
                                return true;
                            }
                            break;
                        default:
                            break;
                    }
                }
                if (lover.IsImposter())
                {
                    if (statistics.SeparatedNeutralAlive.Count == 0) // キル能力を持つ別陣営はインポスターのみ
                    {
                        return true;
                    }
                }
            }


            int allTask = 0;
            int allCompTask = 0;

            foreach (var playerId in this.aliveLover)
            {
                var (compTask, totalTask) = Helper.GameSystem.GetTaskInfo(
                    Helper.Player.GetPlayerControlById(playerId).Data);
                allCompTask = allCompTask + compTask;
                allTask = allTask + totalTask;
            }
            if (allCompTask >= allTask) { return true; }


            return false;
        }
    }
}
