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
    public class Painter : SingleRoleBase, IRoleAbility
    {
        private byte targetDeadBodyId;

        public enum PainterOption
        {
            PaintColorIsRandom,
            CanPaintDistance,
        }

        public bool PaintColorIsRandom;
        private float paintDistance;

        public Painter() : base(
            ExtremeRoleId.Painter,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Painter.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public RoleAbilityButtonBase Button
        { 
            get => this.carryButton;
            set
            {
                this.carryButton = value;
            }
        }

        private RoleAbilityButtonBase carryButton;

        public static void PaintDeadBody(
            byte rolePlayerId, byte targetPlayerId)
        {
            var role = (Painter)ExtremeRoleManager.GameRole[rolePlayerId];

            DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; ++i)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == targetPlayerId)
                {

                    Color oldColor = array[i].bodyRenderer.color;

                    if (role.PaintColorIsRandom)
                    {
                        array[i].bodyRenderer.color = new Color(
                            Random.value,
                            Random.value,
                            Random.value,
                            oldColor.a);
                    }
                    else
                    {
                        array[i].bodyRenderer.color = new Color(
                            oldColor.r, oldColor.g, oldColor.b, 0);
                    }
                    break;
                }
            }
        }
        
        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Translation.GetString("paint"),
                Loader.CreateSpriteFromResources(
                   Path.CarrierCarry, 115f));
        }

        public bool IsAbilityUse()
        {
            setTargetDeadBody();
            return this.IsCommonUse() && this.targetDeadBodyId != byte.MaxValue;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public bool UseAbility()
        {

            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.PainterPaintBody,
                new List<byte>
                { 
                    PlayerControl.LocalPlayer.PlayerId,
                    this.targetDeadBodyId
                });

            PaintDeadBody(
                PlayerControl.LocalPlayer.PlayerId,
                this.targetDeadBodyId);
            return true;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            this.CreateCommonAbilityOption(
                parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)PainterOption.CanPaintDistance),
                string.Concat(
                    this.RoleName,
                    PainterOption.CanPaintDistance.ToString()),
                1.0f, 1.0f, 5.0f, 0.5f,
                parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)PainterOption.PaintColorIsRandom),
                string.Concat(
                    this.RoleName,
                    PainterOption.PaintColorIsRandom.ToString()),
                false, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.paintDistance = OptionHolder.AllOption[
                GetRoleOptionId((int)PainterOption.CanPaintDistance)].GetValue();
            this.PaintColorIsRandom = OptionHolder.AllOption[
                GetRoleOptionId((int)PainterOption.PaintColorIsRandom)].GetValue();
            this.RoleAbilityInit();
        }

        private void setTargetDeadBody()
        {
            this.targetDeadBodyId = byte.MaxValue;

            foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(
                PlayerControl.LocalPlayer.GetTruePosition(),
                this.paintDistance,
                Constants.PlayersOnlyMask))
            {
                if (collider2D.tag == "DeadBody")
                {
                    DeadBody component = collider2D.GetComponent<DeadBody>();

                    if (component && !component.Reported)
                    {
                        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                        Vector2 truePosition2 = component.TruePosition;
                        if ((Vector2.Distance(truePosition2, truePosition) <= this.paintDistance) &&
                            (PlayerControl.LocalPlayer.CanMove) &&
                            (!PhysicsHelpers.AnythingBetween(
                                truePosition, truePosition2, Constants.ShipAndObjectsMask, false)))
                        {
                            this.targetDeadBodyId = GameData.Instance.GetPlayerById(component.ParentId).PlayerId;
                            break;
                        }
                    }
                }
            }
        }
    }
}
