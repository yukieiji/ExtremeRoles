using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;

using ExtremeRoles.Module.AbilityButton.Roles;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Eater : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        public class EaterAbilityButton : RoleAbilityButtonBase
        {
            public int CurAbilityNum
            {
                get => this.abilityNum;
            }

            private int abilityNum = 0;
            private bool isKillEatMode;
            private float killEatTime;

            private string deadBodyEatString;
            private string killEatString;
            private Sprite deadBodyEatSprite;
            private Sprite killEatSprite;
            private Func<bool> killEatModeCheck;

            private TMPro.TextMeshPro abilityCountText = null;

            public EaterAbilityButton(
                Func<bool> ability,
                Func<bool> canUse,
                Sprite deadBodyEatSprite,
                Sprite killEatSprite,
                Vector3 positionOffset,
                Action abilityCleanUp,
                Func<bool> abilityCheck,
                Func<bool> killEatModeCheck,
                float killEatTime,
                KeyCode hotkey = KeyCode.F,
                bool mirror = false) : base(
                    "",
                    ability,
                    canUse,
                    deadBodyEatSprite,
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

                this.killEatModeCheck = killEatModeCheck;

                this.deadBodyEatString = Translation.GetString("deadBodyEat");
                this.killEatString = Translation.GetString("eatKill");
                this.ButtonText = this.deadBodyEatString;

                this.deadBodyEatSprite = deadBodyEatSprite;
                this.killEatSprite = killEatSprite;

                this.killEatTime = killEatTime;

                this.isKillEatMode = false;
            }

            public void UpdateAbilityCount(int newCount)
            {
                this.abilityNum = newCount;
                this.updateAbilityCountText();
            }

            protected override void AbilityButtonUpdate()
            {

                this.isKillEatMode = this.killEatModeCheck();
                if (this.isKillEatMode)
                {
                    this.ButtonSprite = this.killEatSprite;
                    this.ButtonText = this.killEatString;
                    this.AbilityActiveTime = this.killEatTime;
                }
                else
                {
                    this.ButtonSprite = this.deadBodyEatSprite;
                    this.ButtonText = this.deadBodyEatString;
                    this.AbilityActiveTime = 0.0f;
                }

                if (this.CanUse() && this.abilityNum > 0)
                {
                    this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.EnabledColor;
                    this.Button.graphic.material.SetFloat("_Desat", 0f);
                }
                else
                {
                    this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.DisabledClear;
                    this.Button.graphic.material.SetFloat("_Desat", 1f);
                }
                if (this.abilityNum == 0)
                {
                    Button.SetCoolDown(0, this.CoolTime);
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

                if (this.abilityNum > 0)
                {
                    Button.SetCoolDown(
                        this.Timer,
                        (this.IsHasCleanUp() && this.IsAbilityOn) ? this.AbilityActiveTime : this.CoolTime);
                    this.updateAbilityCountText();
                }
            }

            protected override void OnClickEvent()
            {
                if (this.CanUse() &&
                    this.Timer < 0f &&
                    this.abilityNum > 0 &&
                    !this.IsAbilityOn)
                {
                    Button.graphic.color = this.DisableColor;

                    if (this.UseAbility())
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
                this.abilityNum = this.abilityNum - 1;
                updateAbilityCountText();
            }

            private void updateAbilityCountText()
            {
                if (this.abilityCountText == null) { return; }

                this.abilityCountText.text = string.Format(
                    Translation.GetString("eaterWinNum"),
                        this.abilityNum);
            }
        }

        public enum EaterOption
        {
            DeadBodyEateRange,
        }

        public RoleAbilityButtonBase Button
        { 
            get => this.eatButton;
            set
            {
                this.eatButton = value;
            }
        }

        private RoleAbilityButtonBase eatButton;
        
        private float range;

        public Eater() : base(
           ExtremeRoleId.Eater,
           ExtremeRoleType.Neutral,
           ExtremeRoleId.Eater.ToString(),
           ColorPalette.TotocalcioGreen,
           false, false, false, false)
        { }

        public void CreateAbility()
        {
            var allOpt = OptionHolder.AllOption;

            this.Button = new EaterAbilityButton(
                UseAbility,
                IsAbilityUse,
                Loader.CreateSpriteFromResources(
                    Path.CarpenterSetCamera),
                Loader.CreateSpriteFromResources(
                    Path.CarpenterVentSeal),
                new Vector3(-1.8f, -0.06f, 0),
                CleanUp,
                IsAbilityCheck,
                IskillEatMode,
                (float)allOpt[GetRoleOptionId(
                    RoleAbilityCommonOption.AbilityActiveTime)].GetValue());

            abilityInit();
            this.Button.SetLabelToCrewmate();
        }

        public bool IsAbilityUse()
        {

            return this.IsCommonUse();
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            
        }

        public bool UseAbility()
        {
            
            return true;
        }

        public void Update(PlayerControl rolePlayer)
        {

            if (CachedShipStatus.Instance == null ||
                GameData.Instance == null ||
                this.IsWin) { return; }
            if (!CachedShipStatus.Instance.enabled ||
                ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger) { return; }

            if (this.eatButton == null) { return; }

            EaterAbilityButton eaterButton = (EaterAbilityButton)this.eatButton;

            if (eaterButton.CurAbilityNum != 0) { return; }

            RPCOperator.Call(
                rolePlayer.NetId,
                RPCOperator.Command.SetRoleWin,
                new List<byte> { rolePlayer.PlayerId });
            this.IsWin = true;
        }

        public void CleanUp()
        {

        }

        public bool IskillEatMode()
        {

        }

        public bool IsAbilityCheck()
        {

        }

        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            var multiAssignRole = targetRole as MultiAssignRoleBase;

            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.IsSameTeam(multiAssignRole.AnotherRole);
                }
            }
            if (OptionHolder.Ship.IsSameNeutralSameWin)
            {
                return this.Id == targetRole.Id;
            }
            else
            {
                return (this.Id == targetRole.Id) && this.IsSameControlId(targetRole);
            }
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {

            CreateFloatOption(
                EaterOption.DeadBodyEateRange,
                1.0f, 0.0f, 2.0f, 0.1f,
                parentOps);

            
        }

        protected override void RoleSpecificInit()
        {

        }
        private void abilityInit()
        {
            if (this.Button == null) { return; }

            var allOps = OptionHolder.AllOption;
            this.Button.SetAbilityCoolTime(
                allOps[GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)].GetValue());

            EaterAbilityButton button = this.Button as EaterAbilityButton;

            if (button != null)
            {
                button.UpdateAbilityCount(
                    allOps[GetRoleOptionId(RoleAbilityCommonOption.AbilityCount)].GetValue());
            }

            this.Button.ResetCoolTimer();
        }
    }
}
