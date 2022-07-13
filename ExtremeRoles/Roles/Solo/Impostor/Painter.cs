using System.Collections.Generic;

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
    public sealed class Painter : SingleRoleBase, IRoleAbility
    {

        public enum PainterOption
        {
            PaintColorIsRandom,
            CanPaintDistance,
        }

        public bool PaintColorIsRandom;
        private float paintDistance;
        private byte targetDeadBodyId;

        public RoleAbilityButtonBase Button
        {
            get => this.paintButton;
            set
            {
                this.paintButton = value;
            }
        }

        private RoleAbilityButtonBase paintButton;

        public Painter() : base(
            ExtremeRoleId.Painter,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Painter.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public static void PaintDeadBody(
            byte rolePlayerId, byte targetPlayerId)
        {
            var role = ExtremeRoleManager.GetSafeCastedRole<Painter>(rolePlayerId);
            if (role == null) { return; }

            DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; ++i)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == targetPlayerId)
                {

                    Color oldColor = array[i].bodyRenderer.color;

                    if (role.PaintColorIsRandom)
                    {
                        Color newColor = new Color(
                            Random.value,
                            Random.value,
                            Random.value,
                            oldColor.a);

                        array[i].bodyRenderer.material.SetColor("_BackColor", newColor);
                        array[i].bodyRenderer.material.SetColor("_BodyColor", newColor);
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
                   Path.PainterPaint));
        }

        public bool IsAbilityUse()
        {
            this.targetDeadBodyId = byte.MaxValue;
            GameData.PlayerInfo info = Player.GetDeadBodyInfo(
                this.paintDistance);
            
            if (info != null)
            {
                this.targetDeadBodyId = info.PlayerId;
            }

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
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.PainterPaintBody,
                new List<byte>
                {
                    CachedPlayerControl.LocalPlayer.PlayerId,
                    this.targetDeadBodyId
                });

            PaintDeadBody(
                CachedPlayerControl.LocalPlayer.PlayerId,
                this.targetDeadBodyId);
            return true;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            this.CreateCommonAbilityOption(
                parentOps);

            CreateFloatOption(
                PainterOption.CanPaintDistance,
                1.0f, 1.0f, 5.0f, 0.5f,
                parentOps);

            CreateBoolOption(
                PainterOption.PaintColorIsRandom,
                false, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.paintDistance = OptionHolder.AllOption[
                GetRoleOptionId(PainterOption.CanPaintDistance)].GetValue();
            this.PaintColorIsRandom = OptionHolder.AllOption[
                GetRoleOptionId(PainterOption.PaintColorIsRandom)].GetValue();
            this.RoleAbilityInit();
        }
    }
}
