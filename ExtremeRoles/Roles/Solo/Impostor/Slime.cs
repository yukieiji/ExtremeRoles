using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Slime : SingleRoleBase, IRoleAbility, IRoleSpecialReset
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

    private Dictionary<Collider2D, IUsable[]> cache;
    private Collider2D[] hitBuffer;

    public sealed class ColliderComparer : IEqualityComparer<Collider2D>
    {
        public bool Equals(Collider2D x, Collider2D y)
        {
            return x == y;
        }

        public int GetHashCode(Collider2D obj)
        {
            return obj.GetInstanceID();
        }
    }

    public Slime() : base(
        ExtremeRoleId.Slime,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.Slime.ToString(),
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
        }
        else
        {
        }
    }
    
    public void CreateAbility()
    {
        this.CreateReclickableAbilityButton(
            "carry",
            Loader.CreateSpriteFromResources(
               Path.CarrierCarry),
            abilityOff: this.CleanUp);

        this.hitBuffer = new Collider2D[60];
        this.cache = new Dictionary<Collider2D, IUsable[]>(new ColliderComparer());
    }

    public bool IsAbilityUse()
    {
        


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
        return true;
    }

    public void CleanUp()
    {
        PlayerControl player = CachedPlayerControl.LocalPlayer;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.SlimeAbility))
        {
            caller.WriteByte(player.PlayerId);
        }
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
