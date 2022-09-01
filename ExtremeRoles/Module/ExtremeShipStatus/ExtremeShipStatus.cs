using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        public HashSet<byte> DeadedAssassin = new HashSet<byte>();

        public ShieldPlayerContainer ShildPlayer = new ShieldPlayerContainer();
        public PlayerHistory History = new PlayerHistory();

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

            History.Clear();

            AssassinMeetingTrigger = false;
            AssassinateMarin = false;
            IsAssassinAssign = false;

            ExiledAssassinId = byte.MaxValue;
            IsMarinPlayerId = byte.MaxValue;

            // 以下リファクタ済み
            
            this.resetDeadPlayerInfo();
            this.resetGhostAbilityReport();
            this.resetPlayerSummary();
            this.resetMeetingCount();
            this.resetRoleAssign();
            this.resetVent();
            this.resetWins();

            this.ClearMeetingResetObject();
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
    }
}
