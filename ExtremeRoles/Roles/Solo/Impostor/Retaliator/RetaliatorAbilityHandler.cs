using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.Retaliator;

public class RetaliatorAbilityHandler(RetaliatorStatusModel status) : IAbility, IKilledFrom, ITryKillTo
{
    private readonly RetaliatorStatusModel status = status;

    public bool IsAbilityUse() => this.status.IsAwake && this.status.RemainingAbilityUses > 0;

    public bool TryKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)
    {
        if (fromPlayer == null || rolePlayer == null)
        {
            return true;
        }

        if (!this.status.IsTargeted(fromPlayer.PlayerId))
        {
            return true;
        }

        if (!ExtremeRoleManager.TryGetRole(fromPlayer.PlayerId, out var fromRole))
        {
            return true;
        }

        bool isValidTeam = this.status.TargetTeam switch
        {
            RetaliatorTargetTeam.CrewmateOnly => fromRole.IsCrewmate(),
            RetaliatorTargetTeam.NeutralOnly => fromRole.IsNeutral(),
            RetaliatorTargetTeam.BothCrewAndNeutral => fromRole.IsCrewmate() || fromRole.IsNeutral(),
            _ => false
        };

        if (!isValidTeam)
        {
            return true;
        }

        Player.RpcUncheckMurderPlayer(
            fromPlayer.PlayerId,
            fromPlayer.PlayerId,
            byte.MaxValue);

        ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
            fromPlayer.PlayerId,
            ExtremeShipStatus.PlayerStatus.Retaliate);

        return false;
    }

    public bool TryRolePlayerKillTo(PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
        if (!this.status.IsAwake)
        {
            this.status.IncrementKill();
        }
        return true;
    }
}
