using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.API.Interface;

public interface IRoleWinPlayerModifier
{
    public void ModifiedWinPlayer(
        NetworkedPlayerInfo rolePlayerInfo,
        GameOverReason reason,
		ref ExtremeGameResult.WinnerTempData winner);
}
