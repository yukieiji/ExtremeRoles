using ExtremeRoles.Helper;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.API.Interface.Ability;

namespace ExtremeRoles.Roles.Solo.Neutral.IronMate;

public class IronMateAbilityHandler(IronMateStatusModel status, IronMateGurdSystem system) : IAbility, IKilledFrom
{
    private readonly IronMateStatusModel status = status;
	private readonly IronMateGurdSystem system = system;

    public bool TryKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)
    {
        byte playerId = rolePlayer.PlayerId;

        if (!this.system.IsContains(playerId))
        {
			this.system.SetUp(playerId, status.BlockNum);
        }

        if (!this.system.TryGetShield(playerId, out int num))
        {
            return true;
        }

        if (fromPlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            fromPlayer.SetKillTimer(10.0f);
            Sound.PlaySound(Sound.Type.GuardianAngleGuard, 0.85f);
        }
        this.system.RpcUpdateNum(playerId, num - 1);
        return false;
    }
}
