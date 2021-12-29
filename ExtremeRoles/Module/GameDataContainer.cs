using System.Collections.Generic;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module
{
    public class GameDataContainer
    {
        public enum PlayerStatus
        {
            Alive = 0,
            Exiled,
            Dead, 
            Disconnected, 
        }

        public static GameOverReason EndReason;
        public static List<GamePlayerInfo> EndGamePlayerInfo = new List<GamePlayerInfo>();
        public static Dictionary<byte, PoolablePlayer> PlayerIcon = new Dictionary<byte, PoolablePlayer>();
        public static List<byte> DeadedAssassin = new List<byte>();
        public static int MeetingsCount = 0;
        public static int WinGameControlId = int.MaxValue;

        public static void GameInit()
        {
            PlayerIcon.Clear();
            DeadedAssassin.Clear();
            MeetingsCount = 0;
            WinGameControlId = int.MaxValue;
        }

        public static void CreatIcons(IntroCutscene __instance)
        {

            EndGamePlayerInfo.Clear();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                PoolablePlayer poolPlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(
                    __instance.PlayerPrefab,
                    HudManager.Instance.transform);

                poolPlayer.UpdateFromPlayerData(player.Data, PlayerOutfitType.Default);
                poolPlayer.SetFlipX(true);
                poolPlayer.gameObject.SetActive(false);
                PlayerIcon.Add(player.PlayerId, poolPlayer);
            }
        }

        public static void EndGameAddStatus(
            GameData.PlayerInfo playerInfo,
            PlayerStatus finalStatus,
            SingleRoleBase role,
            int totalTask,
            int completedTask)
        {
            EndGamePlayerInfo.Add(
                new GamePlayerInfo()
                {
                    PlayerName = playerInfo.PlayerName,
                    Roles = role,
                    CompletedTasks = completedTask,
                    TotalTasks = totalTask,
                    StatusInfo = finalStatus,
                });
        }

        public class PlayerStatistics
        {
            public int AllTeamCrewmate { get; set; }
            public int TeamImpostorAlive { get; set; }
            public int TeamCrewmateAlive { get; set; }
            public int TeamNeutralAlive { get; set; }
            public int TotalAlive { get; set; }

            public Dictionary<(NeutralSeparateTeam, int), int> SeparatedNeutralAlive;
            public bool IsAssassinationMarin { get; set; }

            public PlayerStatistics()
            {
                makePlayerStatic();
            }

            private void makePlayerStatic()
            {
                int numTotalAlive = 0;

                int numCrew = 0;
                int numCrewAlive = 0;

                int numImpostorAlive = 0;

                int numNeutralAlive = 0;
                Dictionary<(NeutralSeparateTeam, int), int> neutralTeam = new Dictionary<
                    (NeutralSeparateTeam, int), int>();

                bool isAssassinationMarin = false;

                foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
                {
                    if (playerInfo.Disconnected) { continue; }
                    SingleRoleBase role = ExtremeRoleManager.GameRole[playerInfo.PlayerId];
                    ExtremeRoleType team = role.Team;

                    // クルーのカウントを数える
                    if (team == ExtremeRoleType.Crewmate) { ++numCrew; }

                    // 死んでたら次のプレイヤーへ
                    if (playerInfo.IsDead) { continue; };

                    int gameControlId = role.GameControlId;
                    if (OptionsHolder.Ship.IsSameNeutralSameWin)
                    {
                        gameControlId = int.MaxValue;
                    }

                    ++numTotalAlive;

                    // 生きてる
                    switch(team)
                    {
                        case ExtremeRoleType.Crewmate:
                            ++numCrewAlive;
                            break;
                        case ExtremeRoleType.Impostor:
                            ++numImpostorAlive;
                            break;
                        case ExtremeRoleType.Neutral:
                            
                            ++numNeutralAlive;

                            switch (role.Id)
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
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }

                    isAssassinationMarin = Patches.AssassinMeeting.AssassinateMarin;

                }

                TotalAlive = numTotalAlive;

                AllTeamCrewmate = numCrew;

                TeamImpostorAlive = numImpostorAlive;
                TeamCrewmateAlive = numCrewAlive;
                TeamNeutralAlive = numNeutralAlive;

                SeparatedNeutralAlive = neutralTeam;

                IsAssassinationMarin = isAssassinationMarin;

            }
        }
       
        private static void addNeutralTeams(
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
        public class GamePlayerInfo
        {
            public string PlayerName { get; set; }
            public SingleRoleBase Roles { get; set; }
            public int CompletedTasks { get; set; }
            public int TotalTasks { get; set; }
            public PlayerStatus StatusInfo { get; set; }
        }
    }
}
