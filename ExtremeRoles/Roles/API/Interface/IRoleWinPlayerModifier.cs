using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.API.Interface;

public interface IRoleWinPlayerModifier
{
    public void ModifiedWinPlayer(
        NetworkedPlayerInfo rolePlayerInfo,
        GameOverReason reason,
		in ExtremeGameResult.WinnerTempData winner);
}
