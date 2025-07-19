using ExtremeRoles.Module;

using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability;

using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Patches.Button;

namespace ExtremeRoles.Roles.Neutral.Jester;

public sealed class JesterRole : SingleRoleBase, IRoleAutoBuildAbility
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
		RoleCore.BuildNeutral(
			ExtremeRoleId.Jester,
			ColorPalette.JesterPink),
        false, false, false, false)
    { }

    public static void OutburstKill(
        byte outburstTargetPlayerId, byte killTargetPlayerId)
    {
        if (outburstTargetPlayerId != PlayerControl.LocalPlayer.PlayerId)
		{
			return;
		}

        PlayerControl killer = Helper.Player.GetPlayerControlById(outburstTargetPlayerId);
        PlayerControl target = Helper.Player.GetPlayerControlById(killTargetPlayerId);

        if (!(
				killer != null &&
				ExtremeRoleManager.TryGetRole(killer.PlayerId, out var killerRole) &&
				KillButtonDoClickPatch.CheckPreKillConditionWithBool(killerRole, killer, target)
			))
		{
			return;
		}
        Helper.Player.RpcUncheckMurderPlayer(
			outburstTargetPlayerId, killTargetPlayerId,
            byte.MaxValue);
    }

    public void CreateAbility()
    {
        this.CreateActivatingAbilityCountButton(
            "outburst",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.JesterOutburst),
            abilityOff: CleanUp,
            forceAbilityOff: () => { });
    }

    public override bool IsSameTeam(SingleRoleBase targetRole) =>
        this.IsNeutralSameTeam(targetRole);

    public bool IsAbilityUse()
    {
        this.tmpTarget = Helper.Player.GetClosestPlayerInRange(
            PlayerControl.LocalPlayer, this,
            this.outburstDistance);
        return IRoleAbility.IsCommonUse() && this.tmpTarget != null;
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
        if (killTarget.PlayerId == PlayerControl.LocalPlayer.PlayerId) { return; }

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.JesterOutburstKill))
        {
            caller.WriteByte(this.outburstTarget.PlayerId);
            caller.WriteByte(killTarget.PlayerId);
        }
        OutburstKill(this.outburstTarget.PlayerId, killTarget.PlayerId);
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateFloatOption(
            JesterOption.OutburstDistance,
            1.0f, 0.0f, 2.0f, 0.1f);

        factory.CreateBoolOption(
            JesterOption.UseSabotage,
            true);

        IRoleAbility.CreateAbilityCountOption(
            factory, 5, 100, 2.0f);
    }

    protected override void RoleSpecificInit()
    {
		var cat = this.Loader;
        this.UseSabotage = cat.GetValue<bool>(
            (int)JesterOption.UseSabotage);
        this.outburstDistance = cat.GetValue<float>(
			(int)JesterOption.OutburstDistance);
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }
}
