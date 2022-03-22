using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module
{
    public class GameDataContainer
    {
        public enum CustomVentType
        {
            MeryVent,
        }

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

            Disconnected,
        }

        public GameOverReason EndReason;
        public List<PlayerSummary> FinalSummary = new List<PlayerSummary>();
        public Dictionary<byte, DeadInfo> DeadPlayerInfo = new Dictionary<byte, DeadInfo>();
        public List<PlayerControl> PlusWinner = new List<PlayerControl>();
        public Dictionary<int, Version> PlayerVersion = new Dictionary<int, Version>();

        public HashSet<byte> DeadedAssassin = new HashSet<byte>();
        public List<IUpdatableObject> UpdateObject = new List<IUpdatableObject>();
        public CustomVentContainer CustomVent = new CustomVentContainer();


        public ShieldPlayerContainer ShildPlayer = new ShieldPlayerContainer();
        public PlayerHistory History = new PlayerHistory();
        public BakaryUnion Union = new BakaryUnion();

        public int MeetingsCount = 0;
        public int WinGameControlId = int.MaxValue;

        public bool AssassinMeetingTrigger = false;
        public bool AssassinateMarin = false;
        public bool WinCheckDisable = false;
        public byte ExiledAssassinId = byte.MaxValue;
        public byte IsMarinPlayerId = byte.MaxValue;

        private List<IMeetingResetObject> resetObject = new List<IMeetingResetObject>();

        public GameDataContainer()
        {
            this.Initialize();
        }

        public void Initialize()
        {
            DeadedAssassin.Clear();
            ShildPlayer.Clear();

            FinalSummary.Clear();
            DeadPlayerInfo.Clear();
            PlusWinner.Clear();
            Union.Clear();
            ClearMeetingResetObject();

            clearUpdateObject();
            CustomVent.Clear();

            MeetingsCount = 0;
            WinGameControlId = int.MaxValue;

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
            var (completedTask, totalTask) = Helper.GameSystem.GetTaskInfo(playerInfo);
            // IsImpostor
            var finalStatus = PlayerStatus.Alive;
            if ((this.EndReason == GameOverReason.ImpostorBySabotage) &&
                (!role.IsImpostor()))
            {
                finalStatus = PlayerStatus.Dead;
            }
            else if ((this.EndReason == (GameOverReason)RoleGameOverReason.AssassinationMarin))
            {
                if (playerInfo.PlayerId == this.IsMarinPlayerId)
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
                else if (playerInfo.PlayerId == this.ExiledAssassinId)
                {
                    if (this.DeadPlayerInfo.ContainsKey(playerInfo.PlayerId))
                    {
                        var info = this.DeadPlayerInfo[playerInfo.PlayerId];
                        finalStatus = info.Reason;
                    }
                }
                else if (!role.IsImpostor())
                {
                    finalStatus = PlayerStatus.Surrender;
                }
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
        public bool IsRoleSetUpEnd()
        {
            if (GameData.Instance == null)
            {
                return false;
            }
            return ExtremeRoleManager.GameRole.Count == GameData.Instance.AllPlayers.Count;
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

            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
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

                if (role.Id == ExtremeRoleId.Assassin)
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
                AssassinAlive = numAssassinAlive,

                SpecialWinCheckRoleAlive = specialWinCheckRoleAlive,
                SeparatedNeutralAlive = neutralTeam,
            };
        }

        public void ReplaceDeadReason(
            byte playerId, PlayerStatus newReason)
        {
            if (!this.DeadPlayerInfo.ContainsKey(playerId)) { return; }
            this.DeadPlayerInfo[playerId].Reason = newReason;
        }

        public void AddMeetingResetObject(
            IMeetingResetObject resetObject)
        {
            this.resetObject.Add(resetObject);
        }
        public void ClearMeetingResetObject()
        {
            foreach (var clerObject in this.resetObject)
            { 
                clerObject.Clear();
            }
            this.resetObject.Clear();
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
                    default:
                        break;
                }
                if (addData != null)
                {
                    roleData.Add(gameControlId, addData);
                }
            }
        }

        private void clearUpdateObject()
        {
            foreach(var updateObject in this.UpdateObject)
            {
                updateObject.Clear();
            }

            this.UpdateObject.Clear();

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
            public int AssassinAlive { get; set; }

            public Dictionary<int, IWinChecker> SpecialWinCheckRoleAlive { get; set; }

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

        public class ShieldPlayerContainer
        {

            private List<(byte, byte)> shield = new List<(byte, byte)>();

            public ShieldPlayerContainer()
            {
                this.Clear();
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

                foreach (var(rolePlayerId, targetPlayerId) in shield)
                {
                    if (rolePlayerId != removeRolePlayerId) { continue; }
                    remove.Add((rolePlayerId, targetPlayerId));
                }

                foreach(var val in remove)
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

        public class PlayerHistory
        {
            public bool BlockAddHistory;

            // 座標、動けるか、ベント内か, 何か使ってるか
            public Queue<
                Tuple<Vector3, bool, bool, bool>> history = new Queue<
                    Tuple<Vector3, bool, bool, bool>>();
            private bool init = false;
            private int size = 0;

            public PlayerHistory()
            {
                this.Clear();
            }

            public void Enqueue(PlayerControl player)
            {
                if (!this.init || this.BlockAddHistory) { return; }

                int overflow = this.history.Count - this.size;
                for (int i = 0; i < overflow; ++i)
                {
                    this.history.Dequeue();
                }

                this.history.Enqueue(
                    Tuple.Create(
                        player.transform.position,
                        player.CanMove,
                        player.inVent,
                        !player.Collider.enabled && !player.NetTransform.enabled && !player.moveable));
            }

            public void Clear()
            {
                this.BlockAddHistory = false;
                DataClear();
                this.init = false;
                this.size = 0;
            }

            public void DataClear()
            {
                this.history.Clear();
            }

            public void Initialize(float historySecond)
            {
                this.size = (int)Mathf.Round(historySecond / Time.fixedDeltaTime);
                this.init = true;
            }

            public IEnumerable<Tuple<
                Vector3, bool, bool, bool>> GetAllHistory() => this.history.Reverse();

            public int GetSize() => this.size;
        }

        public class CustomVentContainer
        {
            private Dictionary<int, CustomVentType> ventType = new Dictionary<int, CustomVentType>();
            private Dictionary<CustomVentType, List<Vent>> addVent = new Dictionary<CustomVentType, List<Vent>>();
            private Dictionary<CustomVentType, Sprite[]> ventAnime = new Dictionary<CustomVentType, Sprite[]>();

            public CustomVentContainer()
            {
                this.Clear();
            }

            public void Clear()
            {
                addVent.Clear();
                ventType.Clear();
                ventAnime.Clear();
            }

            public void AddVent(
                Vent newVent,
                CustomVentType type)
            {
                var allVents = ShipStatus.Instance.AllVents.ToList();
                allVents.Add(newVent);
                ShipStatus.Instance.AllVents = allVents.ToArray();
                if (this.addVent.ContainsKey(type))
                {
                    this.addVent[type].Add(newVent);
                }
                else
                {
                    var ventList = new List<Vent>();
                    ventList.Add(newVent);
                    this.addVent.Add(type, ventList);
                }
                if (!this.ventAnime.ContainsKey(type))
                {
                    ventAnime.Add(type, new Sprite[18]);
                }

                ventType.Add(newVent.Id, type);
            }

            public List<Vent> GetCustomVent(CustomVentType type)
            {
                if (this.addVent.ContainsKey(type))
                {
                    return this.addVent[type];
                }
                return new List<Vent>();
            }

            public Sprite GetVentSprite(int ventId, int index)
            {
                CustomVentType type = ventType[ventId];
                Sprite img = ventAnime[type][index];

                if (img != null)
                {
                    return img;
                }
                else
                {
                    switch (type)
                    {
                        case CustomVentType.MeryVent:
                            img = Resources.Loader.CreateSpriteFromResources(
                                string.Format(Resources.Path.MeryCustomVentAnime, index), 125f);
                            break;
                        default:
                            return null;
                    }

                    ventAnime[type][index] = img;
                    return img;
                }
            }

            public bool IsCustomVent(int ventId) => this.ventType.ContainsKey(ventId);
        }

        public class BakaryUnion
        {
            private bool isChangeCooking = false;

            private float timer = 0.0f;
            private float goodTime = 0.0f;
            private float badTime = 0.0f;
            private bool isUnion = false;
            private HashSet<byte> aliveBakary = new HashSet<byte> ();

            public BakaryUnion()
            {
                this.Clear();
            }

            public bool IsEstablish()
            {
                this.updateBakaryAlive();
                return this.aliveBakary.Count != 0;
            }

            public string GetBreadBakingCondition()
            {
                if (!this.isChangeCooking)
                {
                    return Helper.Translation.GetString("goodBread");
                }

                if (this.timer < this.goodTime)
                {
                    return Helper.Translation.GetString("rawBread");
                }
                else if (this.goodTime <= this.timer && this.timer < this.badTime)
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
                this.ResetTimer();
                this.isUnion = false;
                this.isChangeCooking = false;
                this.aliveBakary.Clear();
            }

            public void ResetTimer()
            {
                this.timer = 0;
            }

            public void SetCookingCondition(
                float goodCookTime,
                float badCookTime,
                bool isChangeCooking)
            {
                this.goodTime = goodCookTime;
                this.badTime = badCookTime;
                this.isChangeCooking = isChangeCooking;
            }

            public void Update()
            {
                if (!this.isUnion) { this.organize(); }
                if (this.aliveBakary.Count == 0) { return; }
                if (MeetingHud.Instance != null) { return; }

                this.timer += Time.fixedDeltaTime;

            }

            private void organize()
            {
                this.isUnion = true;
                foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
                {
                    if (role.Id == ExtremeRoleId.Bakary)
                    { 
                        this.aliveBakary.Add(playerId); 
                    }
                    
                    var multiAssignRole = role as MultiAssignRoleBase;
                    if (multiAssignRole != null)
                    {
                        if (multiAssignRole.AnotherRole != null)
                        {
                            if (multiAssignRole.AnotherRole.Id == ExtremeRoleId.Bakary)
                            {
                                this.aliveBakary.Add(playerId);
                            }
                        }
                    }

                }
            }

            private void updateBakaryAlive()
            {
                if (this.aliveBakary.Count == 0) { return; }

                HashSet<byte> updatedBakary = new HashSet<byte>();

                foreach (var playerId in this.aliveBakary)
                {
                    PlayerControl player = Helper.Player.GetPlayerControlById(playerId);
                    if ((!player.Data.IsDead && !player.Data.Disconnected))
                    {
                        updatedBakary.Add(playerId);
                    }
                }

                this.aliveBakary = updatedBakary;
            }
        }
    }

}
