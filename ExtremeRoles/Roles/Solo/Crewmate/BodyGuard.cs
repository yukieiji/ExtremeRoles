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
        public class BodyGuardShieldButton : RoleAbilityButtonBase
        {
            public int CurAbilityNum
            {
                get => this.abilityNum;
            }

            private int abilityNum = 0;
            private TMPro.TextMeshPro abilityCountText = null;

            private Func<bool> resetCheck;
            private Action resetAction;
            private string resetAbilityText;
            private Sprite resetSprite;

            private string defaultAbilityText;
            private Sprite defaultSprite;

            public BodyGuardShieldButton(
                string shieldAbilityText,
                string resetAbilityText,
                Func<bool> shieldAbility,
                Action resetAbility,
                Func<bool> canUse,
                Func<bool> resetCheckFunc,
                Sprite sprite,
                Sprite resetSprite,
                Vector3 positionOffset,
                Action abilityCleanUp = null,
                Func<bool> abilityCheck = null,
                KeyCode hotkey = KeyCode.F,
                bool mirror = false) : base(
                    shieldAbilityText,
                    shieldAbility,
                    canUse,
                    sprite,
                    positionOffset,
                    abilityCleanUp,
                    abilityCheck,
                    hotkey, mirror)
            {
                this.abilityCountText = GameObject.Instantiate(
                    this.Button.cooldownTimerText,
                    this.Button.cooldownTimerText.transform.parent);
                updateAbilityCountText();
                this.abilityCountText.enableWordWrapping = false;
                this.abilityCountText.transform.localScale = Vector3.one * 0.5f;
                this.abilityCountText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);

                this.resetAction = resetAbility;
                
                this.resetCheck = resetCheckFunc;
                this.resetAbilityText = resetAbilityText;
                this.resetSprite = resetSprite;

                this.defaultAbilityText = shieldAbilityText;
                this.defaultSprite = sprite;
            }

            public void UpdateAbilityCount(int newCount)
            {
                this.abilityNum = newCount;
                this.updateAbilityCountText();
            }

            protected override void AbilityButtonUpdate()
            {

                bool isReset = this.resetCheck();
                if (isReset)
                {
                    this.abilityCountText.gameObject.SetActive(false);
                    this.ButtonSprite = this.resetSprite;
                    this.ButtonText = this.resetAbilityText;
                }
                else
                {
                    this.abilityCountText.gameObject.SetActive(true);
                    this.ButtonSprite = this.defaultSprite;
                    this.ButtonText = this.defaultAbilityText;
                }

                if (this.CanUse() && 
                    (this.abilityNum > 0 || isReset))
                {
                    this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.EnabledColor;
                    this.Button.graphic.material.SetFloat("_Desat", 0f);
                }
                else
                {
                    this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.DisabledClear;
                    this.Button.graphic.material.SetFloat("_Desat", 1f);
                }
                if (this.abilityNum == 0 && !isReset)
                {
                    Button.SetCoolDown(this.Timer, this.CoolTime);
                    return;
                }
                if (this.Timer >= 0)
                {
                    bool abilityOn = this.IsHasCleanUp() && IsAbilityOn;

                    if (abilityOn || (
                            !CachedPlayerControl.LocalPlayer.PlayerControl.inVent &&
                            CachedPlayerControl.LocalPlayer.PlayerControl.moveable))
                    {
                        this.Timer -= Time.deltaTime;
                    }
                    if (abilityOn)
                    {
                        if (!this.AbilityCheck())
                        {
                            this.Timer = 0;
                            this.IsAbilityOn = false;
                        }
                    }
                }

                if (this.Timer <= 0 && this.IsHasCleanUp() && IsAbilityOn)
                {
                    this.IsAbilityOn = false;
                    this.Button.cooldownTimerText.color = Palette.EnabledColor;
                    this.CleanUp();
                    this.reduceAbilityCount();
                    this.ResetCoolTimer();
                }

                if (this.abilityNum > 0 || isReset)
                {
                    Button.SetCoolDown(
                        this.Timer,
                        (this.IsHasCleanUp() && this.IsAbilityOn) ? this.AbilityActiveTime : this.CoolTime);
                }
            }

            protected override void OnClickEvent()
            {
                if (this.CanUse() &&
                    this.Timer < 0f &&
                    !this.IsAbilityOn)
                {
                    Button.graphic.color = this.DisableColor;

                    if (this.abilityNum <= 0 || this.resetCheck())
                    {
                        this.resetAction();
                    }
                    else if (this.abilityNum > 0 && this.UseAbility())
                    {
                        if (this.IsHasCleanUp())
                        {
                            this.Timer = this.AbilityActiveTime;
                            Button.cooldownTimerText.color = this.TimerOnColor;
                            this.IsAbilityOn = true;
                        }
                        else
                        {
                            this.reduceAbilityCount();
                            this.ResetCoolTimer();
                        }
                    }
                }
            }

            private void reduceAbilityCount()
            {
                --this.abilityNum;
                if (this.abilityCountText != null)
                {
                    updateAbilityCountText();
                }
            }

            private void updateAbilityCountText()
            {
                this.abilityCountText.text = Translation.GetString("buttonCountText") + string.Format(
                    Translation.GetString(OptionUnit.Shot.ToString()), this.abilityNum);
            }

        }

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
            AwakeMeetingReport,
            ReportMeeting
        }

        public RoleAbilityButtonBase Button
        {
            get => this.shieldButton;
            set
            {
                this.shieldButton = value;
                this.castedShieldButton = (BodyGuardShieldButton)this.shieldButton;
            }
        }

        public byte TargetPlayer = byte.MaxValue;

        private int shildNum;
        private float shieldRange;
        private RoleAbilityButtonBase shieldButton;
        private BodyGuardShieldButton castedShieldButton;

        private Sprite shildButtonImage;

        private bool awakeMeetingAbility;
        private float meetingAbilityTaskGage;
        private bool awakeMeetingReport;
        private float meetingReportTaskGage;

        private bool reportNextMeeting = false;

        private TMPro.TextMeshPro meetingText;

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
                case BodyGuardRpcOps.AwakeMeetingReport:
                    awakeReportMeeting(reader.ReadByte());
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

            if (bodyGuard == null || 
                !bodyGuard.awakeMeetingReport) { return; }

            if (MeetingHud.Instance)
            {
                reportMeeting();
            }
            else
            {
                bodyGuard.reportNextMeeting = true;
            }
        }

        private static void awakeReportMeeting(byte bodyGuardPlayerId)
        {
            BodyGuard bodyGuard = ExtremeRoleManager.GetSafeCastedRole<BodyGuard>(
                bodyGuardPlayerId);
            if (bodyGuard != null)
            {
                bodyGuard.awakeMeetingReport = true;
            }
        }

        private static void reportMeeting()
        {
            if (CachedPlayerControl.LocalPlayer.Data.IsDead) { return; }

            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(
                CachedPlayerControl.LocalPlayer,
                Translation.GetString("martyrdomReport"));
        }

        public override string GetRolePlayerNameTag(
            SingleRoleBase targetRole, byte targetPlayerId)
        {
            if (!shilded.TryGetBodyGuardPlayerId(
                targetPlayerId, out byte bodyGuardPlayerId)) { return string.Empty; }

            if (bodyGuardPlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
            {
                return Design.ColoedString(
                    this.NameColor, $" ■");
            }

            return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
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

            this.Button = new BodyGuardShieldButton(
                Translation.GetString("shield"),
                Translation.GetString("resetShield"),
                UseAbility,
                Reset,
                IsAbilityUse,
                IsResetMode,
                this.shildButtonImage,
                Loader.CreateSpriteFromResources(
                    Path.TestButton),
                new Vector3(-1.8f, -0.06f, 0),
                null,
                null,
                KeyCode.F,
                false);

            this.RoleAbilityInit();

            this.castedShieldButton.UpdateAbilityCount(
                OptionHolder.AllOption[GetRoleOptionId(
                    RoleAbilityCommonOption.AbilityCount)].GetValue());
            this.Button.SetLabelToCrewmate();
        }

        public void Reset()
        {
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
            byte playerId = localPlayer.PlayerId;

            RPCOperator.Call(
                localPlayer.NetId,
                RPCOperator.Command.BodyGuardAbility,
                new List<byte>
                {
                    (byte)BodyGuardRpcOps.ResetShield,
                    playerId
                });
            resetShield(playerId);

            if (this.shieldButton == null) { return; }

            this.castedShieldButton.UpdateAbilityCount(this.shildNum);
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

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsResetMode() => this.TargetPlayer == byte.MaxValue;

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
            byte bodyGuardPlayerId = CachedPlayerControl.LocalPlayer.PlayerId;
            byte targetPlayerId = instance.TargetPlayerId;

            if (targetPlayerId == bodyGuardPlayerId) { return true; }

            bool isProtected = shilded.TryGetBodyGuardPlayerId(
                targetPlayerId, out byte featShildBodyGuardPlayerId);
            if (featShildBodyGuardPlayerId == bodyGuardPlayerId &&
                isProtected)
            {
                return true;
            }

            return
                !this.awakeMeetingAbility ||
                this.castedShieldButton.CurAbilityNum <= 0 ||
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
                        targetPlayerId
                    });
                featShield(player.PlayerId, targetPlayerId);
                this.castedShieldButton.UpdateAbilityCount(
                    this.castedShieldButton.CurAbilityNum - 1);
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
                    RPCOperator.Call(
                        CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                        RPCOperator.Command.BodyGuardAbility,
                        new List<byte>
                        {
                            (byte)BodyGuardRpcOps.AwakeMeetingReport,
                            rolePlayer.PlayerId,
                        });
                    this.awakeMeetingReport = true;
                }
            }
            if (this.awakeMeetingReport && MeetingHud.Instance)
            {
                if (this.meetingText == null)
                {
                    this.meetingText = UnityEngine.Object.Instantiate(
                        FastDestroyableSingleton<HudManager>.Instance.TaskText,
                        MeetingHud.Instance.transform);
                    this.meetingText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                    this.meetingText.transform.position = Vector3.zero;
                    this.meetingText.transform.localPosition = new Vector3(-2.85f, 3.15f, -20f);
                    this.meetingText.transform.localScale *= 0.9f;
                    this.meetingText.color = Palette.White;
                    this.meetingText.gameObject.SetActive(false);
                }

                this.meetingText.text = string.Format(
                    Helper.Translation.GetString("meetingShieldState"),
                    this.castedShieldButton.CurAbilityNum);
                this.meetingText.gameObject.SetActive(true);
            }
            else
            {
                if (this.meetingText != null)
                {
                    this.meetingText.gameObject.SetActive(false);
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
            this.awakeMeetingAbility = this.meetingAbilityTaskGage <= 0.0f;
            this.awakeMeetingReport = this.meetingReportTaskGage <= 0.0f;

            this.RoleAbilityInit();
            this.shildNum = this.castedShieldButton.CurAbilityNum;
        }
    }
}
