using System;
using System.Collections.Generic;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public sealed class BakeryUnion : MonoBehaviour
    {
        private bool isChangeCooking = false;

        private float timer = 0.0f;
        private float goodTime = 0.0f;
        private float badTime = 0.0f;
        private bool isUnion = false;
        private HashSet<byte> aliveBakary = new HashSet<byte>();

        public BakeryUnion(IntPtr ptr) : base(ptr) { }

        public void Awake()
        {
            isUnion = false;
            isChangeCooking = false;
            aliveBakary.Clear();
        }

        public void FixedUpdate()
        {
            if (AmongUsClient.Instance.GameState != 
                InnerNet.InnerNetClient.GameStates.Started) { return; }
            if (!ExtremeRolesPlugin.ShipState.IsRoleSetUpEnd) { return; }

            if (!isUnion) { organize(); }
            if (aliveBakary.Count == 0) { return; }
            if (MeetingHud.Instance != null) { return; }

            this.timer += Time.fixedDeltaTime;
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
                return Translation.GetString("goodBread");
            }

            if (timer < goodTime)
            {
                return Translation.GetString("rawBread");
            }
            else if (goodTime <= timer && timer < badTime)
            {
                return Translation.GetString("goodBread");
            }
            else
            {
                return Translation.GetString("badBread");
            }
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

        public void ResetTimer()
        {
            timer = 0;
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
                PlayerControl player = Player.GetPlayerControlById(playerId);
                if (!player.Data.IsDead && !player.Data.Disconnected)
                {
                    updatedBakary.Add(playerId);
                }
            }

            aliveBakary = updatedBakary;
        }
    }
}
