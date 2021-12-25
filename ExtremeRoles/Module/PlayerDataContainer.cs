using System.Collections.Generic;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module
{
    public class PlayerDataContainer
    {
        public enum PlayerStatus
        {
            Alive = 0,
            Exiled,
            Dead, 
            Disconnected, 
        }

        public static List<GamePlayerInfo> EndGamePlayerInfo = new List<GamePlayerInfo>();
        public static Dictionary<byte, PoolablePlayer> PlayerIcon = new Dictionary<byte, PoolablePlayer>();
        public static List<byte> DeadedAssassin = new List<byte>();

        public static void GameInit()
        {
            PlayerIcon.Clear();
            DeadedAssassin.Clear();
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
            public int TotalAlive { get; set; }

            public List<byte> NeutralKillerPlayer { get; set; }
            public List<byte> NeutralAlivePlayer { get; set; }
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

                int numImpostorsAlive = 0;

                List<byte> neutralKillerPlayer = new List<byte>();
                List<byte> neutralAlivePlayer = new List<byte> ();

                bool isAssassinationMarin = false;

                foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
                {
                    if (playerInfo.Disconnected) { continue; }
                    SingleRoleBase role = ExtremeRoleManager.GameRole[playerInfo.PlayerId];
                    ExtremeRoleType team = role.Teams;

                    // クルーのカウントを数える
                    if (team == ExtremeRoleType.Crewmate) { ++numCrew; }

                    // 死んでたら次のプレイヤーへ
                    if (playerInfo.IsDead) { continue; };

                    ++numTotalAlive;

                    // 生きてる
                    switch(team)
                    {
                        case ExtremeRoleType.Crewmate:
                            ++numCrewAlive;
                            break;
                        case ExtremeRoleType.Impostor:
                            ++numImpostorsAlive;
                            break;
                        case ExtremeRoleType.Neutral:
                            if (role.CanKill)
                            {
                                neutralKillerPlayer.Add(playerInfo.PlayerId);
                            }
                            neutralAlivePlayer.Add(playerInfo.PlayerId);
                            break;
                        default:
                            break;
                    }

                    isAssassinationMarin = Patches.AssassinMeeting.AssassinateMarin;

                }

                TotalAlive = numTotalAlive;

                AllTeamCrewmate = numCrew;

                TeamImpostorAlive = numImpostorsAlive;
                TeamCrewmateAlive = numImpostorsAlive;

                NeutralKillerPlayer = neutralKillerPlayer;
                NeutralAlivePlayer = neutralAlivePlayer;

                IsAssassinationMarin = isAssassinationMarin;

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
