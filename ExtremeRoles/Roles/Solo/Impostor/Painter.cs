using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class Painter : SingleRoleBase, IRoleAbility
    {
        public enum PainterOption
        {
            CanPaintDistance,
        }
        private float paintDistance;
        private byte targetDeadBodyId;

        public ExtremeAbilityButton Button
        {
            get => this.paintButton;
            set
            {
                this.paintButton = value;
            }
        }

        private ExtremeAbilityButton paintButton;

        private Sprite randomColorPaintImage;
        private Sprite transColorPaintImage;

        public Painter() : base(
            ExtremeRoleId.Painter,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Painter.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public static void PaintDeadBody(
            byte targetPlayerId, byte isRandomModeMessage)
        {
            bool isRandomColorMode = isRandomModeMessage == byte.MaxValue;

            DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; ++i)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == targetPlayerId)
                {

                    Color oldColor = array[i].bodyRenderer.color;

                    if (isRandomColorMode)
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

            this.randomColorPaintImage = Loader.CreateSpriteFromResources(
                Path.PainterPaintRandom);
            this.transColorPaintImage = Loader.CreateSpriteFromResources(
                Path.PainterPaintTrans);

            this.CreateNormalAbilityButton(
                "paint", this.randomColorPaintImage);
        }

        public bool IsAbilityUse()
        {
            this.targetDeadBodyId = byte.MaxValue;
            GameData.PlayerInfo info = Player.GetDeadBodyInfo(
                this.paintDistance);

            this.Button.Behavior.SetButtonImage(
                Input.GetKey(KeyCode.LeftShift) ?
                this.randomColorPaintImage : this.transColorPaintImage);

            if (info != null)
            {
                this.targetDeadBodyId = info.PlayerId;
            }

            return this.IsCommonUse() && this.targetDeadBodyId != byte.MaxValue;
        }

        public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
        {
            return;
        }

        public void ResetOnMeetingStart()
        {
            return;
        }

        public bool UseAbility()
        {
            byte message = Input.GetKey(KeyCode.LeftShift) ? byte.MaxValue : byte.MinValue;

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.PainterPaintBody))
            {
                caller.WriteByte(this.targetDeadBodyId);
                caller.WriteByte(message);
            }
            PaintDeadBody(
                this.targetDeadBodyId,
                message);
            return true;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            this.CreateCommonAbilityOption(
                parentOps);

            CreateFloatOption(
                PainterOption.CanPaintDistance,
                1.0f, 1.0f, 5.0f, 0.5f,
                parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.paintDistance = OptionHolder.AllOption[
                GetRoleOptionId(PainterOption.CanPaintDistance)].GetValue();
            this.RoleAbilityInit();
        }
    }
}
