using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public sealed class Umbrer : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        private sealed class InfectedContainer
        {
            public HashSet<PlayerControl> FirstStage => this.firstStage;
            public HashSet<PlayerControl> FinalStage => this.finalStage;

            private HashSet<PlayerControl> firstStage = new HashSet<PlayerControl>();
            private HashSet<PlayerControl> finalStage = new HashSet<PlayerControl>();

            public InfectedContainer()
            {
                this.firstStage.Clear();
                this.finalStage.Clear();
            }
            public void AddPlayer(PlayerControl player)
            {
                finalStage.Add(player);
            }

            public bool IsAllPlayerInfected()
            {
                if (this.firstStage.Count <= 0) { return false; }

                foreach (GameData.PlayerInfo player in
                    GameData.Instance.AllPlayers.GetFastEnumerator())
                {
                    if (player == null || player?.Object == null) { continue; }
                    if (player.IsDead || player.Disconnected) { continue; }

                    if (!this.firstStage.Contains(player.Object))
                    {
                        return false;
                    }

                }
                return true;
            }

            public bool IsContain(PlayerControl player) =>
                this.firstStage.Contains(player) || this.finalStage.Contains(player);

            public bool IsFirstStage(PlayerControl player) => 
                this.firstStage.Contains(player);

            public void Update()
            {
                removeToHashSet(ref this.firstStage);
                removeToHashSet(ref this.finalStage);
            }

            private void removeToHashSet(ref HashSet<PlayerControl> cont)
            {
                List<PlayerControl> remove = new List<PlayerControl>();

                foreach (PlayerControl player in this.firstStage)
                {
                    if (player == null ||
                        player.Data == null ||
                        player.Data.IsDead ||
                        player.Data.Disconnected)
                    {
                        remove.Add(player);
                    }
                }
                foreach (PlayerControl player in remove)
                {
                    cont.Remove(player);
                }
            }

        }

        private sealed class UmbrerVirusAbility : RoleAbilityButtonBase
        {
            public bool IsUpgradeMode => this.isUpgradeVirus;

            private Sprite setVirusSprite;
            private string setVirusButtonText;
            private float setVirusTime;

            private Sprite upgradeVirusSprite;
            private string upgradeVirusButtonText;
            private float upgradeVirusTime;

            private bool isUpgradeVirus;
            private Func<bool> upgradeVirusFunc;

            public UmbrerVirusAbility(
                string setVirusButtonText,
                string upgradeVirusButtonText,
                Sprite setVirusSprite,
                Sprite upgradeVirusSprite,
                float setVirusTime,
                float upgradeVirusTime,
                Func<bool> upgradeVirusModeCheck,
                Func<bool> ability,
                Func<bool> canUse,
                Vector3 positionOffset,
                Action abilityCleanUp = null,
                Func<bool> abilityCheck = null,
                KeyCode hotkey = KeyCode.F,
                bool mirror = false) : base(
                    setVirusButtonText,
                    ability, canUse,
                    setVirusSprite,
                    positionOffset,
                    abilityCleanUp,
                    abilityCheck,
                    hotkey, mirror)
            {

                this.setVirusSprite = setVirusSprite;
                this.setVirusButtonText = setVirusButtonText;
                this.setVirusTime = setVirusTime;

                this.upgradeVirusSprite = upgradeVirusSprite;
                this.upgradeVirusButtonText = upgradeVirusButtonText;
                this.upgradeVirusTime = upgradeVirusTime;

                this.isUpgradeVirus = false;
                this.upgradeVirusFunc = upgradeVirusModeCheck;
            }

            protected override void AbilityButtonUpdate()
            {
                this.isUpgradeVirus = this.upgradeVirusFunc();
                if (this.isUpgradeVirus)
                {
                    this.ButtonSprite = this.setVirusSprite;
                    this.ButtonText = this.setVirusButtonText;
                    this.AbilityActiveTime = this.setVirusTime;
                }
                else
                {
                    this.ButtonSprite = this.upgradeVirusSprite;
                    this.ButtonText = this.upgradeVirusButtonText;
                    this.AbilityActiveTime = this.upgradeVirusTime;
                }

                if (this.CanUse())
                {
                    this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.EnabledColor;
                    this.Button.graphic.material.SetFloat("_Desat", 0f);
                }
                else
                {
                    this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.DisabledClear;
                    this.Button.graphic.material.SetFloat("_Desat", 1f);
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
                    this.ResetCoolTimer();
                }

                Button.SetCoolDown(
                    this.Timer,
                    (this.IsHasCleanUp() && this.IsAbilityOn) ? this.AbilityActiveTime : this.CoolTime);
            }

            protected override void OnClickEvent()
            {
                if (this.CanUse() &&
                    this.Timer < 0f &&
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
                            this.ResetCoolTimer();
                        }
                    }
                }
            }
        }
        public enum UmbrerOption
        {
            
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
        private InfectedContainer container;

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
            if (CachedShipStatus.Instance == null ||
                this.IsWin ||
                GameData.Instance == null) { return; }
            if (!CachedShipStatus.Instance.enabled) { return; }

            if (this.container.IsAllPlayerInfected())
            {
                this.IsWin = true;
                RPCOperator.RoleIsWin(rolePlayer.PlayerId);
                return;
            }

            this.container.Update();
        }

        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            if (this.Id == targetRole.Id)
            {
                if (OptionHolder.Ship.IsSameNeutralSameWin)
                {
                    return true;
                }
                else
                {
                    return this.IsSameControlId(targetRole);
                }
            }
            else
            {
                return base.IsSameTeam(targetRole);
            }
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            

            this.CreateCommonAbilityOption(parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.container = new InfectedContainer();

            var allOpt = OptionHolder.AllOption;

            this.RoleAbilityInit();
        }
    }
}
