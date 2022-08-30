using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        public enum PlayerStatus
        {
            Alive = 0,
            Exiled,
            Dead,
            Killed,

            Suicide,
            MissShot,
            Retaliate,
            Departure,
            Martyrdom,

            Explosion,

            Assassinate,
            DeadAssassinate,
            Surrender,
            Zombied,

            Disconnected,
        }

        public bool IsRoleSetUpEnd => isRoleSetUpEnd;

        public List<PlayerSummary> FinalSummary = new List<PlayerSummary>();
        public Dictionary<byte, DeadInfo> DeadPlayerInfo = new Dictionary<byte, DeadInfo>();
        public Dictionary<int, Version> PlayerVersion = new Dictionary<int, Version>();

        public HashSet<byte> DeadedAssassin = new HashSet<byte>();

        public ShieldPlayerContainer ShildPlayer = new ShieldPlayerContainer();
        public PlayerHistory History = new PlayerHistory();
        public BakaryUnion Union = new BakaryUnion();

        public GhostRoleAbilityManager AbilityManager = new GhostRoleAbilityManager();

        public int MeetingsCount = 0;

        public bool IsAssassinAssign = false;
        public bool AssassinMeetingTrigger = false;
        public bool AssassinateMarin = false;
        public byte ExiledAssassinId = byte.MaxValue;
        public byte IsMarinPlayerId = byte.MaxValue;

        private bool isRoleSetUpEnd;

        private GameObject status;

        public ExtremeShipStatus()
        {
            Initialize(false);
        }

        public void Initialize(
            bool includeGameObject = true)
        {
            DeadedAssassin.Clear();
            ShildPlayer.Clear();

            FinalSummary.Clear();
            DeadPlayerInfo.Clear();

            Union.Clear();

            History.Clear();

            AbilityManager.Clear();

            MeetingsCount = 0;

            AssassinMeetingTrigger = false;
            AssassinateMarin = false;
            IsAssassinAssign = false;


            ExiledAssassinId = byte.MaxValue;
            IsMarinPlayerId = byte.MaxValue;

            isRoleSetUpEnd = false;

            // 以下リファクタ済み
            this.resetVent();
            this.resetWins();
            this.ResetVison();

            this.ClearMeetingResetObject();

            if (!includeGameObject) { return; }

            if (this.status != null)
            {
                UnityEngine.Object.Destroy(this.status);
                this.status = null;
            }
            this.status = new GameObject("ExtremeShipStatus");

            this.resetUpdateObject();
        }

        public void AddDeadInfo(
            PlayerControl deadPlayer,
            DeathReason reason,
            PlayerControl killer)
        {

            if (DeadPlayerInfo.ContainsKey(
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
            var (completedTask, totalTask) = Helper.GameSystem.GetTaskInfo(playerInfo);
            // IsImpostor
            var finalStatus = PlayerStatus.Alive;
            if (EndReason == GameOverReason.ImpostorBySabotage &&
                !role.IsImpostor())
            {
                finalStatus = PlayerStatus.Dead;
            }
            else if (EndReason == (GameOverReason)RoleGameOverReason.AssassinationMarin)
            {
                if (playerInfo.PlayerId == IsMarinPlayerId)
                {
                    if (playerInfo.IsDead || playerInfo.Disconnected)
                    {
                        finalStatus = PlayerStatus.DeadAssassinate;
                    }
                    else
                    {
                        finalStatus = PlayerStatus.Assassinate;
                    }
                }
                else if (playerInfo.PlayerId == ExiledAssassinId)
                {
                    if (DeadPlayerInfo.TryGetValue(
                        playerInfo.PlayerId, out DeadInfo info))
                    {
                        finalStatus = info.Reason;
                    }
                }
                else if (!role.IsImpostor())
                {
                    finalStatus = PlayerStatus.Surrender;
                }
            }
            else if (EndReason == (GameOverReason)RoleGameOverReason.UmbrerBiohazard)
            {
                if (role.Id != ExtremeRoleId.Umbrer &&
                    !playerInfo.IsDead &&
                    !playerInfo.Disconnected)
                {
                    finalStatus = PlayerStatus.Zombied;
                }
                else
                {
                    if (DeadPlayerInfo.TryGetValue(
                        playerInfo.PlayerId, out DeadInfo info))
                    {
                        finalStatus = info.Reason;
                    }
                }
            }
            else if (playerInfo.Disconnected)
            {
                finalStatus = PlayerStatus.Disconnected;
            }
            else
            {
                if (DeadPlayerInfo.TryGetValue(
                        playerInfo.PlayerId, out DeadInfo info))
                {
                    finalStatus = info.Reason;
                }
            }

            FinalSummary.Add(
                new PlayerSummary
                {
                    PlayerName = playerInfo.PlayerName,
                    Role = role,
                    StatusInfo = finalStatus,
                    TotalTask = totalTask,
                    CompletedTask = EndReason == GameOverReason.HumansByTask ? totalTask : completedTask,
                });

        }
        public void RoleSetUpEnd()
        {
            isRoleSetUpEnd = true;
        }

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

                // クルーのカウントを数える
                if (team == ExtremeRoleType.Crewmate) { ++numCrew; }

                // 死んでたら次のプレイヤーへ
                if (playerInfo.IsDead) { continue; };

                ++numTotalAlive;

                int gameControlId = role.GameControlId;

                if (role.Id == ExtremeRoleId.Assassin &&
                    role.IsImpostor())
                {
                    var assassin = role as Roles.Combination.Assassin;
                    if (assassin != null)
                    {
                        if (!assassin.CanKilled && !assassin.CanKilledFromNeutral)
                        {
                            ++numAssassinAlive;
                        }
                    }
                }

                if (ExtremeRoleManager.SpecialWinCheckRole.Contains(role.Id))
                {
                    addSpecialWinCheckRole(
                        ref specialWinCheckRoleAlive,
                        gameControlId,
                        role.Id, role,
                        playerInfo.PlayerId);
                }

                if (OptionHolder.Ship.IsSameNeutralSameWin && role.IsNeutral())
                {
                    gameControlId = int.MaxValue;
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
                                if (((Roles.Combination.Vigilante)role).Condition ==
                                    Roles.Combination.Vigilante.VigilanteCondition.NewEnemyNeutralForTheShip)
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

        public void ReplaceDeadReason(
            byte playerId, PlayerStatus newReason)
        {
            if (!DeadPlayerInfo.ContainsKey(playerId)) { return; }
            DeadPlayerInfo[playerId].Reason = newReason;
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
                    default:
                        break;
                }
                if (addData != null)
                {
                    roleData.Add(gameControlId, addData);
                }
            }
        }

        public sealed class DeadInfo
        {
            public PlayerStatus Reason { get; set; }

            public DateTime DeadTime { get; set; }

            public PlayerControl Killer { get; set; }
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

        }
        public sealed class PlayerSummary
        {
            public string PlayerName { get; set; }
            public SingleRoleBase Role { get; set; }
            public int CompletedTask { get; set; }
            public int TotalTask { get; set; }
            public PlayerStatus StatusInfo { get; set; }
        }

        public sealed class ShieldPlayerContainer
        {

            private List<(byte, byte)> shield = new List<(byte, byte)>();

            public ShieldPlayerContainer()
            {
                Clear();
            }

            public void Clear()
            {
                shield.Clear();
            }

            public void Add(byte rolePlayerId, byte targetPlayerId)
            {
                shield.Add((rolePlayerId, targetPlayerId));
            }

            public void Remove(byte removeRolePlayerId)
            {
                List<(byte, byte)> remove = new List<(byte, byte)>();

                foreach (var (rolePlayerId, targetPlayerId) in shield)
                {
                    if (rolePlayerId != removeRolePlayerId) { continue; }
                    remove.Add((rolePlayerId, targetPlayerId));
                }

                foreach (var val in remove)
                {
                    shield.Remove(val);
                }

            }
            public byte GetBodyGuardPlayerId(byte targetPlayerId)
            {
                if (shield.Count == 0) { return byte.MaxValue; }

                foreach (var (rolePlayerId, shieldPlayerId) in shield)
                {
                    if (shieldPlayerId == targetPlayerId) { return rolePlayerId; }
                }
                return byte.MaxValue;
            }
            public bool IsShielding(byte rolePlayerId, byte targetPlayerId)
            {
                if (shield.Count == 0) { return false; }
                return shield.Contains((rolePlayerId, targetPlayerId));
            }
        }

        public sealed class PlayerHistory
        {
            public bool BlockAddHistory;

            // 座標、動けるか、ベント内か, 何か使ってるか
            public Queue<(Vector3, bool, bool, bool)> history = new Queue<
                (Vector3, bool, bool, bool)>();
            private bool init = false;
            private int size = 0;

            public PlayerHistory()
            {
                Clear();
            }

            public void Enqueue(PlayerControl player)
            {
                if (!init || BlockAddHistory) { return; }

                int overflow = history.Count - size;
                for (int i = 0; i < overflow; ++i)
                {
                    history.Dequeue();
                }

                history.Enqueue(
                    (
                        player.transform.position,
                        player.CanMove,
                        player.inVent,
                        !player.Collider.enabled && !player.NetTransform.enabled && !player.moveable
                    )
                );
            }

            public void Clear()
            {
                BlockAddHistory = false;
                DataClear();
                init = false;
                size = 0;
            }

            public void DataClear()
            {
                history.Clear();
            }

            public void Initialize(float historySecond)
            {
                size = (int)Mathf.Round(historySecond / Time.fixedDeltaTime);
                init = true;
            }

            public IEnumerator<
                (Vector3, bool, bool, bool)> GetAllHistory() => history.Reverse().GetEnumerator();

            public int GetSize() => size;
        }

        public sealed class BakaryUnion
        {
            private bool isChangeCooking = false;

            private float timer = 0.0f;
            private float goodTime = 0.0f;
            private float badTime = 0.0f;
            private bool isUnion = false;
            private HashSet<byte> aliveBakary = new HashSet<byte>();

            public BakaryUnion()
            {
                Clear();
            }

            public bool IsEstablish()
            {
                updateBakaryAlive();
                return aliveBakary.Count != 0;
            }

            public string GetBreadBakingCondition()
            {
                if (!isChangeCooking)
                {
                    return Helper.Translation.GetString("goodBread");
                }

                if (timer < goodTime)
                {
                    return Helper.Translation.GetString("rawBread");
                }
                else if (goodTime <= timer && timer < badTime)
                {
                    return Helper.Translation.GetString("goodBread");
                }
                else
                {
                    return Helper.Translation.GetString("badBread");
                }
            }

            public void Clear()
            {
                ResetTimer();
                isUnion = false;
                isChangeCooking = false;
                aliveBakary.Clear();
            }

            public void ResetTimer()
            {
                timer = 0;
            }

            public void SetCookingCondition(
                float goodCookTime,
                float badCookTime,
                bool isChangeCooking)
            {
                goodTime = goodCookTime;
                badTime = badCookTime;
                this.isChangeCooking = isChangeCooking;
            }

            public void Update()
            {
                if (!isUnion) { organize(); }
                if (aliveBakary.Count == 0) { return; }
                if (MeetingHud.Instance != null) { return; }

                timer += Time.fixedDeltaTime;

            }

            private void organize()
            {
                isUnion = true;
                foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
                {
                    if (role.Id == ExtremeRoleId.Bakary)
                    {
                        aliveBakary.Add(playerId);
                    }

                    var multiAssignRole = role as MultiAssignRoleBase;
                    if (multiAssignRole != null)
                    {
                        if (multiAssignRole.AnotherRole != null)
                        {
                            if (multiAssignRole.AnotherRole.Id == ExtremeRoleId.Bakary)
                            {
                                aliveBakary.Add(playerId);
                            }
                        }
                    }

                }
            }

            private void updateBakaryAlive()
            {
                if (aliveBakary.Count == 0) { return; }

                HashSet<byte> updatedBakary = new HashSet<byte>();

                foreach (var playerId in aliveBakary)
                {
                    PlayerControl player = Helper.Player.GetPlayerControlById(playerId);
                    if (!player.Data.IsDead && !player.Data.Disconnected)
                    {
                        updatedBakary.Add(playerId);
                    }
                }

                aliveBakary = updatedBakary;
            }
        }
    }

}
