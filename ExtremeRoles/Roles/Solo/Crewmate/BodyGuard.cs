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
    public sealed class BodyGuard : 
        SingleRoleBase,
        IRoleAbility,
        IRoleMeetingButtonAbility,
        IRoleUpdate,
        IRoleSpecialReset
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

            private bool isReset;

            private Action baseCleanUp;
            private Action reduceCountAction;

            public BodyGuardShieldButton(
                string shieldAbilityText,
                string resetAbilityText,
                Func<bool> shieldAbility,
                Action resetAbility,
                Func<bool> canUse,
                Func<bool> resetCheckFunc,
                Sprite sprite,
                Sprite resetSprite,
                Action abilityCleanUp = null,
                Func<bool> abilityCheck = null,
                KeyCode hotkey = KeyCode.F
                ) : base(
                    shieldAbilityText,
                    shieldAbility,
                    canUse,
                    sprite,
                    abilityCleanUp,
                    abilityCheck,
                    hotkey)
            {
                var coolTimeText = this.GetCoolDownText();

                this.abilityCountText = GameObject.Instantiate(
                    coolTimeText, coolTimeText.transform.parent);
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

                this.reduceCountAction = this.reduceAbilityCountAction();

                if (HasCleanUp())
                {
                    this.baseCleanUp = new Action(this.AbilityCleanUp);
                    this.AbilityCleanUp += this.reduceCountAction;
                }
                else
                {
                    this.baseCleanUp = null;
                    this.AbilityCleanUp = this.reduceCountAction;
                }
            }

            public void UpdateAbilityCount(int newCount)
            {
                this.abilityNum = newCount;
                this.updateAbilityCountText();
                if (this.State == AbilityState.None)
                {
                    this.SetStatus(AbilityState.CoolDown);
                }
            }

            public override void ForceAbilityOff()
            {
                this.SetStatus(AbilityState.Ready);
                this.baseCleanUp?.Invoke();
            }

            protected override void DoClick()
            {
                if (this.IsEnable() &&
                    this.Timer <= 0f &&
                    this.IsAbilityReady())
                {
                    if (this.abilityNum <= 0 || this.isReset)
                    {
                        this.resetAction.Invoke();
                        this.ResetCoolTimer();
                    }
                    else if (
                        this.abilityNum > 0 && 
                        this.UseAbility.Invoke())
                    {
                        if (this.HasCleanUp())
                        {
                            this.SetStatus(AbilityState.Activating);
                        }
                        else
                        {
                            this.reduceCountAction.Invoke();
                            this.ResetCoolTimer();
                        }
                    }
                }
            }

            protected override bool IsEnable() =>
                this.CanUse.Invoke() && (this.abilityNum > 0 || this.isReset);

            protected override void UpdateAbility()
            {
                this.isReset = this.resetCheck.Invoke();
                if (this.isReset)
                {
                    this.abilityCountText.gameObject.SetActive(false);
                    this.SetButtonImg(this.resetSprite);
                    this.SetButtonText(this.resetAbilityText);
                }
                else
                {
                    this.abilityCountText.gameObject.SetActive(true);
                    this.SetButtonImg(this.defaultSprite);
                    this.SetButtonText(this.defaultAbilityText);
                }

                if (this.abilityNum <= 0 && !this.isReset)
                {
                    this.SetStatus(AbilityState.None);
                }
            }

            private Action reduceAbilityCountAction()
            {
                return () =>
                {
                    --this.abilityNum;
                    if (this.abilityCountText != null)
                    {
                        updateAbilityCountText();
                    }
                };
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
            IsReportPlayerName,
            ReportPlayerMode,
            IsBlockMeetingKill,
        }

        public enum BodyGuardRpcOps : byte
        {
            FeatShield,
            ResetShield,
            CoverDead,
            AwakeMeetingReport,
            ReportMeeting
        }

        public enum BodyGuardReportPlayerNameMode
        {
            GuardedPlayerNameOnly,
            BodyGuardPlayerNameOnly,
            BothPlayerName,
        }

        public RoleAbilityButtonBase Button
        {
            get => this.shieldButton;
            set
            {
                this.shieldButton = value;
            }
        }

        public static bool IsBlockMeetingKill { get; private set; } = true;

        private byte targetPlayer = byte.MaxValue;

        private int shildNum;
        private float shieldRange;
        private RoleAbilityButtonBase shieldButton;

        private Sprite shildButtonImage;

        private bool awakeMeetingAbility;
        private float meetingAbilityTaskGage;
        private bool awakeMeetingReport;
        private float meetingReportTaskGage;

        private bool isReportWithPlayerName;
        private BodyGuardReportPlayerNameMode reportMode;
        private string reportStr = string.Empty;

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
                    byte prevTargetPlayerId = reader.ReadByte();
                    byte targetBodyGuardPlayerId = reader.ReadByte();
                    coverDead(killerPlayerId, prevTargetPlayerId, targetBodyGuardPlayerId);
                    break;
                case BodyGuardRpcOps.AwakeMeetingReport:
                    awakeReportMeeting(reader.ReadByte());
                    break;
                case BodyGuardRpcOps.ReportMeeting:
                    reportMeeting(reader.ReadString());
                    break;
                default:
                    break;
            }

        }

        public static bool TryRpcKillGuardedBodyGuard(byte killerPlayerId, byte targetPlayerId)
        {
            if (!TryGetShiledPlayerId(targetPlayerId, out byte bodyGuardPlayerId))
            {
                return false;
            }

            return rpcTryKillBodyGuard(killerPlayerId, targetPlayerId, bodyGuardPlayerId);
        }

        public static bool TryGetShiledPlayerId(
            byte targetPlayerId, out byte bodyGuardPlayerId)
        {
            return shilded.TryGetBodyGuardPlayerId(targetPlayerId, out bodyGuardPlayerId);
        }

        private static bool rpcTryKillBodyGuard(
            byte killerPlayerId, byte prevTargetPlayerId, byte targetBodyGuard)
        {
            PlayerControl bodyGuardPlayer = Player.GetPlayerControlById(targetBodyGuard);
            if (bodyGuardPlayer == null ||
                bodyGuardPlayer.Data == null ||
                bodyGuardPlayer.Data.IsDead ||
                bodyGuardPlayer.Data.Disconnected)
            {
                return false;
            }

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.BodyGuardAbility))
            {
                caller.WriteByte((byte)BodyGuardRpcOps.CoverDead);
                caller.WriteByte(killerPlayerId);
                caller.WriteByte(prevTargetPlayerId);
                caller.WriteByte(targetBodyGuard);
            }
            coverDead(killerPlayerId, prevTargetPlayerId, targetBodyGuard);
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
            byte killerPlayerId, byte prevTargetPlayerId, byte targetBodyGuard)
        {
            if (targetBodyGuard == CachedPlayerControl.LocalPlayer.PlayerId)
            {
                Sound.PlaySound(Sound.SoundType.GuardianAngleGuard, 0.6f);
            }

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

            string reportStr = bodyGuard.isReportWithPlayerName ? 
               bodyGuard.reportMode switch
               {
                   BodyGuardReportPlayerNameMode.GuardedPlayerNameOnly =>
                       string.Format(
                           Translation.GetString("martyrdomReportWithGurdedPlayer"),
                           bodyGuardPlayer.Data.DefaultOutfit.PlayerName),
                   BodyGuardReportPlayerNameMode.BodyGuardPlayerNameOnly =>
                        string.Format(
                            Translation.GetString("martyrdomReportWithBodyGurdPlayer"),
                            bodyGuardPlayer.Data.DefaultOutfit.PlayerName),
                   BodyGuardReportPlayerNameMode.BothPlayerName =>
                       string.Format(
                           Translation.GetString("martyrdomReportWithBoth"),
                           bodyGuardPlayer.Data.DefaultOutfit.PlayerName,
                           Player.GetPlayerControlById(prevTargetPlayerId)?.Data.DefaultOutfit.PlayerName),
                   _ => Translation.GetString("martyrdomReport")
               } : 
               Translation.GetString("martyrdomReport");

            if (MeetingHud.Instance)
            {
                reportMeeting(reportStr);
            }
            else
            {
                bodyGuard.reportStr = reportStr;
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

        private static void reportMeeting(string text)
        {
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(
                CachedPlayerControl.LocalPlayer, text);
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


        public override void ExiledAction(PlayerControl rolePlayer)
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
                    Path.BodyGuardResetShield),
                null,
                null,
                KeyCode.F);

            this.RoleAbilityInit();
            if (this.shieldButton is BodyGuardShieldButton button)
            {
                button.UpdateAbilityCount(
                    OptionHolder.AllOption[GetRoleOptionId(
                        RoleAbilityCommonOption.AbilityCount)].GetValue());
            }
            this.Button.SetLabelToCrewmate();
        }

        public void Reset()
        {
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
            byte playerId = localPlayer.PlayerId;

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.BodyGuardAbility))
            {
                caller.WriteByte((byte)BodyGuardRpcOps.ResetShield);
                caller.WriteByte(playerId);
            }
            resetShield(playerId);

            if (this.shieldButton is BodyGuardShieldButton button)
            {
                button.UpdateAbilityCount(this.shildNum);
            }
        }

        public bool UseAbility()
        {
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
            byte playerId = localPlayer.PlayerId;

            if (this.targetPlayer != byte.MaxValue)
            {
                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.BodyGuardAbility))
                {
                    caller.WriteByte((byte)BodyGuardRpcOps.FeatShield);
                    caller.WriteByte(playerId);
                    caller.WriteByte(this.targetPlayer);
                }
                featShield(playerId, this.targetPlayer);

                this.targetPlayer = byte.MaxValue;

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsResetMode() => this.targetPlayer == byte.MaxValue;

        public bool IsAbilityUse()
        {

            this.targetPlayer = byte.MaxValue;

            PlayerControl target = Player.GetClosestPlayerInRange(
                CachedPlayerControl.LocalPlayer, this,
                this.shieldRange);

            if (target != null)
            {
                byte targetId = target.PlayerId;

                if (!shilded.IsShielding(
                    CachedPlayerControl.LocalPlayer.PlayerId, targetId))
                {
                    this.targetPlayer = targetId;
                }
            }

            return this.IsCommonUse();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            if (!string.IsNullOrEmpty(this.reportStr))
            {
                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.BodyGuardAbility))
                {
                    caller.WriteByte((byte)BodyGuardRpcOps.ReportMeeting);
                    caller.WriteStr(this.reportStr);
                }
                reportMeeting(this.reportStr);
            }
            this.reportStr = string.Empty;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            this.reportStr = string.Empty;
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

            if (this.shieldButton is BodyGuardShieldButton button)
            {
                return
                    !this.awakeMeetingAbility ||
                    button.CurAbilityNum <= 0 ||
                    instance.TargetPlayerId == 253;
            }
            else
            {
                return true;
            }
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
                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.BodyGuardAbility))
                {
                    caller.WriteByte((byte)BodyGuardRpcOps.FeatShield);
                    caller.WriteByte(player.PlayerId);
                    caller.WriteByte(targetPlayerId);
                }
                featShield(player.PlayerId, targetPlayerId);

                if (this.shieldButton is BodyGuardShieldButton button)
                {
                    button.UpdateAbilityCount(button.CurAbilityNum - 1);
                }
            }
            return meetingfeatShield;
        }

        public void AllReset(PlayerControl rolePlayer)
        {
            resetShield(rolePlayer.PlayerId);
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
                    using (var caller = RPCOperator.CreateCaller(
                        RPCOperator.Command.BodyGuardAbility))
                    {
                        caller.WriteByte((byte)BodyGuardRpcOps.AwakeMeetingReport);
                        caller.WriteByte(rolePlayer.PlayerId);
                    }
                    this.awakeMeetingReport = true;
                }
            }
            if (this.awakeMeetingAbility && MeetingHud.Instance)
            {
                if (this.meetingText == null)
                {
                    this.meetingText = UnityEngine.Object.Instantiate(
                        FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText,
                        MeetingHud.Instance.transform);
                    this.meetingText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                    this.meetingText.transform.position = Vector3.zero;
                    this.meetingText.transform.localPosition = new Vector3(-2.85f, 3.15f, -20f);
                    this.meetingText.transform.localScale *= 0.9f;
                    this.meetingText.color = Palette.White;
                    this.meetingText.gameObject.SetActive(false);
                }

                if (this.shieldButton is BodyGuardShieldButton button)
                {
                    this.meetingText.text = string.Format(
                        Helper.Translation.GetString("meetingShieldState"),
                        button.CurAbilityNum);
                }
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
            var reportPlayerNameOpt = CreateBoolOption(
                BodyGuardOption.IsReportPlayerName,
                false, parentOps);
            CreateSelectionOption(
                BodyGuardOption.ReportPlayerMode,
                new string[]
                {
                    BodyGuardReportPlayerNameMode.GuardedPlayerNameOnly.ToString(),
                    BodyGuardReportPlayerNameMode.BodyGuardPlayerNameOnly.ToString(),
                    BodyGuardReportPlayerNameMode.BothPlayerName.ToString(),
                }, reportPlayerNameOpt);
            CreateBoolOption(
                BodyGuardOption.IsBlockMeetingKill,
                true, parentOps);
        }

        protected override void RoleSpecificInit()
        {

            var allOpt = OptionHolder.AllOption;

            this.reportStr = string.Empty;

            IsBlockMeetingKill = allOpt[
                GetRoleOptionId(BodyGuardOption.IsBlockMeetingKill)].GetValue();

            this.shieldRange = allOpt[
                GetRoleOptionId(BodyGuardOption.ShieldRange)].GetValue();

            this.meetingAbilityTaskGage = (float)allOpt[
                GetRoleOptionId(BodyGuardOption.FeatMeetingAbilityTaskGage)].GetValue() / 100.0f;
            this.meetingReportTaskGage = (float)allOpt[
                GetRoleOptionId(BodyGuardOption.FeatMeetingReportTaskGage)].GetValue() / 100.0f;

            this.isReportWithPlayerName = allOpt[
                GetRoleOptionId(BodyGuardOption.IsReportPlayerName)].GetValue();
            this.reportMode = (BodyGuardReportPlayerNameMode)allOpt[
                GetRoleOptionId(BodyGuardOption.ReportPlayerMode)].GetValue();

            this.awakeMeetingAbility = this.meetingAbilityTaskGage <= 0.0f;
            this.awakeMeetingReport = this.meetingReportTaskGage <= 0.0f;

            this.RoleAbilityInit();
            if (this.shieldButton is BodyGuardShieldButton button)
            {
                this.shildNum = button.CurAbilityNum;
                button.UpdateAbilityCount(
                    allOpt[GetRoleOptionId(
                        RoleAbilityCommonOption.AbilityCount)].GetValue());
            }
        }
    }
}
