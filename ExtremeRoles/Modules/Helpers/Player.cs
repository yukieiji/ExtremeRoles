using System;
using System.Collections.Generic;
using System.Text;

namespace ExtremeRoles.Modules.Helpers
{
    public class Player
    {
        public enum MurderAttemptResult
        {
            PerformKill,
            SuppressKill,
            BlankKill
        }

        public static PlayerControl GetPlayerControlById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == id) { return player; }
            }
            return null;
        }

        public static Dictionary<byte, PlayerControl> AllPlayersById()
        {
            Dictionary<byte, PlayerControl> res = new Dictionary<byte, PlayerControl>();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                res.Add(player.PlayerId, player);
            return res;
        }

    }
}
