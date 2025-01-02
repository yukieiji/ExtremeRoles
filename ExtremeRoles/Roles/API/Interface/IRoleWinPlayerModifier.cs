using ExtremeRoles.Module.GameResult;

namespace ExtremeRoles.Roles.API.Interface;

public interface IRoleWinPlayerModifier
{
    public void ModifiedWinPlayer(
        NetworkedPlayerInfo rolePlayerInfo,
        GameOverReason reason,
		in WinnerTempData winner);
}
