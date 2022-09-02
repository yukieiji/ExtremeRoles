using System;
using System.Collections.Generic;

using UnityEngine;
using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class SandWorm : SingleRoleBase, IRoleAbility
    {
        public sealed class AssaultButton : RoleAbilityButtonBase
        {
            public AssaultButton(
                Func<bool> ability,
                Func<bool> canUse,
                Sprite sprite) : base(
                    Translation.GetString("assault"),
                    ability,
                    canUse,
                    sprite,
                    new Vector3(-1.8f, -0.06f, 0),
                    null, null,
                    KeyCode.F, false)
            { }

            protected override void AbilityButtonUpdate()
            {
                bool isLightOff = false;
                foreach (PlayerTask task in 
                    CachedPlayerControl.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
                {
                    if (task.TaskType == TaskTypes.FixLights)
                    {
                        isLightOff = true;
                        break;
                    }
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

                if (this.Timer >= 0 && (isVentIn() || isLightOff))
                {
                    this.Timer -= Time.deltaTime;
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
                        this.ResetCoolTimer();
                    }
                }
            }
        }

        public enum SandWormOption
        {
            AssaultKillCoolReduce,
            KillCoolPenalty,
            AssaultRange,
        }

        public RoleAbilityButtonBase Button
        {
            get => this.assaultButton;
            set
            {
                this.assaultButton = value;
            }
        }

        private float killPenalty;
        private float killBonus;

        private float range;

        private RoleAbilityButtonBase assaultButton;
        private PlayerControl targetPlayer = null;

        public SandWorm() : base(
            ExtremeRoleId.SandWorm,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.SandWorm.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public override bool TryRolePlayerKillTo(
            PlayerControl rolePlayer, PlayerControl targetPlayer)
        {
            
            bool isLightOff = false;

            foreach (PlayerTask task in targetPlayer.myTasks.GetFastEnumerator())
            {
                if (task.TaskType == TaskTypes.FixLights)
                {
                    isLightOff = true;
                    break;
                }
            }

            if (isLightOff)
            {
                this.KillCoolTime = this.KillCoolTime - this.killBonus;
            }
            else
            {
                this.KillCoolTime = this.KillCoolTime + this.killPenalty;
            }

            this.KillCoolTime = Mathf.Clamp(this.KillCoolTime, 0.1f, float.MaxValue);
            
            return true;
        }


        public void CreateAbility()
        {
            this.Button = new AssaultButton(
                UseAbility,
                IsAbilityUse,
                FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic.sprite);

            this.RoleAbilityInit();
        }

        public bool IsAbilityUse()
        {
            this.targetPlayer = Player.GetClosestPlayerInRange(
                CachedPlayerControl.LocalPlayer,
                this, this.range);

            return isVentIn() && this.targetPlayer != null;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            this.targetPlayer = null;
        }

        public bool UseAbility()
        {

            float prevTime = PlayerControl.LocalPlayer.killTimer;
            Helper.Logging.Debug($"PrevKillCool:{prevTime}");

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RPCOperator.Command.StartVentAnimation,
                Hazel.SendOption.Reliable, -1);
            writer.WritePacked(Vent.currentVent.Id);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCOperator.StartVentAnimation(
                Vent.currentVent.Id);


            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.UncheckedMurderPlayer,
                new List<byte>
                {
                    CachedPlayerControl.LocalPlayer.PlayerId,
                    this.targetPlayer.PlayerId,
                    byte.MinValue
                });
            RPCOperator.UncheckedMurderPlayer(
                CachedPlayerControl.LocalPlayer.PlayerId,
                this.targetPlayer.PlayerId,
                byte.MinValue);

            this.KillCoolTime = this.KillCoolTime - this.killBonus;
            this.KillCoolTime = Mathf.Clamp(this.KillCoolTime, 0.1f, float.MaxValue);

            this.targetPlayer = null;
            CachedPlayerControl.LocalPlayer.PlayerControl.SetKillTimer(prevTime);

            return true;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateFloatOption(
                SandWormOption.KillCoolPenalty,
                5.0f, 1.0f, 10.0f, 0.1f,
                parentOps, format: OptionUnit.Second);

            CreateFloatOption(
                SandWormOption.AssaultKillCoolReduce,
                3.0f, 1.0f, 5.0f, 0.1f,
                parentOps, format: OptionUnit.Second);

            CreateFloatOption(
                SandWormOption.AssaultRange,
                2.0f, 0.1f, 3.0f, 0.1f,
                parentOps);

            CreateFloatOption(
                RoleAbilityCommonOption.AbilityCoolTime,
                15.0f, 0.5f, 45.0f, 0.1f,
                parentOps, format: OptionUnit.Second);

        }

        protected override void RoleSpecificInit()
        {
            this.range = OptionHolder.AllOption[
                GetRoleOptionId(SandWormOption.AssaultRange)].GetValue();

            this.killPenalty = OptionHolder.AllOption[
                GetRoleOptionId(SandWormOption.KillCoolPenalty)].GetValue();
            this.killBonus = OptionHolder.AllOption[
                GetRoleOptionId(SandWormOption.AssaultKillCoolReduce)].GetValue();

            if (!this.HasOtherKillCool)
            {
                this.HasOtherKillCool = true;
                this.KillCoolTime = PlayerControl.GameOptions.KillCooldown;
            }

            this.RoleAbilityInit();
        }

        private static bool isVentIn()
        {
            bool result = CachedPlayerControl.LocalPlayer.PlayerControl.inVent;
            Vent vent = Vent.currentVent;

            if (!result || vent == null) { return false; }

            if (ExtremeRolesPlugin.GameDataStore.CustomVent.IsCustomVent(
                    vent.Id)) { return false; }

            return true;
        }
    }
}
