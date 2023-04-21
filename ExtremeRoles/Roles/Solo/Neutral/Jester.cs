using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Jester : SingleRoleBase, IRoleAbility
{
    public enum JesterOption
    {
        UseSabotage,
        OutburstDistance
    }

    public ExtremeAbilityButton Button
    { 
        get => this.outburstButton;
        set
        {
            this.outburstButton = value;
        }
    }

    private float outburstDistance;
    private PlayerControl tmpTarget;
    private PlayerControl outburstTarget;
    private ExtremeAbilityButton outburstButton;

    public Jester(): base(
        ExtremeRoleId.Jester,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Jester.ToString(),
        ColorPalette.JesterPink,
        false, false, false, false)
    { }

    public static void OutburstKill(
        byte outburstTargetPlayerId, byte killTargetPlayerId)
    {
        if (outburstTargetPlayerId != CachedPlayerControl.LocalPlayer.PlayerId) { return; }

        PlayerControl killer = Helper.Player.GetPlayerControlById(outburstTargetPlayerId);
        PlayerControl target = Helper.Player.GetPlayerControlById(killTargetPlayerId);

        if (killer == null || target == null) { return; }

        byte killerId = killer.PlayerId;
        byte targetId = target.PlayerId;

        var killerRole = ExtremeRoleManager.GameRole[killerId];
        var targetRole = ExtremeRoleManager.GameRole[targetId];

        if (!killer.CanMove) { return; }

        bool canKill = killerRole.TryRolePlayerKillTo(
             killer, target);
        if (!canKill) { return; }

        canKill = targetRole.TryRolePlayerKilledFrom(
            target, killer);
        if (!canKill) { return; }

        var multiAssignRole = killerRole as MultiAssignRoleBase;
        if (multiAssignRole != null)
        {
            if (multiAssignRole.AnotherRole != null)
            {
                canKill = multiAssignRole.AnotherRole.TryRolePlayerKillTo(
                    killer, target);
                if (!canKill) { return; }
            }
        }

        multiAssignRole = targetRole as MultiAssignRoleBase;
        if (multiAssignRole != null)
        {
            if (multiAssignRole.AnotherRole != null)
            {
                canKill = multiAssignRole.AnotherRole.TryRolePlayerKilledFrom(
                    target, killer);
                if (!canKill) { return; }
            }
        }

        if (Crewmate.BodyGuard.TryRpcKillGuardedBodyGuard(
                killer.PlayerId, target.PlayerId) ||
            Patches.Button.KillButtonDoClickPatch.IsMissMuderKill(
                killer, target))
        {
            return;
        }

        Helper.Player.RpcUncheckMurderPlayer(
            killerId, target.PlayerId,
            byte.MaxValue);
    }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "outburst",
            Loader.CreateSpriteFromResources(
                Path.JesterOutburst),
            abilityOff: CleanUp,
            forceAbilityOff: () => { });
    }

    public override bool IsSameTeam(SingleRoleBase targetRole) =>
        this.IsNeutralSameTeam(targetRole);

    public bool IsAbilityUse()
    {
        this.tmpTarget = Helper.Player.GetClosestPlayerInRange(
            CachedPlayerControl.LocalPlayer, this,
            this.outburstDistance);
        return this.IsCommonUse() && this.tmpTarget != null;
    }

    public override void ExiledAction(PlayerControl rolePlayer)
    {
        this.IsWin = true;
    }

    public bool UseAbility()
    {
        this.outburstTarget = this.tmpTarget;
        return true;
    }
    public void CleanUp()
    {
        if (this.outburstTarget == null) { return; }
        if (this.outburstTarget.Data.IsDead || this.outburstTarget.Data.Disconnected) { return; }
        if (ExtremeRoleManager.GameRole.Count == 0) { return; }

        var role = ExtremeRoleManager.GameRole[this.outburstTarget.PlayerId];
        if (!role.CanKill()) { return; }

        PlayerControl killTarget = Helper.Player.GetClosestPlayerInKillRange(
            this.outburstTarget);

        if (killTarget == null) { return; }
        if (killTarget.Data.IsDead || killTarget.Data.Disconnected) { return; }
        if (killTarget.PlayerId == CachedPlayerControl.LocalPlayer.PlayerId) { return; }
        
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.JesterOutburstKill))
        {
            caller.WriteByte(this.outburstTarget.PlayerId);
            caller.WriteByte(killTarget.PlayerId);
        }
        OutburstKill(this.outburstTarget.PlayerId, killTarget.PlayerId);
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        CreateFloatOption(
            JesterOption.OutburstDistance,
            1.0f, 0.0f, 2.0f, 0.1f,
            parentOps);

        CreateBoolOption(
            JesterOption.UseSabotage,
            true, parentOps);

        this.CreateAbilityCountOption(
            parentOps, 5, 100, 2.0f);
    }

    protected override void RoleSpecificInit()
    {
        this.UseSabotage = AllOptionHolder.Instance.GetValue<bool>(
            GetRoleOptionId(JesterOption.UseSabotage));
        this.outburstDistance = AllOptionHolder.Instance.GetValue<float>(
            GetRoleOptionId(JesterOption.OutburstDistance));
        this.RoleAbilityInit();
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }
}
