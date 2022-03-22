using System.Collections.Generic;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Neutral;


namespace ExtremeRoles.Module.SpecialWinChecker
{
    internal class YandereWinChecker : IWinChecker
    {
        public RoleGameOverReason Reason => RoleGameOverReason.YandereShipJustForTwo;

        private List<Yandere> aliveYandere = new List<Yandere>();

        public YandereWinChecker()
        {
            aliveYandere.Clear();
        }

        public void AddAliveRole(
            byte playerId, SingleRoleBase role)
        {
            aliveYandere.Add((Yandere)role);
        }

        public bool IsWin(
            GameDataContainer.PlayerStatistics statistics)
        {
            List<PlayerControl> aliveOneSideLover = new List<PlayerControl>();

            foreach (Yandere role in aliveYandere)
            {
                var playerInfo = role.OneSidedLover.Data;

                if (!playerInfo.IsDead && !playerInfo.Disconnected)
                {
                    aliveOneSideLover.Add(role.OneSidedLover);
                }
            }

            int aliveNum = aliveYandere.Count + aliveOneSideLover.Count;

            if (aliveOneSideLover.Count != 0) { return false; }
            if (aliveNum < statistics.TotalAlive - aliveNum) { return false; }

            foreach (var player in aliveOneSideLover)
            {
                ExtremeRolesPlugin.GameDataStore.PlusWinner.Add(player);
            }

            return true;
        }
    }
}
