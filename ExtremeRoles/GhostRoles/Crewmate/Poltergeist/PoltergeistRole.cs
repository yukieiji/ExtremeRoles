using System.Collections.Generic;

using UnityEngine;


using ExtremeRoles.Helper;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GhostRoles.API.Interface;

namespace ExtremeRoles.GhostRoles.Crewmate.Poltergeist;

#nullable enable

public sealed class PoltergeistRole : GhostRoleBase
{
    private float range;

	private DeadBody? carringBody;
	private NetworkedPlayerInfo? targetBody;

    public PoltergeistRole(IGhostRoleCoreProvider provider) : base(
        true,
		provider.Get(ExtremeGhostRoleId.Poltergeist))
    {
		this.range = this.Loader.GetValue<Option, float>(Option.Range);
	}

    public static void DeadbodyMove(
        byte playerId, byte targetPlayerId,
        float x, float y, bool pickUp)
    {

        var rolePlayer = Player.GetPlayerControlById(playerId);
        var role = ExtremeGhostRoleManager.GetSafeCastedGhostRole<PoltergeistRole>(playerId);
        if (role == null || rolePlayer == null) { return; }

        rolePlayer.NetTransform.SnapTo(new Vector2(x, y));

        if (pickUp)
        {
            pickUpDeadBody(rolePlayer, role, targetPlayerId);
        }
        else
        {
            setDeadBody(rolePlayer, role);
        }
    }
    private static void pickUpDeadBody(
        PlayerControl rolePlayer,
        PoltergeistRole role,
        byte targetPlayerId)
    {

        DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
        for (int i = 0; i < array.Length; ++i)
        {
            if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == targetPlayerId)
            {
                role.carringBody = array[i];
                role.carringBody.transform.position = rolePlayer.transform.position;
                role.carringBody.transform.SetParent(rolePlayer.transform);
                break;
            }
        }
    }

    private static void setDeadBody(
        PlayerControl rolePlayer,
        PoltergeistRole role)
    {
        if (role.carringBody == null) { return; }
        if (role.carringBody.transform.parent != rolePlayer.transform) { return; }

        Vector2 pos = rolePlayer.GetTruePosition();
        role.carringBody.transform.SetParent(null);
        role.carringBody.transform.position = new Vector3(pos.x, pos.y, (pos.y / 1000f));
        role.carringBody = null;
    }

    public override void CreateAbility()
    {
        this.Button = GhostRoleAbilityFactory.CreateActivatingCountAbility(
            AbilityType.PoltergeistMoveDeadbody,
            Resources.UnityObjectLoader.LoadSpriteFromResources(
                Resources.ObjectPath.CarrierCarry),
            this.IsReportAbility(),
            this.isPreCheck,
            this.isAbilityUse,
            this.UseAbility,
            abilityCall,
			true, null,
            cleanUp,
            cleanUp, KeyCode.F);
        this.ButtonInit();
        this.Button.SetLabelToCrewmate();
    }

    public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>();

    protected override void OnMeetingEndHook()
    {
        return;
    }

    protected override void OnMeetingStartHook()
    {
        this.targetBody = null;
    }

    protected override void UseAbility(RPCOperator.RpcCaller caller)
    {
        PlayerControl player = PlayerControl.LocalPlayer;
        Vector3 pos = player.transform.position;

        caller.WriteByte(player.PlayerId);
        caller.WriteByte(this.targetBody!.PlayerId);
        caller.WriteFloat(pos.x);
        caller.WriteFloat(pos.y);
        caller.WriteBoolean(true);
    }

    private bool isPreCheck() => this.targetBody != null;

    private bool isAbilityUse()
    {
        this.targetBody = null;

        if (ShipStatus.Instance == null ||
            !ShipStatus.Instance.enabled) { return false; }

        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();

        foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(
            truePosition, this.range, Constants.PlayersOnlyMask))
        {
            if (!collider2D.CompareTag("DeadBody")) { continue; }

            DeadBody component = collider2D.GetComponent<DeadBody>();

            if (component && !component.Reported && component.transform.parent == null)
            {
                Vector2 truePosition2 = component.TruePosition;
                if ((Vector2.Distance(truePosition2, truePosition) <= range) &&
                    (PlayerControl.LocalPlayer.CanMove) &&
                    (!PhysicsHelpers.AnythingBetween(
                        truePosition, truePosition2,
                        Constants.ShipAndObjectsMask, false)))
                {
                    this.targetBody = GameData.Instance.GetPlayerById(component.ParentId);
                    break;
                }
            }
        }

        return IsCommonUse() && this.targetBody != null;
    }
    private void abilityCall()
    {
        pickUpDeadBody(PlayerControl.LocalPlayer, this, this.targetBody!.PlayerId);
        this.targetBody = null;
    }
    private void cleanUp()
    {
        PlayerControl player = PlayerControl.LocalPlayer;
        Vector3 pos = player.transform.position;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.UseGhostRoleAbility))
        {
            caller.WriteByte((byte)AbilityType.PoltergeistMoveDeadbody); // アビリティタイプ
            caller.WriteBoolean(false); // 報告できるかどうか
            caller.WriteByte(player.PlayerId);
            caller.WriteByte(byte.MinValue);
            caller.WriteFloat(pos.x);
            caller.WriteFloat(pos.y);
            caller.WriteBoolean(false);
        }

        setDeadBody(player, this);
    }
}
