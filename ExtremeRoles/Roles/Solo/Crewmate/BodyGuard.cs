using System;
using System.Collections.Generic;

using UnityEngine;
using Hazel;

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
    public sealed class BodyGuard : SingleRoleBase, IRoleAbility, IRoleMeetingButtonAbility, IRoleUpdate
    {
        public enum BodyGuardOption
        {
            ShieldRange,
            FeatMeetingAbilityTaskGage,
            FeatMeetingReportTaskGage,
        }

        public enum BodyGuardRpcOps : byte
        {
            FeatShield,
            ResetShield
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

        private bool awakeMeetingAbility;
        private float meetingAbilityTaskGage;
        private bool awakeMeetingReport;
        private float meetingReportTaskGage;

        public BodyGuard() : base(
            ExtremeRoleId.BodyGuard,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.BodyGuard.ToString(),
            ColorPalette.BodyGuardOrange,
            false, true, false, false)
        { }

        public static void Ability(ref MessageReader reader)
        {
            BodyGuardRpcOps ops = (BodyGuardRpcOps)reader.ReadByte();
            switch (ops)
            {
                case BodyGuardRpcOps.FeatShield:
                    byte featBodyGuardPlayerId = reader.ReadByte();
                    byte targetPlayerId = reader.ReadByte();
                    featShield(featBodyGuardPlayerId, targetPlayerId);
                    break;
                case BodyGuardRpcOps.ResetShield:
                    byte resetBodyGuardPlayerId = reader.ReadByte();
                    resetShield(resetBodyGuardPlayerId);
                    break;
                default:
                    break;
            }

        }
        private static void featShield(byte rolePlayerId, byte targetPlayer)
        {
            ExtremeRolesPlugin.ShipState.ShildPlayer.Add(
                rolePlayerId, targetPlayer);
        }

        private static void resetShield(byte playerId)
        {
            ExtremeRolesPlugin.ShipState.ShildPlayer.Remove(playerId);
        }

        public override void ExiledAction(GameData.PlayerInfo rolePlayer)
        {
            resetShield(rolePlayer.PlayerId);
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer, PlayerControl killerPlayer)
        {
            resetShield(rolePlayer.PlayerId);

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
                    RPCOperator.Command.BodyGuardAbility,
                    new List<byte>
                    {
                        (byte)BodyGuardRpcOps.FeatShield,
                        playerId,
                        this.TargetPlayer
                    });
                featShield(playerId, this.TargetPlayer);
                
                this.TargetPlayer = byte.MaxValue;
            }
            else
            {
                RPCOperator.Call(
                    localPlayer.NetId,
                    RPCOperator.Command.BodyGuardAbility,
                    new List<byte>
                    {
                        (byte)BodyGuardRpcOps.ResetShield,
                        playerId
                    });
                resetShield(playerId);

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

        public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance)
        {

            int abilityNum = 0;

            AbilityCountButton button = this.shieldButton as AbilityCountButton;
            if (button != null)
            {
                abilityNum = button.CurAbilityNum;
            }

            return
                !this.awakeMeetingAbility ||
                abilityNum <= 0 ||
                instance.TargetPlayerId == 253;
        }

        public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
        {
            abilityButton.name = $"bodyGuardFeatShield_{instance.TargetPlayerId}";
            var controllerHighlight = abilityButton.transform.FindChild("ControllerHighlight");
            if (controllerHighlight != null)
            {
                controllerHighlight.localScale *= new Vector2(1.25f, 1.25f);
            }
        }

        public Action CreateAbilityAction(PlayerVoteArea instance)
        {
            PlayerControl player = CachedPlayerControl.LocalPlayer;
            byte targetPlayerId = instance.TargetPlayerId;

            void meetingfeatShield()
            {
                RPCOperator.Call(
                    player.NetId,
                    RPCOperator.Command.BodyGuardAbility,
                    new List<byte>
                    {
                        (byte)BodyGuardRpcOps.FeatShield,
                        player.PlayerId,
                        this.TargetPlayer
                    });
                featShield(player.PlayerId, targetPlayerId);
            }
            return meetingfeatShield;
        }

        public void SetSprite(SpriteRenderer render)
        {
            render.sprite = this.shildButtonImage;
            render.transform.localScale *= new Vector2(0.625f, 0.625f);
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (!this.awakeMeetingAbility || !this.awakeMeetingReport)
            {
                
                float taskGage = Player.GetPlayerTaskGage(rolePlayer);
                
                if (taskGage >= this.meetingAbilityTaskGage && 
                    !this.awakeMeetingAbility)
                {
                    this.awakeMeetingAbility = true;
                }
                if (taskGage >= this.meetingReportTaskGage &&
                    !this.awakeMeetingReport)
                {
                    this.awakeMeetingReport = true;
                }
                
            }
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

            CreateIntOption(
                BodyGuardOption.FeatMeetingAbilityTaskGage,
                30, 0, 100, 10,
                parentOps,
                format: OptionUnit.Percentage);
            CreateIntOption(
                BodyGuardOption.FeatMeetingReportTaskGage,
                60, 0, 100, 10,
                parentOps,
                format: OptionUnit.Percentage);

        }

        protected override void RoleSpecificInit()
        {

            var allOpt = OptionHolder.AllOption;

            this.shieldRange = allOpt[
                GetRoleOptionId(BodyGuardOption.ShieldRange)].GetValue();

            this.meetingAbilityTaskGage = (float)allOpt[
                GetRoleOptionId(BodyGuardOption.FeatMeetingAbilityTaskGage)].GetValue() / 100.0f;
            this.meetingReportTaskGage = (float)allOpt[
                GetRoleOptionId(BodyGuardOption.FeatMeetingReportTaskGage)].GetValue() / 100.0f;

            this.RoleAbilityInit();
            if (this.Button != null)
            {
                this.shildNum = ((AbilityCountButton)this.shieldButton).CurAbilityNum;
            }
        }
    }
}
