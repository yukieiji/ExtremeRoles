using System.Collections.Generic;


using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Neutral;


namespace ExtremeRoles.Module.SpecialWinChecker;

internal sealed class YandereWinChecker : IWinChecker
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
        PlayerStatistics statistics)
    {
        List<PlayerControl> aliveOneSideLover = new List<PlayerControl>();

        int oneSidedLoverImpNum = 0;
        int oneSidedLoverNeutralNum = 0;

        foreach (Yandere role in aliveYandere)
        {
            if (role.OneSidedLover == null) { continue; }

            var playerInfo = role.OneSidedLover.Data;
            var oneSidedLoverRole = ExtremeRoleManager.GameRole[playerInfo.PlayerId];

            if (playerInfo.IsDead || playerInfo.Disconnected)
            {
                continue;
            }

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
                    case ExtremeRoleId.Miner:
                    case ExtremeRoleId.Eater:
                    case ExtremeRoleId.Traitor:
                    case ExtremeRoleId.Queen:
                    case ExtremeRoleId.Delinquent:
                        ++oneSidedLoverNeutralNum;
                        break;
                    default:
                        break;
                }
            }
        }

        int aliveNum = aliveYandere.Count + aliveOneSideLover.Count;

        if (aliveOneSideLover.Count == 0 || aliveYandere.Count == 0) { return false; }
        if (aliveNum < statistics.TotalAlive - aliveNum) { return false; }
        if (statistics.TeamImpostorAlive - statistics.AssassinAlive - oneSidedLoverImpNum > 0) { return false; }
        if (statistics.SeparatedNeutralAlive.Count - oneSidedLoverNeutralNum > 1) { return false; }

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.SetWinPlayer))
        {
            caller.WriteInt(aliveOneSideLover.Count);
            foreach (var player in aliveOneSideLover)
            {
                caller.WriteByte(player.PlayerId);
                ExtremeRolesPlugin.ShipState.AddWinner(player);
            }
        }

        return true;
    }
}
