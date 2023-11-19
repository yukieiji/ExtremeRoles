using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.API.Interface;

public interface IRoleWinPlayerModifier
{
    public void ModifiedWinPlayer(
        GameData.PlayerInfo rolePlayerInfo,
        GameOverReason reason,
		ref ExtremeGameResult.WinnerTempData winner);
}
