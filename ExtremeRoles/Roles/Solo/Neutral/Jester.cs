using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Jester : SingleRoleBase, IRoleAbility
    {

        public enum JesterOption
        {
            OutburstDistance,
        }

        public RoleAbilityButtonBase Button
        { 
            get => this.outburstButton;
            set
            {
                this.outburstButton = value;
            }
        }

        private float distance;
        private PlayerControl target;
        private PlayerControl outburstTarget;
        private RoleAbilityButtonBase outburstButton;

        public Jester(): base(
            ExtremeRoleId.Jester,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Jester.ToString(),
            ColorPalette.JesterPink,
            false, false, false, true)
        {}

        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Helper.Translation.GetString("outburst"),
                Loader.CreateSpriteFromResources(
                    Path.JesterOutburst),
                abilityCleanUp:CleanUp);
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

        public bool IsAbilityUse()
        {
            this.target = Helper.Player.GetPlayerTarget(
                PlayerControl.LocalPlayer, this,
                this.distance);
            return this.IsCommonUse() && this.target != null;
        }

        public override void ExiledAction(GameData.PlayerInfo rolePlayer)
        {
            this.IsWin = true;
        }

        public bool UseAbility()
        {
            this.outburstTarget = this.target;
            return true;
        }
        public void CleanUp()
        {
            if (this.outburstTarget == null) { return; }
            if (this.outburstTarget.Data.IsDead || this.outburstTarget.Data.Disconnected) { return; }
            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            var role = ExtremeRoleManager.GameRole[this.outburstTarget.PlayerId];
            if (!role.CanKill) { return; }

            PlayerControl killTarget = this.outburstTarget.FindClosestTarget(
                !role.IsImpostor());

            if (killTarget == null) { return; }
            if (killTarget.Data.IsDead || killTarget.Data.Disconnected) { return; }
            if (killTarget.PlayerId == PlayerControl.LocalPlayer.PlayerId) { return; }

            var killTargetPlayerRole = ExtremeRoleManager.GameRole[killTarget.PlayerId];

            bool canKill = role.TryRolePlayerKillTo(
                this.outburstTarget, killTarget);
            if (!canKill) { return; }

            canKill = killTargetPlayerRole.TryRolePlayerKilledFrom(
                killTarget, this.outburstTarget);
            if (!canKill) { return; }

            var bodyGuard = ExtremeRolesPlugin.GameDataStore.ShildPlayer.GetBodyGuardPlayerId(
                killTarget.PlayerId);

            PlayerControl prevTarget = killTarget;

            if (bodyGuard != byte.MaxValue)
            {
                killTarget = Helper.Player.GetPlayerControlById(bodyGuard);
                if (killTarget == null)
                {
                    killTarget = prevTarget;
                }
                else if (killTarget.Data.IsDead || killTarget.Data.Disconnected)
                {
                    killTarget = prevTarget;
                }
            }

            byte animate = byte.MaxValue;

            if (killTarget.PlayerId != prevTarget.PlayerId)
            {
                animate = 0;
            }

            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.UncheckedMurderPlayer,
                new List<byte> { this.outburstTarget.PlayerId, killTarget.PlayerId, animate });
            RPCOperator.UncheckedMurderPlayer(
                this.outburstTarget.PlayerId,
                killTarget.PlayerId,
                animate);
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId((int)JesterOption.OutburstDistance),
                string.Concat(
                    this.RoleName,
                    JesterOption.OutburstDistance.ToString()),
                1.0f, 0.0f, 2.0f, 0.1f,
                parentOps);

            this.CreateAbilityCountOption(
                parentOps, 100, 10.0f);
        }

        protected override void RoleSpecificInit()
        {
            this.distance = OptionHolder.AllOption[
                GetRoleOptionId((int)JesterOption.OutburstDistance)].GetValue();
            this.RoleAbilityInit();
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
