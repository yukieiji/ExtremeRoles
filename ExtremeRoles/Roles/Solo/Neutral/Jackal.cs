using System;
using System.Collections.Generic;

using HarmonyLib;
using Hazel;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Jackal : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        public enum JackalSetting
        {
            SidekickNum,
            SidekickLimitNum,

            UpgradeSidekickNum,

            CanSetImpostorToSideKick,
            CanSeeImpostorToSideKickImpostor,
            SidekickUseSabotage,
            SidekickJackalCanMakeSidekick,
        }

        public List<byte> SideKickPlayerId = new List<byte>();

        public RoleAbilityButton Button
        {
            get => this.createSideKick;
            set
            {
                this.createSideKick = value;
            }
        }

        public int NumAbility = 0;
        public int CurRecursion = 0;
        public int SidekickRecursionLimit = 0;

        private RoleAbilityButton createSideKick;

        private int numUpgradeSideKick = 0;

        private bool isAlreadyUpgrated = false;

        private bool canSetImpostorToSideKick = false;
        private bool canSeeImpostorToSideKickImpostor = false;
        private bool sideKickUseSabotage = false;
        private bool sideKickUseVent = false;
        private bool sidekickJackalCanMakeSidekick = false;

        public Jackal() : base(
            ExtremeRoleId.Jackal,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Jackal.ToString(),
            ColorPalette.JackalBlue,
            true, false, true, false)
        {
            this.SideKickPlayerId.Clear();
            this.isAlreadyUpgrated = false;
        }

        public static void TargetToSideKick(byte callerId, byte targetId)
        {

        }

        public void CreateAbility()
        {
            throw new NotImplementedException();
        }

        public bool IsAbilityUse()
        {
            throw new NotImplementedException();
        }

        public void UseAbility()
        {
            throw new NotImplementedException();
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (this.SideKickPlayerId.Count == 0 || this.isAlreadyUpgrated) { return; }

            if (rolePlayer.Data.IsDead || rolePlayer.Data.Disconnected)
            {
                this.isAlreadyUpgrated = true;
                for (int i = 0; i < this.numUpgradeSideKick; ++i)
                {
                    int useIndex = UnityEngine.Random.Range(0, this.SideKickPlayerId.Count);
                    byte targetPlayerId = this.SideKickPlayerId[useIndex];
                    this.SideKickPlayerId.RemoveAt(useIndex);

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                        rolePlayer.NetId, (byte)CustomRPC.ReplaceRole, Hazel.SendOption.Reliable, -1);

                    writer.Write(rolePlayer.PlayerId);
                    writer.Write(targetPlayerId);
                    writer.Write(
                        (byte)ExtremeRoleManager.ReplaceOperation.SidekickToJackal);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);

                    Sidekick.BecomeToJackal(rolePlayer.PlayerId, targetPlayerId);
                }
            }

        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            throw new NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new NotImplementedException();
        }
    }

    public class Sidekick : SingleRoleBase
    {
        public bool IsPrevRoleImpostor = false;
        public bool CanSeeImpostorToSideKickImpostor = false;

        private int recursion = 0;
        private bool sidekickJackalCanMakeSidekick = false;

        public Sidekick(
            int curRecursion,
            bool isImpostor,
            bool useSabotage,
            bool useVent,
            bool canSeeImpostorToSideKickImpostor,
            bool sidekickJackalCanMakeSidekick
            ) : base(
                ExtremeRoleId.Sidekick,
                ExtremeRoleType.Neutral,
                ExtremeRoleId.Sidekick.ToString(),
                ColorPalette.JackalBlue,
                false, false, useVent, useSabotage)
        {
            this.IsPrevRoleImpostor = isImpostor;
            this.CanSeeImpostorToSideKickImpostor = canSeeImpostorToSideKickImpostor;

            this.recursion = curRecursion;
            this.sidekickJackalCanMakeSidekick = sidekickJackalCanMakeSidekick;
        }

        public static void BecomeToJackal(byte callerId, byte targetId)
        {

            var newJackal = new Jackal();
            var curJackal = (Jackal)ExtremeRoleManager.GameRole[callerId];
            var curSideKick = (Sidekick)ExtremeRoleManager.GameRole[targetId];
            
            newJackal.GameInit();
            if (!curSideKick.sidekickJackalCanMakeSidekick || curSideKick.recursion >= newJackal.SidekickRecursionLimit)
            {
                newJackal.NumAbility = 0;
            }
            newJackal.CurRecursion = curSideKick.recursion + 1;
            newJackal.SideKickPlayerId = new List<byte> (curJackal.SideKickPlayerId);

            ExtremeRoleManager.GameRole[targetId] = newJackal;

        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            throw new Exception("Don't call this class method!!");
        }

        protected override void RoleSpecificInit()
        {
            throw new Exception("Don't call this class method!!");
        }
    }

}
