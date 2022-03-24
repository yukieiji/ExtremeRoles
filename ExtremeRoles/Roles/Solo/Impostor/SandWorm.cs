using System;
using System.Collections.Generic;

using UnityEngine;
using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public class SandWorm : SingleRoleBase, IRoleAbility
    {
        public class AssaultButton : RoleAbilityButtonBase
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
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
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
            IsAssultPlaySound,
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
        private bool isAssultPlaySound = false;

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
            this.KillCoolTime = this.KillCoolTime + this.killPenalty;
            return true;
        }


        public void CreateAbility()
        {
            this.Button = new AssaultButton(
                UseAbility,
                IsAbilityUse,
                Loader.CreateSpriteFromResources(
                    Path.TestButton));

            this.RoleAbilityInit();
        }

        public bool IsAbilityUse()
        {
            this.targetPlayer = Player.GetPlayerTarget(
                PlayerControl.LocalPlayer,
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
            writer.Write(this.isAssultPlaySound);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCOperator.StartVentAnimation(
                Vent.currentVent.Id, this.isAssultPlaySound);


            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.UncheckedMurderPlayer,
                new List<byte>
                { 
                    PlayerControl.LocalPlayer.PlayerId,
                    this.targetPlayer.PlayerId,
                    byte.MinValue
                });
            RPCOperator.UncheckedMurderPlayer(
                PlayerControl.LocalPlayer.PlayerId,
                this.targetPlayer.PlayerId,
                byte.MinValue);

            this.KillCoolTime = this.KillCoolTime - this.killBonus;
            this.targetPlayer = null;
            PlayerControl.LocalPlayer.SetKillTimer(prevTime);

            return true;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId((int)SandWormOption.KillCoolPenalty),
                string.Concat(
                    this.RoleName,
                    SandWormOption.KillCoolPenalty.ToString()),
                5.0f, 1.0f, 10.0f, 0.1f,
                parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)SandWormOption.AssaultKillCoolReduce),
                string.Concat(
                    this.RoleName,
                    SandWormOption.AssaultKillCoolReduce.ToString()),
                3.0f, 1.0f, 5.0f, 0.1f,
                parentOps);


            CustomOption.Create(
                GetRoleOptionId((int)SandWormOption.AssaultRange),
                string.Concat(
                    this.RoleName,
                    SandWormOption.AssaultRange.ToString()),
                2.0f, 0.1f, 3.0f, 0.1f,
                parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)SandWormOption.IsAssultPlaySound),
                string.Concat(
                    this.RoleName,
                    SandWormOption.IsAssultPlaySound.ToString()),
                true, parentOps);

            CustomOption.Create(
                this.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime),
                string.Concat(
                    this.RoleName,
                    RoleAbilityCommonOption.AbilityCoolTime.ToString()),
                15.0f, 0.5f, 45.0f, 0.1f,
                parentOps, format: OptionUnit.Second);

        }

        protected override void RoleSpecificInit()
        {
            this.range = OptionHolder.AllOption[
                GetRoleOptionId((int)SandWormOption.AssaultRange)].GetValue();
            this.isAssultPlaySound = OptionHolder.AllOption[
                GetRoleOptionId((int)SandWormOption.IsAssultPlaySound)].GetValue();

            this.killPenalty = OptionHolder.AllOption[
                GetRoleOptionId((int)SandWormOption.KillCoolPenalty)].GetValue();
            this.killBonus = OptionHolder.AllOption[
                GetRoleOptionId((int)SandWormOption.AssaultKillCoolReduce)].GetValue();

            if (!this.HasOtherKillCool)
            {
                this.HasOtherKillCool = true;
                this.KillCoolTime = PlayerControl.GameOptions.KillCooldown;
            }

            this.RoleAbilityInit();
        }

        private static bool isVentIn()
        {
            bool result = PlayerControl.LocalPlayer.inVent;
            Vent vent = Vent.currentVent;

            if (!result || vent == null) { return false; }

            if (ExtremeRolesPlugin.GameDataStore.CustomVent.IsCustomVent(
                    vent.Id)) { return false; }

            return true;
        }
    }
}
