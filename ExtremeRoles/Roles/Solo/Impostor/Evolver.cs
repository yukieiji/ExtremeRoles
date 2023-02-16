using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class Evolver : SingleRoleBase, IRoleAbility
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

            PlayerControl rolePlayer = CachedPlayerControl.LocalPlayer;

            if (this.isEvolvdAnimation)
            {
                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.UncheckedShapeShift))
                {
                    caller.WriteByte(rolePlayer.PlayerId);
                    caller.WriteByte(rolePlayer.PlayerId);
                    caller.WriteByte(byte.MaxValue);
                }
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

            this.Button.SetButtonText(this.defaultButtonText);

            if (!this.isEatingEndCleanBody) { return; }

            Player.RpcCleanDeadBody(this.eatingBodyId);
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
            
            this.Button.SetButtonText(
                result ? this.eatingText : this.defaultButtonText);

            return result;
        }

        public bool UseAbility()
        {
            this.eatingBodyId = this.targetBody.PlayerId;
            return true;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateBoolOption(
                EvolverOption.IsEvolvedAnimation,
                true, parentOps);

            CreateBoolOption(
                EvolverOption.IsEatingEndCleanBody,
                true, parentOps);

            CreateFloatOption(
                EvolverOption.EatingRange,
                2.5f, 0.5f, 5.0f, 0.5f,
                parentOps);

            CreateIntOption(
                EvolverOption.KillCoolReduceRate,
                10, 1, 50, 1, parentOps,
                format: OptionUnit.Percentage);

            CreateFloatOption(
                EvolverOption.KillCoolResuceRateMulti,
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
                this.KillCoolTime = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
                    FloatOptionNames.KillCooldown);
            }

            this.defaultKillCoolTime = this.KillCoolTime;
            
            var allOption = OptionHolder.AllOption;

            this.isEvolvdAnimation = allOption[
                GetRoleOptionId(EvolverOption.IsEvolvedAnimation)].GetValue();
            this.isEatingEndCleanBody = allOption[
                GetRoleOptionId(EvolverOption.IsEatingEndCleanBody)].GetValue();
            this.eatingRange = allOption[
                GetRoleOptionId(EvolverOption.EatingRange)].GetValue();
            this.reduceRate = allOption[
                GetRoleOptionId(EvolverOption.KillCoolReduceRate)].GetValue();
            this.reruceMulti = (float)allOption[
                GetRoleOptionId(EvolverOption.KillCoolResuceRateMulti)].GetValue();

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
