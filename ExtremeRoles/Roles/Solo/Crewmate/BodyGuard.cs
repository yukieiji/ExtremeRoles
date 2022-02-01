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

        private RoleAbilityButtonBase shieldButton;
        private float shieldRange;

        public BodyGuard() : base(
            ExtremeRoleId.BodyGuard,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.BodyGuard.ToString(),
            ColorPalette.BodyGuardOrange,
            false, true, false, false)
        { }

        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("shield"),
                Loader.CreateSpriteFromResources(
                    Path.MaintainerRepair, 115f));
            this.Button.SetLabelToCrewmate();
        }

        public bool UseAbility()
        {
            return true;
        }

        public bool IsAbilityUse()
        {
            this.setTarget();
            return this.IsCommonUse() && this.TargetPlayer != byte.MaxValue;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {

            this.shieldRange = OptionHolder.AllOption[
                GetRoleOptionId((int)BodyGuardOption.ShieldeRange)].GetValue();

            this.CreateAbilityCountOption(
                parentOps, 30);
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();
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
                this.TargetPlayer = result.PlayerId;
                Helper.Player.SetPlayerOutLine(result, this.NameColor);
            }
        }

    }
}
