using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public class Evolver : SingleRoleBase, IRoleAbility
    {
        public enum EvolverOption
        {
            IsEvolvedAnimation,
            IsEatingEndCleanBody,
            EatingRange,
            KillCoolReduceRate,
            KillCoolResuceRateMulti,
        }


        public GameData.PlayerInfo targetBody;
        public byte eatingBodyId;

        private float eatingRange = 1.0f;
        private float reduceRate = 1.0f;
        private float reruceMulti = 1.0f;
        private bool isEvolvdAnimation;
        private bool isEatingEndCleanBody;

        private string defaultButtonText;
        private string eatingText;

        private float defaultKillCoolTime;

        public RoleAbilityButtonBase Button
        {
            get => this.evolveButton;
            set
            {
                this.evolveButton = value;
            }
        }
        private RoleAbilityButtonBase evolveButton;

        public Evolver() : base(
            ExtremeRoleId.Evolver,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Evolver.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        {
            this.isEatingEndCleanBody = false;
        }

        public void CreateAbility()
        {
            this.defaultButtonText = Translation.GetString("evolve");

            this.CreateAbilityCountButton(
                this.defaultButtonText,
                Loader.CreateSpriteFromResources(
                    Path.EvolverEvolved),
                checkAbility: CheckAbility,
                abilityCleanUp: CleanUp);
        }

        public bool IsAbilityUse()
        {
            this.targetBody = Player.GetDeadBodyInfo(
                this.eatingRange);
            return this.IsCommonUse() && this.targetBody != null;
        }

        public void CleanUp()
        {
            
            if (this.isEvolvdAnimation)
            {
                var rolePlayer = PlayerControl.LocalPlayer;

                RPCOperator.Call(
                    PlayerControl.LocalPlayer.NetId,
                    RPCOperator.Command.UncheckedShapeShift,
                    new List<byte> { rolePlayer.PlayerId, rolePlayer.PlayerId, byte.MaxValue });
                RPCOperator.UncheckedShapeShift(
                    rolePlayer.PlayerId,
                    rolePlayer.PlayerId,
                    byte.MaxValue);
            }

            this.KillCoolTime = this.KillCoolTime * ((100f - this.reduceRate) / 100f);
            this.reduceRate = this.reduceRate * this.reruceMulti;
            
            this.CanKill = true;
            this.KillCoolTime = Mathf.Clamp(
                this.KillCoolTime, 0.1f, this.defaultKillCoolTime);

            this.Button.ButtonText = this.defaultButtonText;

            if (!this.isEatingEndCleanBody) { return; }

            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.CleanDeadBody,
                new List<byte> { this.eatingBodyId });

            RPCOperator.CleanDeadBody(this.eatingBodyId);
        }

        public bool CheckAbility()
        {
            this.targetBody = Player.GetDeadBodyInfo(
                this.eatingRange);

            bool result;

            if (this.targetBody == null)
            {
                result = false;
            }
            else
            {
                result = this.eatingBodyId == this.targetBody.PlayerId;
            }
            
            this.Button.ButtonText = result ? this.eatingText : this.defaultButtonText;

            return result;
        }

        public bool UseAbility()
        {
            this.eatingBodyId = this.targetBody.PlayerId;
            return true;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId((int)EvolverOption.IsEvolvedAnimation),
                string.Concat(
                    this.RoleName,
                    EvolverOption.IsEvolvedAnimation.ToString()),
                true, parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)EvolverOption.IsEatingEndCleanBody),
                string.Concat(
                    this.RoleName,
                    EvolverOption.IsEatingEndCleanBody.ToString()),
                true, parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)EvolverOption.EatingRange),
                string.Concat(
                    this.RoleName,
                    EvolverOption.EatingRange.ToString()),
                2.5f, 0.5f, 5.0f, 0.5f,
                parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)EvolverOption.KillCoolReduceRate),
                string.Concat(
                    this.RoleName,
                    EvolverOption.KillCoolReduceRate.ToString()),
                10, 1, 50, 1, parentOps,
                format: OptionUnit.Percentage);

            CustomOption.Create(
                GetRoleOptionId((int)EvolverOption.KillCoolResuceRateMulti),
                string.Concat(
                    this.RoleName,
                    EvolverOption.KillCoolResuceRateMulti.ToString()),
                1.0f, 1.0f, 5.0f, 0.1f,
                parentOps, format: OptionUnit.Multiplier);

            this.CreateAbilityCountOption(
                parentOps, 5, 10, 5.0f);
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();

            if(!this.HasOtherKillCool)
            {
                this.HasOtherKillCool = true;
                this.KillCoolTime = PlayerControl.GameOptions.KillCooldown;
            }

            this.defaultKillCoolTime = this.KillCoolTime;
            
            var allOption = OptionHolder.AllOption;

            this.isEvolvdAnimation = allOption[
                GetRoleOptionId((int)EvolverOption.IsEvolvedAnimation)].GetValue();
            this.isEatingEndCleanBody = allOption[
                GetRoleOptionId((int)EvolverOption.IsEatingEndCleanBody)].GetValue();
            this.eatingRange = allOption[
                GetRoleOptionId((int)EvolverOption.EatingRange)].GetValue();
            this.reduceRate = allOption[
                GetRoleOptionId((int)EvolverOption.KillCoolReduceRate)].GetValue();
            this.reruceMulti = (float)allOption[
                GetRoleOptionId((int)EvolverOption.KillCoolResuceRateMulti)].GetValue();

            this.eatingText = Translation.GetString("eating");

        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }
    }
}
