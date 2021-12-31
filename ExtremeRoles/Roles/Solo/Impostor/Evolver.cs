using UnityEngine;

using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public class Evolver : SingleRoleBase, IRoleAbility
    {
        public enum EvolverOption
        {
            IsEvolvdAnimation,
            IsEatingEndCleanBody,
            KillCoolReduceRate,
        }


        public GameData.PlayerInfo targetBody;
        public byte eatingBodyId;

        private float reduceRate;
        private bool isEvolvdAnimation;
        private bool isEatingEndCleanBody;
        public Evolver() : base(
            ExtremeRoleId.Evolver,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Evolver.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        {
            this.isEatingEndCleanBody = false;
        }

        public RoleAbilityButton Button
        {
            get => this.evolveButton;
            set
            {
                this.evolveButton = value;
            }
        }
        private RoleAbilityButton evolveButton;

        public void CreateAbility()
        {
            this.CreateAbilityButton(
                Translation.GetString("Evoled"),
                Helper.Resources.LoadSpriteFromResources(
                    Resources.ResourcesPaths.TestButton, 115f),
                checkAbility: CheckAbility,
                abilityCleanUp: CleanUp,
                abilityNum: OptionsHolder.VanillaMaxPlayerNum - 1);
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

            this.KillCoolTime = this.KillCoolTime * this.reduceRate;

            if (!this.isEatingEndCleanBody) { return; }

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RPCOperator.Command.CleanDeadBody,
                Hazel.SendOption.Reliable, -1);
            writer.Write(this.eatingBodyId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCOperator.CleanDeadBody(this.eatingBodyId);
        }

        public bool CheckAbility()
        {
            setTargetDeadBody();
            return this.eatingBodyId == this.targetBody.PlayerId;
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
                GetRoleOptionId((int)EvolverOption.IsEvolvdAnimation),
                Design.ConcatString(
                    this.RoleName,
                    EvolverOption.IsEvolvdAnimation.ToString()),
                true, parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)EvolverOption.IsEatingEndCleanBody),
                Design.ConcatString(
                    this.RoleName,
                    EvolverOption.IsEatingEndCleanBody.ToString()),
                true, parentOps);
            CustomOption.Create(
                GetRoleOptionId((int)EvolverOption.KillCoolReduceRate),
                Design.ConcatString(
                    this.RoleName,
                    EvolverOption.KillCoolReduceRate.ToString()),
                5, 1, 50, 1,
                parentOps,
                format: "unitPercentage");

            this.CreateRoleAbilityOption(
                parentOps, true, 10);
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();

            if(!this.HasOtherKillCool)
            {
                this.HasOtherKillCool = true;
                this.KillCoolTime = PlayerControl.GameOptions.KillCooldown;
            }

            var allOption = OptionsHolder.AllOption;

            this.isEvolvdAnimation = allOption[
                GetRoleOptionId((int)EvolverOption.IsEvolvdAnimation)].GetValue();
            this.isEatingEndCleanBody = allOption[
                GetRoleOptionId((int)EvolverOption.IsEatingEndCleanBody)].GetValue();
            this.reduceRate = (100 - allOption[
                GetRoleOptionId((int)EvolverOption.KillCoolReduceRate)].GetValue()) / 100;

        }

        private void setTargetDeadBody()
        {
            this.targetBody = null;

            foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(
                PlayerControl.LocalPlayer.GetTruePosition(),
                PlayerControl.LocalPlayer.MaxReportDistance,
                Constants.PlayersOnlyMask))
            {
                if (collider2D.tag == "DeadBody")
                {
                    DeadBody component = collider2D.GetComponent<DeadBody>();

                    if (component && !component.Reported)
                    {
                        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                        Vector2 truePosition2 = component.TruePosition;
                        if ((Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance) &&
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
    }
}
