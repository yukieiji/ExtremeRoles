using Il2CppSystem.Collections.Generic;

namespace ExtremeRoles.Roles.API.Interface
{
    public interface IRoleWinPlayerModifier
    {
        public void ModifiedWinPlayer(
            GameData.PlayerInfo rolePlayerInfo,
            GameOverReason reason,
            ref List<WinningPlayerData> winner,
            ref System.Collections.Generic.List<PlayerControl> pulsWinner);
    }

    public static class RoleWinPlayerModifierMixin
    {
        public static void AddWinner(
            this IRoleWinPlayerModifier self,
            GameData.PlayerInfo playerInfo,
            List<WinningPlayerData> winner,
            System.Collections.Generic.List<PlayerControl> pulsWinner)
        {
            winner.Add(new WinningPlayerData(playerInfo));
            pulsWinner.Add(playerInfo.Object);
        }

        public static void RemoveWinner(
            this IRoleWinPlayerModifier self,
            GameData.PlayerInfo playerInfo,
            List<WinningPlayerData> winner,
            System.Collections.Generic.List<PlayerControl> pulsWinner)
        {
           
            winner.Remove(new WinningPlayerData(playerInfo));
            if (pulsWinner.Contains(playerInfo.Object))
            {
                pulsWinner.Remove(playerInfo.Object);
            }
        }

    }

}
