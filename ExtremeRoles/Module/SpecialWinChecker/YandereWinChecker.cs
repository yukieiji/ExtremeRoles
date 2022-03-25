using System.Collections.Generic;

using Hazel;

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

            int oneSidedLoverImpNum = 0;
            int oneSidedLoverNeutralNum = 0;

            foreach (Yandere role in aliveYandere)
            {
                if (role.OneSidedLover == null) { return false; }

                var playerInfo = role.OneSidedLover.Data;
                var oneSidedLoverRole = ExtremeRoleManager.GameRole[playerInfo.PlayerId];

                if (!playerInfo.IsDead && !playerInfo.Disconnected)
                {
                    aliveOneSideLover.Add(role.OneSidedLover);

                    if (oneSidedLoverRole.IsImpostor())
                    { 
                        ++oneSidedLoverImpNum; 
                    }
                    else if (oneSidedLoverRole.IsNeutral())
                    {
                        switch (oneSidedLoverRole.Id)
                        {
                            case ExtremeRoleId.Alice:
                            case ExtremeRoleId.Jackal:
                            case ExtremeRoleId.Sidekick:
                            case ExtremeRoleId.Lover:
                            case ExtremeRoleId.Missionary:
                                ++oneSidedLoverNeutralNum;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            int aliveNum = aliveYandere.Count + aliveOneSideLover.Count;

            if (aliveOneSideLover.Count == 0 || aliveYandere.Count == 0) { return false; }
            if (aliveNum < statistics.TotalAlive - aliveNum) { return false; }
            if (statistics.TeamImpostorAlive - oneSidedLoverImpNum > 0) { return false; }
            if (statistics.SeparatedNeutralAlive.Count - oneSidedLoverNeutralNum > 1) { return false; }

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RPCOperator.Command.SetWinPlayer,
                Hazel.SendOption.Reliable, -1);
            writer.Write(aliveOneSideLover.Count);

            foreach (var player in aliveOneSideLover)
            {
                writer.Write(player.PlayerId);
                ExtremeRolesPlugin.GameDataStore.PlusWinner.Add(player);
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            return true;
        }
    }
}
