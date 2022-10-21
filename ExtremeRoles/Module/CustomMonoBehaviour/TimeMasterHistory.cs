using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.Performance;
using Il2CppInterop.Runtime.Attributes;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public sealed class TimeMasterHistory : MonoBehaviour
    {
        public bool BlockAddHistory => this.isBlockAdd;

        // 座標、動けるか、ベント内か, 何か使ってるか
        private Queue<(Vector3, bool, bool, bool)> history = new Queue<
            (Vector3, bool, bool, bool)>();
        private bool init = false;
        private int size = 0;
        private bool isBlockAdd;

        public TimeMasterHistory(IntPtr ptr) : base(ptr) { }

        public void Awake()
        {
            this.Clear();
        }

        public void FixedUpdate()
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started ||
                MeetingHud.Instance ||
                ExileController.Instance) 
            { 
                return; 
            }
            if (!ExtremeRolesPlugin.ShipState.IsRoleSetUpEnd) { return; }
            if (!init || BlockAddHistory) { return; }

            PlayerControl player = CachedPlayerControl.LocalPlayer;

            int overflow = this.history.Count - size;
            for (int i = 0; i < overflow; ++i)
            {
                this.history.Dequeue();
            }

            this.history.Enqueue(
                (
                    player.transform.position,
                    player.CanMove,
                    player.inVent,
                    !player.Collider.enabled && !player.NetTransform.enabled && !player.moveable
                )
            );
        }

        public void SetAddHistoryBlock(bool active)
        {
            this.isBlockAdd = active;
        }

        public void Clear()
        {
            ResetAfterRewind();
            this.init = false;
            this.size = 0;
        }

        public void ResetAfterRewind()
        {
            this.history.Clear();
            SetAddHistoryBlock(false);
        }

        public void Initialize(float historySecond)
        {
            this.size = (int)Mathf.Round(historySecond / Time.fixedDeltaTime);
            this.init = true;
        }

        [HideFromIl2Cpp]
        public IEnumerable<
            (Vector3, bool, bool, bool)> GetAllHistory() => history.Reverse();

        public int GetSize() => this.size;
    }
}
