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
using ExtremeRoles.Extension.Ship;
using AmongUs.GameOptions;

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
                    null, null,
                    KeyCode.F)
            { }

            protected override bool IsEnable() => this.CanUse.Invoke();

            protected override void DoClick()
            {
                if (this.IsEnable() &&
                    this.Timer <= 0f &&
                    this.IsAbilityReady() &&
                    this.UseAbility.Invoke())
                {
                    this.SetStatus(
                        this.HasCleanUp() ?
                        AbilityState.Activating :
                        AbilityState.CoolDown);
                }
            }

            protected override void UpdateAbility()
            {
                if (this.Timer > 0.0f)
                {
                    this.SetStatus(
                        isVentIn() || isLightOff() ? 
                        AbilityState.CoolDown : AbilityState.Stop);
                }
            }
            private static bool isLightOff()
            {
                foreach (PlayerTask task in
                    CachedPlayerControl.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
                {
                    if (task.TaskType == TaskTypes.FixLights)
                    {
                        return true;
                    }
                }
                return false;
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

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.StartVentAnimation))
            {
                caller.WritePackedInt(Vent.currentVent.Id);
            }

            RPCOperator.StartVentAnimation(
                Vent.currentVent.Id);

            Player.RpcUncheckMurderPlayer(
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
                this.KillCoolTime = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
                    FloatOptionNames.KillCooldown);
            }

            this.RoleAbilityInit();
        }

        private static bool isVentIn()
        {
            bool result = CachedPlayerControl.LocalPlayer.PlayerControl.inVent;
            Vent vent = Vent.currentVent;

            if (!result || vent == null) { return false; }

            if (CachedShipStatus.Instance.IsCustomVent(vent.Id)) { return false; }

            return true;
        }
    }
}
