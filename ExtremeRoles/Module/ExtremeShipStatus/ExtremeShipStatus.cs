using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        public ShieldPlayerContainer ShildPlayer = new ShieldPlayerContainer();

        private GameObject status;

        public ExtremeShipStatus()
        {
            Initialize(false);
            this.playerVersion.Clear();
        }

        public void Initialize(
            bool includeGameObject = true)
        {
            ShildPlayer.Clear();

            // 以下リファクタ済み
            
            this.resetDeadPlayerInfo();
            this.resetGhostAbilityReport();
            this.resetGlobalAction();
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
                this.history = null;
                this.union = null;
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
    }
}
