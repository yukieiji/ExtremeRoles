using System.Collections.Generic;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        public PlayerStatistics CreateStatistics()
        {
            int numTotalAlive = 0;

            int numCrew = 0;
            int numCrewAlive = 0;

            int numImpostorAlive = 0;

            int numNeutralAlive = 0;

            int numAssassinAlive = 0;
            Dictionary<(NeutralSeparateTeam, int), int> neutralTeam = new Dictionary<
                (NeutralSeparateTeam, int), int>();
            Dictionary<int, IWinChecker> specialWinCheckRoleAlive = new Dictionary<
                int, IWinChecker>();

            foreach (GameData.PlayerInfo playerInfo in
                GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (playerInfo.Disconnected) { continue; }
                SingleRoleBase role = ExtremeRoleManager.GameRole[playerInfo.PlayerId];
                ExtremeRoleType team = role.Team;
                ExtremeRoleId roleId = role.Id;

                // クルーのカウントを数える
                if (team == ExtremeRoleType.Crewmate) { ++numCrew; }

                // 死んでたら次のプレイヤーへ
                if (playerInfo.IsDead) { continue; };

                // マッドメイトの生存をカウントしないオプション
                if (roleId == ExtremeRoleId.Madmate)
                {
                    Roles.Solo.Neutral.Madmate madmate = role as Roles.Solo.Neutral.Madmate;
                    if (madmate != null)
                    {
                        if (madmate.IsDontCountAliveCrew)
                        {
                            continue;
                        }
                    }
                }

                ++numTotalAlive;

                int gameControlId = role.GameControlId;

                if (roleId == ExtremeRoleId.Assassin &&
                    role.IsImpostor())
                {
                    Assassin assassin = role as Assassin;
                    if (assassin != null)
                    {
                        if (!assassin.CanKilled && !assassin.CanKilledFromNeutral)
                        {
                            ++numAssassinAlive;
                        }
                    }
                }

                if (ExtremeRoleManager.SpecialWinCheckRole.Contains(roleId))
                {
                    addSpecialWinCheckRole(
                        ref specialWinCheckRoleAlive,
                        gameControlId,
                        roleId, role,
                        playerInfo.PlayerId);
                }

                if (ExtremeGameManager.Instance.ShipOption.IsSameNeutralSameWin && role.IsNeutral())
                {
                    gameControlId = ExtremeGameManager.SameNeutralGameControlId;
                }

                // 生きてる
                switch (team)
                {
                    case ExtremeRoleType.Crewmate:
                        ++numCrewAlive;
                        break;
                    case ExtremeRoleType.Impostor:
                        ++numImpostorAlive;
                        break;
                    case ExtremeRoleType.Neutral:

                        ++numNeutralAlive;

                        switch (roleId)
                        {
                            case ExtremeRoleId.Alice:
                                addNeutralTeams(
                                    ref neutralTeam,
                                    gameControlId,
                                    NeutralSeparateTeam.Alice);
                                break;
                            case ExtremeRoleId.Jackal:
                            case ExtremeRoleId.Sidekick:
                                addNeutralTeams(
                                    ref neutralTeam,
                                    gameControlId,
                                    NeutralSeparateTeam.Jackal);
                                break;
                            case ExtremeRoleId.Lover:
                                addNeutralTeams(
                                    ref neutralTeam,
                                    gameControlId,
                                    NeutralSeparateTeam.Lover);
                                break;
                            case ExtremeRoleId.Missionary:
                                addNeutralTeams(
                                    ref neutralTeam,
                                    gameControlId,
                                    NeutralSeparateTeam.Missionary);
                                break;
                            case ExtremeRoleId.Yandere:
                                addNeutralTeams(
                                    ref neutralTeam,
                                    gameControlId,
                                    NeutralSeparateTeam.Yandere);
                                break;
                            case ExtremeRoleId.Vigilante:
                                if (((Vigilante)role).Condition ==
                                    Vigilante.VigilanteCondition.NewEnemyNeutralForTheShip)
                                {
                                    addNeutralTeams(
                                        ref neutralTeam,
                                        gameControlId,
                                        NeutralSeparateTeam.Vigilante);
                                }
                                break;
                            case ExtremeRoleId.Miner:
                                addNeutralTeams(
                                    ref neutralTeam,
                                    gameControlId,
                                    NeutralSeparateTeam.Miner);
                                break;
                            case ExtremeRoleId.Eater:
                                addNeutralTeams(
                                    ref neutralTeam,
                                    gameControlId,
                                    NeutralSeparateTeam.Eater);
                                break;
                            case ExtremeRoleId.Traitor:
                                addNeutralTeams(
                                    ref neutralTeam,
                                    gameControlId,
                                    NeutralSeparateTeam.Traitor);
                                break;
                            case ExtremeRoleId.Queen:
                            case ExtremeRoleId.Servant:
                                addNeutralTeams(
                                    ref neutralTeam,
                                    gameControlId,
                                    NeutralSeparateTeam.Queen);
                                break;
                            case ExtremeRoleId.Delinquent:
                                addNeutralTeams(
                                    ref neutralTeam,
                                    gameControlId,
                                    NeutralSeparateTeam.Kids);
                                break;
                            default:
                                checkMultiAssignedServant(
                                    ref neutralTeam,
                                    gameControlId, role);
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }

            return new PlayerStatistics()
            {
                TotalAlive = numTotalAlive,

                AllTeamCrewmate = numCrew,

                TeamImpostorAlive = numImpostorAlive,
                TeamCrewmateAlive = numCrewAlive,
                TeamNeutralAlive = numNeutralAlive,
                AssassinAlive = numAssassinAlive,

                SpecialWinCheckRoleAlive = specialWinCheckRoleAlive,
                SeparatedNeutralAlive = neutralTeam,
            };
        }

        private void checkMultiAssignedServant(
            ref Dictionary<(NeutralSeparateTeam, int), int> neutralTeam,
            int gameControlId,
            SingleRoleBase role)
        {
            var multiAssignRole = role as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole?.Id == ExtremeRoleId.Servant)
                {
                    addNeutralTeams(
                        ref neutralTeam,
                        gameControlId,
                        NeutralSeparateTeam.Queen);
                }
            }
        }

        private void addNeutralTeams(
            ref Dictionary<(NeutralSeparateTeam, int), int> neutralTeam,
            int gameControlId,
            NeutralSeparateTeam team)
        {
            var key = (team, gameControlId);

            if (neutralTeam.ContainsKey(key))
            {
                neutralTeam[key] = neutralTeam[key] + 1;
            }
            else
            {
                neutralTeam.Add(key, 1);
            }
        }

        private void addSpecialWinCheckRole(
            ref Dictionary<int, IWinChecker> roleData,
            int gameControlId,
            ExtremeRoleId roleId,
            SingleRoleBase role,
            byte playerId)
        {

            if (roleData.ContainsKey(gameControlId))
            {
                roleData[gameControlId].AddAliveRole(
                    playerId, role);
            }
            else
            {
                IWinChecker addData = null;
                switch (roleId)
                {
                    case ExtremeRoleId.Lover:
                        addData = new LoverWinChecker();
                        addData.AddAliveRole(playerId, role);
                        break;
                    case ExtremeRoleId.Yandere:
                        addData = new YandereWinChecker();
                        addData.AddAliveRole(playerId, role);
                        break;
                    case ExtremeRoleId.Vigilante:
                        addData = new VigilanteWinChecker();
                        break;
                    case ExtremeRoleId.Delinquent:
                        addData = new KidsWinChecker();
                        addData.AddAliveRole(playerId, role);
                        break;
                    default:
                        break;
                }
                if (addData != null)
                {
                    roleData.Add(gameControlId, addData);
                }
            }
        }

        public sealed class PlayerStatistics
        {
            public int AllTeamCrewmate { get; set; }
            public int TeamImpostorAlive { get; set; }
            public int TeamCrewmateAlive { get; set; }
            public int TeamNeutralAlive { get; set; }
            public int TotalAlive { get; set; }
            public int AssassinAlive { get; set; }

            public Dictionary<int, IWinChecker> SpecialWinCheckRoleAlive { get; set; }

            public Dictionary<(NeutralSeparateTeam, int), int> SeparatedNeutralAlive { get; set; }

            public override string ToString()
            {
                var builder = new System.Text.StringBuilder();

                builder.AppendLine("------------ Current Player Statistics ------------");
                builder.AppendLine($"Total Player Alive : {this.TotalAlive}");
                builder.AppendLine($"Crewmate Alive : {this.TeamCrewmateAlive} / {this.AllTeamCrewmate}");
                builder.AppendLine($"Impostor Alive :{this.TeamImpostorAlive}");
                builder.AppendLine($"Assassin Alive : {this.AssassinAlive}");
                builder.AppendLine($"Neutral Alive : {this.TeamNeutralAlive}");

                builder.AppendLine("------ Neutral Win Special Checker ------");
                foreach (var (id, winChecker) in this.SpecialWinCheckRoleAlive)
                {
                    builder.AppendLine(
                        $"{winChecker} --- GameControlId:{id} --- IsWin:{winChecker.IsWin(this)}");
                }

                builder.AppendLine(
                    $"------ Neutral Separate Teams --- CurrentAliveTeams:{this.SeparatedNeutralAlive.Count} ------");
                foreach (var ((team, id), aliveNum) in this.SeparatedNeutralAlive)
                {
                    builder.AppendLine(
                        $"{team} --- GameControlId:{id} --- AliveNum: {aliveNum}");
                }
                return builder.ToString();
            }
        }
    }
}
