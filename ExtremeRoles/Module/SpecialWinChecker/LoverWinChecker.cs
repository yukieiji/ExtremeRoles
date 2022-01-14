using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using System;
using System.Collections.Generic;

namespace ExtremeRoles.Module.SpecialWinChecker
{
    internal class LoverWinChecker : IWinChecker
    {
        public RoleGameOverReason Reason => RoleGameOverReason.ShipFallInLove;

        private int loverNum;
        private HashSet<ExtremeRoleType> roles = new HashSet<ExtremeRoleType>();
        private List<byte> aliveLover = new List<byte> ();

        public LoverWinChecker()
        {
            this.roles.Clear();
            this.aliveLover.Clear();
            this.loverNum = 0;
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
            this.roles.Add(role.Team);
            this.aliveLover.Add(playerId);
        }

        public bool IsWin(GameDataContainer.PlayerStatistics statistics)
        {
            int aliveNum = this.aliveLover.Count;

            if (aliveNum == 0) { return false; }
            if (roles.Count == 1) { return false; }
            // Helper.Logging.Debug("Ckpt:1");
            if (aliveNum < this.loverNum) { return false; }
            // Helper.Logging.Debug("Ckpt:2");
            if (aliveNum < statistics.TotalAlive - aliveNum) { return false; }
            // Helper.Logging.Debug("Ckpt:3");

            int allTask = 0;
            int allCompTask = 0;

            foreach (var playerId in this.aliveLover)
            {
                var (compTask, totalTask) = Helper.Task.GetTaskInfo(
                    Helper.Player.GetPlayerControlById(playerId).Data);
                allCompTask = allCompTask + compTask;
                allTask = allTask + totalTask;
            }
            // Helper.Logging.Debug("Ckpt:4");
            if (allCompTask >= allTask) { return true; }
            // Helper.Logging.Debug("Ckpt:5");

            return false;
        }
    }
}
