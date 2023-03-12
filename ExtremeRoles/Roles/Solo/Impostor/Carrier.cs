using System.Collections;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Carrier : SingleRoleBase, IRoleAbility, IRoleSpecialReset
{
    private DeadBody carringBody;
    private float alphaValue;
    private bool canReportOnCarry;
    private GameData.PlayerInfo targetBody;

    public enum CarrierOption
    {
        CarryDistance,
        CanReportOnCarry,
    }

    private float carryDistance;

    public ExtremeAbilityButton Button
    {
        get => this.carryButton;
        set
        {
            this.carryButton = value;
        }
    }

    private ExtremeAbilityButton carryButton;

    public Carrier() : base(
        ExtremeRoleId.Carrier,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.Carrier.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    {
        this.canReportOnCarry = false;
    }

    public static void Ability(
        byte rolePlayerId, float x, float y,
        byte targetPlayerId, bool isCarry)
    {
        var rolePlayer = Player.GetPlayerControlById(rolePlayerId);
        var role = ExtremeRoleManager.GetSafeCastedRole<Carrier>(rolePlayerId);
        if (role == null || rolePlayer == null) { return; }

        rolePlayer.NetTransform.SnapTo(new Vector2(x, y));

        if (isCarry)
        {
            carryDeadBody(role, rolePlayer, targetPlayerId);
        }
        else
        {
            rolePlayer.StartCoroutine(
                deadBodyToReportablePosition(
                    role, rolePlayer).WrapToIl2Cpp());
        }
    }

    private static void carryDeadBody(
        Carrier role, PlayerControl rolePlayer, byte targetPlayerId)
    {
        DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        for (int i = 0; i < array.Length; ++i)
        {
            if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == targetPlayerId)
            {
                Color oldColor = array[i].bodyRenderer.color;

                role.carringBody = array[i];
                role.carringBody.transform.position = rolePlayer.transform.position;
                role.carringBody.transform.SetParent(rolePlayer.transform);
                
                role.alphaValue = oldColor.a;
                role.carringBody.bodyRenderer.color = new Color(
                    oldColor.r, oldColor.g, oldColor.b, 0);
                
                if (!role.canReportOnCarry)
                {
                    role.carringBody.GetComponentInChildren<BoxCollider2D>().enabled = false;
                }
                
                break;
            }
        }
    }

    private static IEnumerator deadBodyToReportablePosition(
        Carrier role,
        PlayerControl rolePlayer)
    {
        if (role.carringBody == null) { yield break; }

        if (!rolePlayer.inVent && !rolePlayer.moveable)
        {
            do
            {
                yield return null;
            }
            while (!rolePlayer.moveable);
        }

        role.carringBody.transform.SetParent(null);

        Vector2 pos = rolePlayer.GetTruePosition();
        role.carringBody.transform.position = new Vector3(pos.x, pos.y, (pos.y / 1000f));

        Color color = role.carringBody.bodyRenderer.color;
        role.carringBody.bodyRenderer.color = new Color(
            color.r, color.g, color.b, role.alphaValue);
        if (!role.canReportOnCarry)
        {
            role.carringBody.GetComponentInChildren<BoxCollider2D>().enabled = true;
        }
        role.carringBody = null;
    }
    
    public void CreateAbility()
    {
        this.CreateReclickableAbilityButton(
            "carry",
            Loader.CreateSpriteFromResources(
               Path.CarrierCarry),
            abilityOff: this.CleanUp);
    }

    public bool IsAbilityUse()
    {
        this.targetBody = Player.GetDeadBodyInfo(this.carryDistance);
        return this.IsCommonUse() && this.targetBody != null;
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
        PlayerControl player = CachedPlayerControl.LocalPlayer;
        Vector3 pos = player.transform.position;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.CarrierAbility))
        {
            caller.WriteByte(player.PlayerId);
            caller.WriteFloat(pos.x);
            caller.WriteFloat(pos.y);
            caller.WriteByte(this.targetBody.PlayerId);
            caller.WriteBoolean(true);
        }
        carryDeadBody(
            this, player,
            this.targetBody.PlayerId);
        return true;
    }

    public void CleanUp()
    {
        if (this.carringBody == null) { return; }

        PlayerControl player = CachedPlayerControl.LocalPlayer;
        Vector3 pos = player.transform.position;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.CarrierAbility))
        {
            caller.WriteByte(player.PlayerId);
            caller.WriteFloat(pos.x);
            caller.WriteFloat(pos.y);
            caller.WriteByte(byte.MinValue);
            caller.WriteBoolean(false);
        }
        player.StartCoroutine(
            deadBodyToReportablePosition(
                this, player).WrapToIl2Cpp());
    }

    protected override void CreateSpecificOption(
        IOption parentOps)
    {
        this.CreateCommonAbilityOption(
            parentOps, 5.0f);

        CreateFloatOption(
            CarrierOption.CarryDistance,
            1.0f, 1.0f, 5.0f, 0.5f,
            parentOps);

        CreateBoolOption(
            CarrierOption.CanReportOnCarry,
            true, parentOps);
    }

    protected override void RoleSpecificInit()
    {
        this.carryDistance = OptionHolder.AllOption[
            GetRoleOptionId(CarrierOption.CarryDistance)].GetValue();
        this.canReportOnCarry = OptionHolder.AllOption[
            GetRoleOptionId(CarrierOption.CanReportOnCarry)].GetValue();
        this.RoleAbilityInit();
    }

    public void AllReset(PlayerControl rolePlayer)
    {
        if (this.carringBody == null) { return; }

        this.carringBody.transform.SetParent(null);
        this.carringBody.transform.position = rolePlayer.GetTruePosition() + new Vector2(0.15f, 0.15f);
        this.carringBody.transform.position -= new Vector3(0.0f, 0.0f, 0.01f);

        Color color = this.carringBody.bodyRenderer.color;
        this.carringBody.bodyRenderer.color = new Color(
            color.r, color.g, color.b, this.alphaValue);
        if (!this.canReportOnCarry)
        {
            this.carringBody.GetComponentInChildren<BoxCollider2D>().enabled = true;
        }
        this.carringBody = null;
    }
}
