using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.ExtremeShipStatus;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class BodyGuard : SingleRoleBase, IRoleAbility
    {
        public enum BodyGuardOption
        {
            ShieldRange
        }

        public RoleAbilityButtonBase Button
        {
            get => this.shieldButton;
            set
            {
                this.shieldButton = value;
            }
        }

        public byte TargetPlayer = byte.MaxValue;

        private int shildNum;
        private float shieldRange;
        private RoleAbilityButtonBase shieldButton;

        private string shieldButtonText = string.Empty;
        private string shieldResetButtonText = string.Empty;

        private Sprite shildButtonImage;
        private Sprite shieldResetButtonImage;

        public BodyGuard() : base(
            ExtremeRoleId.BodyGuard,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.BodyGuard.ToString(),
            ColorPalette.BodyGuardOrange,
            false, true, false, false)
        { }

        public override void ExiledAction(GameData.PlayerInfo rolePlayer)
        {
            RPCOperator.BodyGuardResetShield(
                rolePlayer.PlayerId);
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer, PlayerControl killerPlayer)
        {
            RPCOperator.BodyGuardResetShield(
                rolePlayer.PlayerId);

            if (rolePlayer.PlayerId == killerPlayer.PlayerId) { return; }

            ExtremeRolesPlugin.ShipState.ReplaceDeadReason(
                rolePlayer.PlayerId,
                ExtremeShipStatus.PlayerStatus.Martyrdom);
        }

        public void CreateAbility()
        {

            this.shildButtonImage = Loader.CreateSpriteFromResources(
                 Path.BodyGuardShield);
            this.shieldResetButtonImage = Loader.CreateSpriteFromResources(
                 Path.TestButton);

            this.shieldButtonText = Translation.GetString("shield");
            this.shieldResetButtonText = Translation.GetString("resetShield");

            this.CreateAbilityCountButton(
                this.shieldButtonText,
                this.shildButtonImage);
            this.Button.SetLabelToCrewmate();
        }

        public bool UseAbility()
        {
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
            byte playerId = localPlayer.PlayerId;

            if (this.TargetPlayer != byte.MaxValue)
            {
                RPCOperator.Call(
                   localPlayer.NetId,
                    RPCOperator.Command.BodyGuardFeatShield,
                    new List<byte>
                    {
                        playerId,
                        this.TargetPlayer
                    });
                RPCOperator.BodyGuardFeatShield(
                    playerId, this.TargetPlayer);
                
                this.TargetPlayer = byte.MaxValue;
            }
            else
            {
                RPCOperator.Call(
                    localPlayer.NetId,
                    RPCOperator.Command.BodyGuardResetShield,
                    new List<byte>
                    {
                        playerId
                    });
                RPCOperator.BodyGuardResetShield(playerId);

                if (this.shieldButton == null) { return true; }

                ((AbilityCountButton)this.shieldButton).UpdateAbilityCount(
                    this.shildNum);
            }

            return true;
        }

        public bool IsAbilityUse()
        {

            this.TargetPlayer = byte.MaxValue;

            PlayerControl target = Player.GetClosestPlayerInRange(
                CachedPlayerControl.LocalPlayer, this,
                this.shieldRange);

            if (target != null)
            {
                byte targetId = target.PlayerId;

                if (!ExtremeRolesPlugin.ShipState.ShildPlayer.IsShielding(
                        CachedPlayerControl.LocalPlayer.PlayerId, targetId))
                {
                    this.TargetPlayer = targetId;
                }
            }
            
            if (this.TargetPlayer == byte.MaxValue)
            {
                this.shieldButton.SetButtonText(this.shieldResetButtonText);
                this.shieldButton.SetButtonImage(this.shieldResetButtonImage);
            }
            else
            {
                this.shieldButton.SetButtonText(this.shieldButtonText);
                this.shieldButton.SetButtonImage(this.shildButtonImage);
            }

            return this.IsCommonUse();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {

            CreateFloatOption(
                BodyGuardOption.ShieldRange,
                1.0f, 0.0f, 2.0f, 0.1f,
                parentOps);

            this.CreateAbilityCountOption(
                parentOps, 2, 5);
        }

        protected override void RoleSpecificInit()
        {
            this.shieldRange = OptionHolder.AllOption[
                GetRoleOptionId(BodyGuardOption.ShieldRange)].GetValue();

            this.RoleAbilityInit();
            if (this.Button != null)
            {
                this.shildNum = ((AbilityCountButton)this.shieldButton).CurAbilityNum;
            }
        }
    }
}
