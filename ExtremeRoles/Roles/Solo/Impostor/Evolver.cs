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
        public Evolver() : base(
            ExtremeRoleId.Evolver,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Evolver.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        {
            this.isEatingEndCleanBody = false;
        }

        public RoleAbilityButtonBase Button
        {
            get => this.evolveButton;
            set
            {
                this.evolveButton = value;
            }
        }
        private RoleAbilityButtonBase evolveButton;

        public void CreateAbility()
        {
            this.defaultButtonText = Translation.GetString("evolve");

            this.CreateAbilityCountButton(
                this.defaultButtonText,
                Loader.CreateSpriteFromResources(
                    Path.EvolverEvolved, 115f),
                checkAbility: CheckAbility,
                abilityCleanUp: CleanUp);
        }

        public bool IsAbilityUse()
        {
            setTargetDeadBody();
            return this.IsCommonUse() && this.targetBody != null;
        }

        public void CleanUp()
        {
            
            if (this.isEvolvdAnimation)
            {
                PlayerControl.LocalPlayer.RpcShapeshift(
                    PlayerControl.LocalPlayer, true);
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
            setTargetDeadBody();

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
                format: "unitPercentage");

            CustomOption.Create(
                GetRoleOptionId((int)EvolverOption.KillCoolResuceRateMulti),
                string.Concat(
                    this.RoleName,
                    EvolverOption.KillCoolResuceRateMulti.ToString()),
                1.0f, 1.0f, 5.0f, 0.1f,
                parentOps, format: "unitMultiplier");

            this.CreateAbilityCountOption(
                parentOps, 10, true);
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

        private void setTargetDeadBody()
        {
            this.targetBody = null;

            foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(
                PlayerControl.LocalPlayer.GetTruePosition(),
                this.eatingRange,
                Constants.PlayersOnlyMask))
            {
                if (collider2D.tag == "DeadBody")
                {
                    DeadBody component = collider2D.GetComponent<DeadBody>();

                    if (component && !component.Reported)
                    {
                        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                        Vector2 truePosition2 = component.TruePosition;
                        if ((Vector2.Distance(truePosition2, truePosition) <= this.eatingRange) &&
                            (PlayerControl.LocalPlayer.CanMove) &&
                            (!PhysicsHelpers.AnythingBetween(
                                truePosition, truePosition2, Constants.ShipAndObjectsMask, false)))
                        {
                            this.targetBody = GameData.Instance.GetPlayerById(component.ParentId);
                            break;
                        }
                    }
                }
            }
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
