using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public sealed class Umbrer : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        private sealed class InfectedPlayer
        {
            private HashSet<byte> infectedPlayer = new HashSet<byte>();

            public InfectedPlayer()
            {
                
            }

            public void AddPlayer(byte playerId)
            {
            }

            public HashSet<byte> GetNoneInfectedPlayer()
            {
            }
            public HashSet<byte> GetNonePowerfulInfectedPlayer()
            {

            }
        }

        public enum UmbrerOption
        {
            CanUseVent,
            CanMoveVentToVent,
            HasTask,
            SeeImpostorTaskGage,
            CanSeeFromImpostor,
            CanSeeFromImpostorTaskGage,
        }


        public RoleAbilityButtonBase Button
        {
            get => this.madmateAbilityButton;
            set
            {
                this.madmateAbilityButton = value;
            }
        }

        private RoleAbilityButtonBase madmateAbilityButton;

        public Umbrer() : base(
            ExtremeRoleId.Umbrer,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Umbrer.ToString(),
            Palette.ImpostorRed,
            false, false, false, false)
        { }

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Helper.Translation.GetString("selfKill"),
                Loader.CreateSpriteFromResources(
                    Path.SucideSprite));
        }

        public bool UseAbility()
        {

            byte playerId = CachedPlayerControl.LocalPlayer.PlayerId;

            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.UncheckedMurderPlayer,
                new List<byte> { playerId, playerId, byte.MaxValue });
            RPCOperator.UncheckedMurderPlayer(
                playerId,
                playerId,
                byte.MaxValue);
            return true;
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void Update(PlayerControl rolePlayer)
        {
            
        }

        public override Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole, byte targetPlayerId)
        {
            if (targetRole.IsImpostor() || 
                targetRole.FakeImposter)
            {
                return Palette.ImpostorRed;
            }
            
            return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            

            this.CreateCommonAbilityOption(parentOps);
        }

        protected override void RoleSpecificInit()
        {
            var allOpt = OptionHolder.AllOption;
            this.isSeeImpostorNow = false;
            this.RoleAbilityInit();
        }
    }
}
