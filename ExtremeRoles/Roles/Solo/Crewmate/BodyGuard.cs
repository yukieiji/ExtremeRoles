using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class BodyGuard : SingleRoleBase, IRoleAbility
    {
        public enum BodyGuardOption
        {
            ShieldRange
        }

        public RoleAbilityButtonBase Button
        {
            get => this.shieldButton;
            set
            {
                this.shieldButton = value;
            }
        }

        public byte TargetPlayer = byte.MaxValue;

        private int shildNum;
        private float shieldRange;
        private RoleAbilityButtonBase shieldButton;

        public BodyGuard() : base(
            ExtremeRoleId.BodyGuard,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.BodyGuard.ToString(),
            ColorPalette.BodyGuardOrange,
            false, true, false, false)
        { }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer, PlayerControl killerPlayer)
        {
            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.BodyGuardResetShield,
                new List<byte>
                {
                    rolePlayer.PlayerId
                });
            RPCOperator.BodyGuardResetShield(
                rolePlayer.PlayerId);

            if (rolePlayer.PlayerId == killerPlayer.PlayerId) { return; }

            RPCOperator.Call(
                rolePlayer.NetId,
                RPCOperator.Command.ReplaceDeadReason,
                new List<byte>
                {
                    rolePlayer.PlayerId,
                    (byte)GameDataContainer.PlayerStatus.Martyrdom
                });
            ExtremeRolesPlugin.GameDataStore.ReplaceDeadReason(
                rolePlayer.PlayerId,
                GameDataContainer.PlayerStatus.Martyrdom);
        }

        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("shield"),
                Loader.CreateSpriteFromResources(
                    Path.BodyGuardShield));
            this.Button.SetLabelToCrewmate();
        }

        public bool UseAbility()
        {

            byte playerId = PlayerControl.LocalPlayer.PlayerId;

            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.BodyGuardFeatShield,
                new List<byte>
                {
                    playerId,
                    this.TargetPlayer 
                });
            RPCOperator.BodyGuardFeatShield(
                playerId, this.TargetPlayer);
            this.TargetPlayer = byte.MaxValue;

            return true;
        }

        public bool IsAbilityUse()
        {

            this.TargetPlayer = byte.MaxValue;

            PlayerControl target = Player.GetPlayerTarget(
                PlayerControl.LocalPlayer, this,
                this.shieldRange);

            if (target != null)
            {
                byte targetId = target.PlayerId;

                if (!ExtremeRolesPlugin.GameDataStore.ShildPlayer.IsShielding(
                        PlayerControl.LocalPlayer.PlayerId, targetId))
                {
                    this.TargetPlayer = targetId;
                }
            }

            return this.IsCommonUse() && this.TargetPlayer != byte.MaxValue;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.BodyGuardResetShield,
                new List<byte>
                {
                    PlayerControl.LocalPlayer.PlayerId
                });
            RPCOperator.BodyGuardResetShield(
                PlayerControl.LocalPlayer.PlayerId);

            if (this.shieldButton == null) { return; }

            ((AbilityCountButton)this.shieldButton).UpdateAbilityCount(
                this.shildNum);
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {

            CustomOption.Create(
               GetRoleOptionId((int)BodyGuardOption.ShieldRange),
               string.Concat(
                   this.RoleName,
                   BodyGuardOption.ShieldRange.ToString()),
               1.0f, 0.0f, 2.0f, 0.1f,
               parentOps);

            this.CreateAbilityCountOption(
                parentOps, 2, 5);
        }

        protected override void RoleSpecificInit()
        {
            this.shieldRange = OptionHolder.AllOption[
                GetRoleOptionId((int)BodyGuardOption.ShieldRange)].GetValue();

            this.RoleAbilityInit();
            if (this.Button != null)
            {
                this.shildNum = ((AbilityCountButton)this.shieldButton).CurAbilityNum;
            }
        }
    }
}
