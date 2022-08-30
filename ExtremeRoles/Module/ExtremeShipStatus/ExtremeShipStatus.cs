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

        public Dictionary<int, Version> PlayerVersion = new Dictionary<int, Version>();

        public HashSet<byte> DeadedAssassin = new HashSet<byte>();

        public ShieldPlayerContainer ShildPlayer = new ShieldPlayerContainer();
        public PlayerHistory History = new PlayerHistory();
        public BakaryUnion Union = new BakaryUnion();


        public int MeetingsCount = 0;

        public bool IsAssassinAssign = false;
        public bool AssassinMeetingTrigger = false;
        public bool AssassinateMarin = false;
        public byte ExiledAssassinId = byte.MaxValue;
        public byte IsMarinPlayerId = byte.MaxValue;

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

            Union.Clear();

            History.Clear();

            MeetingsCount = 0;

            AssassinMeetingTrigger = false;
            AssassinateMarin = false;
            IsAssassinAssign = false;


            ExiledAssassinId = byte.MaxValue;
            IsMarinPlayerId = byte.MaxValue;

            // 以下リファクタ済み
            
            this.resetDeadPlayerInfo();
            this.resetPlayerSummary();
            this.resetRoleAssign();
            this.resetVent();
            this.resetWins();

            this.ClearMeetingResetObject();
            this.ResetGhostAbilityReport();
            this.ResetVison();

            if (!includeGameObject) { return; }

            if (this.status != null)
            {
                UnityEngine.Object.Destroy(this.status);
                this.status = null;
            }
            this.status = new GameObject("ExtremeShipStatus");

            this.resetUpdateObject();
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
