using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;


namespace ExtremeRoles.Module.SpecialWinChecker
{
    internal class VigilanteWinChecker : IWinChecker
    {
        public RoleGameOverReason Reason => RoleGameOverReason.VigilanteNewIdealWorld;

        public VigilanteWinChecker()
        { }

        public void AddAliveRole(
            byte playerId, SingleRoleBase role)
        { }

        public bool IsWin(
            GameDataContainer.PlayerStatistics statistics)
        {
            int heroNum = 0;
            int villanNum = 0;
            int vigilanteNum = 0;


            foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
            {
                var playerInfo = GameData.Instance.GetPlayerById(playerId);
                if (!playerInfo.IsDead)
                {
                    if (role.Id != ExtremeRoleId.Hero)
                    {
                        ++heroNum;
                    }
                    else if (role.Id != ExtremeRoleId.Villain)
                    {
                        ++villanNum;
                    }
                    else if (role.Id != ExtremeRoleId.Vigilante)
                    {
                        ++vigilanteNum;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (heroNum > 0 && villanNum > 0 && vigilanteNum > 0)
            {
                return true;
            }
            return false;
        }
    }
}
