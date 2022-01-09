using System;
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
            Killed, 
            Suicide,
            MissShot,
            Disconnected,
        }

        public GameOverReason EndReason;
        public List<PlayerSummary> FinalSummary = new List<PlayerSummary>();
        public Dictionary<byte, DeadInfo> DeadPlayerInfo = new Dictionary<byte, DeadInfo>();

        public PoolablePlayer PlayerPrefab;

        public List<byte> DeadedAssassin = new List<byte>();

        public int MeetingsCount = 0;
        public int WinGameControlId = int.MaxValue;

        public bool AssassinMeetingTrigger = false;
        public bool AssassinateMarin = false;
        public bool WinCheckDisable = false;
        public byte ExiledAssassinId = byte.MaxValue;
        public byte IsMarinPlayerId = byte.MaxValue;

        private bool isRoleSetUpEnd = false;

        public GameDataContainer()
        {
            this.Initialize();
        }

        public void Initialize()
        {
            DeadedAssassin.Clear();
            FinalSummary.Clear();
            DeadPlayerInfo.Clear();

            MeetingsCount = 0;
            WinGameControlId = int.MaxValue;

            isRoleSetUpEnd = false;
            AssassinMeetingTrigger = false;
            AssassinateMarin = false;
            WinCheckDisable = false;

            ExiledAssassinId = byte.MaxValue;
            IsMarinPlayerId = byte.MaxValue;
        }

        public void AddDeadInfo(
            PlayerControl deadPlayer,
            DeathReason reason,
            PlayerControl killer)
        {

            if (this.DeadPlayerInfo.ContainsKey(
                deadPlayer.PlayerId)) { return; }
 
            var newReson = PlayerStatus.Dead;

            switch (reason)
            {
                case DeathReason.Exile:
                    newReson = PlayerStatus.Exiled;
                    break;
                case DeathReason.Disconnect:
                    newReson = PlayerStatus.Disconnected;
                    break;
                case DeathReason.Kill:
                    newReson = PlayerStatus.Killed;
                    if (killer.PlayerId == deadPlayer.PlayerId)
                    {
                        newReson = PlayerStatus.Suicide;
                    }
                    break;
                default:
                    break;

            }

            DeadPlayerInfo.Add(
                deadPlayer.PlayerId,
                new DeadInfo
                {
                    DeadTime = DateTime.UtcNow,
                    Reason = newReson,
                    Killer = killer
                });
        }

        public void AddPlayerSummary(
            GameData.PlayerInfo playerInfo)
        {

            var role = ExtremeRoleManager.GameRole[playerInfo.PlayerId];
            var (completedTask, totalTask) = Helper.Task.GetTaskInfo(playerInfo);

            var finalStatus = PlayerStatus.Alive;
            if (
                (this.EndReason == GameOverReason.ImpostorBySabotage) &&
                (!playerInfo.Role.IsImpostor))
            {
                finalStatus = PlayerStatus.Dead;
            }
            else if (playerInfo.Disconnected)
            {
                finalStatus = PlayerStatus.Disconnected;
            }
            else
            {
                if (this.DeadPlayerInfo.ContainsKey(playerInfo.PlayerId))
                {
                    var info = this.DeadPlayerInfo[playerInfo.PlayerId];
                    finalStatus = info.Reason;
                }
            }

            this.FinalSummary.Add(
                new PlayerSummary
                {
                    PlayerName = playerInfo.PlayerName,
                    Role = role,
                    StatusInfo = finalStatus,
                    TotalTask = totalTask,
                    CompletedTask = EndReason == GameOverReason.HumansByTask ? totalTask : completedTask,
                });

        }

        public void RoleSetUpEnded()
        {
            this.isRoleSetUpEnd = true;
        }
        public bool IsRoleSetUpEnd()
        {
            return this.isRoleSetUpEnd;
        }

        public void SetPoolPlayerPrefab(IntroCutscene __instance)
        {
            PlayerPrefab = UnityEngine.Object.Instantiate(
                __instance.PlayerPrefab,
                HudManager.Instance.transform);
            UnityEngine.Object.DontDestroyOnLoad(PlayerPrefab);
            PlayerPrefab.name = "poolablePlayerPrefab";
            PlayerPrefab.gameObject.SetActive(false);
        }

        public PlayerStatistics CreateStatistics()
        {
            int numTotalAlive = 0;

            int numCrew = 0;
            int numCrewAlive = 0;

            int numImpostorAlive = 0;

            int numNeutralAlive = 0;
            Dictionary<(NeutralSeparateTeam, int), int> neutralTeam = new Dictionary<
                (NeutralSeparateTeam, int), int>();

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
                if (OptionHolder.Ship.IsSameNeutralSameWin)
                {
                    gameControlId = int.MaxValue;
                }

                ++numTotalAlive;

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
                            case ExtremeRoleId.Lover:
                                addNeutralTeams(
                                    ref neutralTeam,
                                    gameControlId,
                                    NeutralSeparateTeam.Lover);
                                break;
                            default:
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

                SeparatedNeutralAlive = neutralTeam,

            };
        }

        public void ReplaceDeadReason(
            byte playerId, PlayerStatus newReason)
        {
            if (!this.DeadPlayerInfo.ContainsKey(playerId)) { return; }
            this.DeadPlayerInfo[playerId].Reason = newReason;
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

        public class DeadInfo
        {
            public PlayerStatus Reason { get; set; }

            public DateTime DeadTime { get; set; }

            public PlayerControl Killer { get; set; }
        }

        public class PlayerStatistics
        {
            public int AllTeamCrewmate { get; set; }
            public int TeamImpostorAlive { get; set; }
            public int TeamCrewmateAlive { get; set; }
            public int TeamNeutralAlive { get; set; }
            public int TotalAlive { get; set; }

            public Dictionary<(NeutralSeparateTeam, int), int> SeparatedNeutralAlive { get; set; }

        }
        public class PlayerSummary
        {
            public string PlayerName { get; set; }
            public SingleRoleBase Role { get; set; }
            public int CompletedTask { get; set; }
            public int TotalTask { get; set; }
            public PlayerStatus StatusInfo { get; set; }
        }
    }
}
