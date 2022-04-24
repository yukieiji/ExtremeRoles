using Il2CppSystem.Collections.Generic;

namespace ExtremeRoles.Roles.API.Interface
{
    public interface IRoleWinPlayerModifier
    {
        public void ModifiedWinPlayer(
            GameData.PlayerInfo rolePlayerInfo,
            GameOverReason reason,
            List<WinningPlayerData> winner);
    }
}
