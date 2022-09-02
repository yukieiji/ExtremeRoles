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
            ResetShield,
            CoverDead,
            ReportMeeting
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

        private bool reportNextMeeting = false;

        private static ShildFeatedPlayer shilded = new ShildFeatedPlayer();

        private class ShildFeatedPlayer
        {
            private List<(byte, byte)> shield = new List<(byte, byte)>();

            public ShildFeatedPlayer()
            {
                Clear();
            }

            public void Clear()
            {
                this.shield.Clear();
            }

            public void Add(byte rolePlayerId, byte targetPlayerId)
            {
                this.shield.Add((rolePlayerId, targetPlayerId));
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
                    this.shield.Remove(val);
                }

            }
            public bool TryGetBodyGuardPlayerId(
                byte targetPlayerId, out byte bodyGuardPlayerId)
            {

                bodyGuardPlayerId = default(byte);
                if (this.shield.Count == 0) { return false; }

                foreach (var (rolePlayerId, shieldPlayerId) in this.shield)
                {
                    if (shieldPlayerId == targetPlayerId)
                    {
                        bodyGuardPlayerId = rolePlayerId;
                        return true; 
                    }
                }
                return false;
            }
            public bool IsShielding(byte rolePlayerId, byte targetPlayerId)
            {
                if (this.shield.Count == 0) { return false; }
                return this.shield.Contains((rolePlayerId, targetPlayerId));
            }
        }

        public BodyGuard() : base(
            ExtremeRoleId.BodyGuard,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.BodyGuard.ToString(),
            ColorPalette.BodyGuardOrange,
            false, true, false, false)
        { }

        public static void ResetAllShild()
        {
            shilded.Clear();
        }

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
                    resetShield(reader.ReadByte());
                    break;
                case BodyGuardRpcOps.CoverDead:
                    byte killerPlayerId = reader.ReadByte();
                    byte targetBodyGuardPlayerId = reader.ReadByte();
                    coverDead(killerPlayerId, targetBodyGuardPlayerId);
                    break;
                case BodyGuardRpcOps.ReportMeeting:
                    reportMeeting();
                    break;
                default:
                    break;
            }

        }

        public static bool TryGetShiledPlayerId(
            byte targetPlayerId, out byte bodyGuardPlayerId)
        {
            return shilded.TryGetBodyGuardPlayerId(targetPlayerId, out bodyGuardPlayerId);
        }

        public static bool RpcTryKillBodyGuard(
            byte killerPlayerId, byte targetBodyGuard)
        {
            PlayerControl bodyGuardPlayer = Player.GetPlayerControlById(targetBodyGuard);
            if (bodyGuardPlayer == null ||
                bodyGuardPlayer.Data == null ||
                bodyGuardPlayer.Data.IsDead ||
                bodyGuardPlayer.Data.Disconnected)
            {
                return false;
            }

            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.BodyGuardAbility,
                new List<byte>
                {
                    (byte)BodyGuardRpcOps.CoverDead,
                    killerPlayerId,
                    targetBodyGuard
                });
            coverDead(killerPlayerId, targetBodyGuard);
            return true;
        }

        private static void featShield(byte rolePlayerId, byte targetPlayer)
        {
            shilded.Add(rolePlayerId, targetPlayer);
        }

        private static void resetShield(byte playerId)
        {
            shilded.Remove(playerId);
        }

        private static void coverDead(
            byte killerPlayerId, byte targetBodyGuard)
        {
            // 必ずテレポートしないキル
            RPCOperator.UncheckedMurderPlayer(
                killerPlayerId, targetBodyGuard, byte.MinValue);
            
            PlayerControl bodyGuardPlayer = Player.GetPlayerControlById(targetBodyGuard);
            
            if (bodyGuardPlayer == null ||
                bodyGuardPlayer.Data == null ||
                !bodyGuardPlayer.Data.IsDead || // 死んでないつまり守護天使に守られた
                bodyGuardPlayer.Data.Disconnected)
            {
                return;
            }

            ExtremeRolesPlugin.ShipState.ReplaceDeadReason(
                targetBodyGuard, ExtremeShipStatus.PlayerStatus.Martyrdom);

            BodyGuard bodyGuard = ExtremeRoleManager.GetSafeCastedRole<BodyGuard>(
                targetBodyGuard);

            if (!bodyGuard.awakeMeetingReport) { return; }

            if (MeetingHud.Instance)
            {
                reportMeeting();
            }
            else
            {
                bodyGuard.reportNextMeeting = true;
            }
        }

        private static void reportMeeting()
        {
            if (CachedPlayerControl.LocalPlayer.Data.IsDead) { return; }

            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(
                CachedPlayerControl.LocalPlayer,
                Translation.GetString("martyrdomReport"));
        }

        public override void ExiledAction(GameData.PlayerInfo rolePlayer)
        {
            resetShield(rolePlayer.PlayerId);
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer, PlayerControl killerPlayer)
        {
            resetShield(rolePlayer.PlayerId);
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

                if (!shilded.IsShielding(
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
            if (this.reportNextMeeting)
            {
                PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
                RPCOperator.Call(
                    localPlayer.NetId,
                    RPCOperator.Command.BodyGuardAbility,
                    new List<byte>
                    {
                        (byte)BodyGuardRpcOps.ReportMeeting,
                    });
                reportMeeting();
            }
            this.reportNextMeeting = false;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            this.reportNextMeeting = false;
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
                AbilityCountButton button = this.shieldButton as AbilityCountButton;
                if (button != null)
                {
                    button.UpdateAbilityCount(button.CurAbilityNum - 1);
                }
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

            this.reportNextMeeting = false;

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
