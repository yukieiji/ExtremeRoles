using System.Collections.Generic;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class Smasher : SingleRoleBase, IRoleAbility
    {
        public enum SmasherOption
        {
            SmashPenaltyKillCool,
        }

        public RoleAbilityButtonBase Button
        {
            get => this.smashButton;
            set
            {
                this.smashButton = value;
            }
        }

        private RoleAbilityButtonBase smashButton;
        private byte targetPlayerId;
        private float prevKillCool;
        private float penaltyKillCool;

        public Smasher() : base(
            ExtremeRoleId.Smasher,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Smasher.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("smash"),
                FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic.sprite);
        }

        public bool IsAbilityUse()
        {
            this.targetPlayerId = byte.MaxValue;
            var player = Player.GetClosestPlayerInKillRange();
            if (player != null)
            {
                this.targetPlayerId = player.PlayerId;
            }
            return this.IsCommonUse() && this.targetPlayerId != byte.MaxValue;
        }

        public bool UseAbility()
        {
            PlayerControl killer = CachedPlayerControl.LocalPlayer;
            if (killer.Data.IsDead || !killer.CanMove) { return false; }

            var role = ExtremeRoleManager.GetLocalPlayerRole();
            var targetPlayerRole = ExtremeRoleManager.GameRole[this.targetPlayerId];
            var prevTarget = Player.GetPlayerControlById(this.targetPlayerId);

            bool canKill = role.TryRolePlayerKillTo(
                killer, prevTarget);
            if (!canKill) { return false; }

            canKill = targetPlayerRole.TryRolePlayerKilledFrom(
                prevTarget, killer);
            if (!canKill) { return false; }

            var multiAssignRole = role as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    canKill = multiAssignRole.AnotherRole.TryRolePlayerKillTo(
                        killer, prevTarget);
                    if (!canKill) { return false; }
                }
            }

            multiAssignRole = targetPlayerRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    canKill = multiAssignRole.AnotherRole.TryRolePlayerKilledFrom(
                        prevTarget, killer);
                    if (!canKill) { return false; }
                }
            }


            var bodyGuard = ExtremeRolesPlugin.ShipState.ShildPlayer.GetBodyGuardPlayerId(
                prevTarget.PlayerId);

            PlayerControl newTarget = prevTarget;

            if (bodyGuard != byte.MaxValue)
            {
                newTarget = Player.GetPlayerControlById(bodyGuard);
                if (newTarget == null)
                {
                    newTarget = prevTarget;
                }
                else if (newTarget.Data.IsDead || newTarget.Data.Disconnected)
                {
                    newTarget = prevTarget;
                }
            }

            byte useAnimation = byte.MaxValue;

            if (newTarget.PlayerId != prevTarget.PlayerId)
            {
                useAnimation = byte.MinValue;
            }

            this.prevKillCool = PlayerControl.LocalPlayer.killTimer;

            RPCOperator.Call(
                killer.NetId,
                RPCOperator.Command.UncheckedMurderPlayer,
                new List<byte> { killer.PlayerId, newTarget.PlayerId, useAnimation });
            RPCOperator.UncheckedMurderPlayer(
                killer.PlayerId,
                newTarget.PlayerId,
                useAnimation);

            if (this.penaltyKillCool > 0.0f)
            {
                if (!this.HasOtherKillCool)
                {
                    this.HasOtherKillCool = true;
                    this.KillCoolTime = PlayerControl.GameOptions.KillCooldown;
                }
                this.KillCoolTime = this.KillCoolTime + this.penaltyKillCool;
            }

            killer.killTimer = this.prevKillCool;

            return true;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            this.CreateAbilityCountOption(
                parentOps, 1, 14);

            CreateFloatOption(
                SmasherOption.SmashPenaltyKillCool,
                4.0f, 0.0f, 30f, 0.5f, parentOps,
                format: OptionUnit.Second);

        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();
            this.penaltyKillCool = OptionHolder.AllOption[
                GetRoleOptionId(SmasherOption.SmashPenaltyKillCool)].GetValue();
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
