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
            ShieldeRange
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
                    Path.TestButton, 115f));
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
            this.setTarget();
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
               GetRoleOptionId((int)BodyGuardOption.ShieldeRange),
               string.Concat(
                   this.RoleName,
                   BodyGuardOption.ShieldeRange.ToString()),
               1.0f, 0.0f, 2.0f, 0.1f,
               parentOps);

            this.CreateAbilityCountOption(
                parentOps, 5);
        }

        protected override void RoleSpecificInit()
        {
            this.shieldRange = OptionHolder.AllOption[
                GetRoleOptionId((int)BodyGuardOption.ShieldeRange)].GetValue();

            this.RoleAbilityInit();
            if (this.Button != null)
            {
                this.shildNum = ((AbilityCountButton)this.shieldButton).CurAbilityNum;
            }
        }

        private void setTarget()
        {
            PlayerControl result = null;
            float num = this.shieldRange;
            this.TargetPlayer = byte.MaxValue;

            if (!ShipStatus.Instance)
            {
                return;
            }

            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();

            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];

                if (!playerInfo.Disconnected &&
                    playerInfo.PlayerId != PlayerControl.LocalPlayer.PlayerId &&
                    !playerInfo.IsDead &&
                    !playerInfo.Object.inVent)
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object)
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num &&
                            !PhysicsHelpers.AnyNonTriggersBetween(
                                truePosition, vector.normalized,
                                magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }

            if (result)
            {
                if (this.IsSameTeam(ExtremeRoleManager.GameRole[result.PlayerId]))
                {
                    result = null;
                }
            }
            if (result != null)
            {
                byte target = result.PlayerId;

                if (!ExtremeRolesPlugin.GameDataStore.ShildPlayer.IsShielding(
                        PlayerControl.LocalPlayer.PlayerId, target))
                {
                    this.TargetPlayer = result.PlayerId;
                    Player.SetPlayerOutLine(result, this.NameColor);
                }
            }
        }

    }
}
