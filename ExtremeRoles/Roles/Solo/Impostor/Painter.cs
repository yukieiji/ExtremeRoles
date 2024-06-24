using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;




using ExtremeRoles.Module.NewOption.Factory;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Painter : SingleRoleBase, IRoleAutoBuildAbility
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
            if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId != targetPlayerId)
            {
                continue;
            }

            DeadBody body = array[i];

            foreach (var rend in body.bodyRenderers)
            {
                Color oldColor = rend.color;

                if (isRandomColorMode)
                {
                    Color newColor = new Color(
                        Random.value,
                        Random.value,
                        Random.value,
                        oldColor.a);

                    rend.material.SetColor("_BackColor", newColor);
                    rend.material.SetColor("_BodyColor", newColor);
                }
                else
                {
                    rend.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0);
                }
            }
            break;
        }
    }

    public void CreateAbility()
    {

        this.randomColorPaintImage = Resources.Loader.CreateSpriteFromResources(
			Path.PainterPaintRandom);
        this.transColorPaintImage = Resources.Loader.CreateSpriteFromResources(
			Path.PainterPaintTrans);

        this.CreateNormalAbilityButton(
            "paint", this.randomColorPaintImage);
    }

    public bool IsAbilityUse()
    {
        this.targetDeadBodyId = byte.MaxValue;
        NetworkedPlayerInfo info = Player.GetDeadBodyInfo(
            this.paintDistance);

        this.Button.Behavior.SetButtonImage(
            Key.IsShift() ?
            this.randomColorPaintImage : this.transColorPaintImage);

        if (info != null)
        {
            this.targetDeadBodyId = info.PlayerId;
        }

        return IRoleAbility.IsCommonUse() && this.targetDeadBodyId != byte.MaxValue;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public bool UseAbility()
    {
        byte message = Key.IsShift() ? byte.MaxValue : byte.MinValue;

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
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateCommonAbilityOption(
            factory);

        factory.CreateFloatOption(
            PainterOption.CanPaintDistance,
            1.0f, 1.0f, 5.0f, 0.5f);
    }

    protected override void RoleSpecificInit()
    {
        this.paintDistance = this.Loader.GetValue<PainterOption, float>(
            PainterOption.CanPaintDistance);
    }
}
