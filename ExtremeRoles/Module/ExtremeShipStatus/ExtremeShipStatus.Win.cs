using System.Collections.Generic;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        public GameOverReason EndReason => this.reason;
        public int WinGameControlId => this.winGameControlId;
        public bool IsDisableWinCheck => this.disableWinCheck;

        private bool disableWinCheck = false;
        private GameOverReason reason;
        private int winGameControlId = int.MaxValue;
        private List<GameData.PlayerInfo> plusWinner = new List<GameData.PlayerInfo>();

        public void RpcRoleIsWin(byte playerId)
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.SetRoleWin))
            {
                caller.WriteByte(playerId);
            }
            RPCOperator.SetRoleWin(playerId);
        }

        public void AddWinner(PlayerControl player)
        {
            this.plusWinner.Add(player.Data);
        }

        public void AddWinner(GameData.PlayerInfo player)
        {
            this.plusWinner.Add(player);
        }

        public void SetDisableWinCheck(bool state)
        {
            this.disableWinCheck = state;
        }

        public List<GameData.PlayerInfo> GetPlusWinner() => this.plusWinner;

        public void SetGameOverReason(GameOverReason endReason)
        {
            this.reason = endReason;
        }

        public void SetPlusWinner(List<GameData.PlayerInfo> newWinner)
        {
            this.plusWinner = newWinner;
        }

        public void SetWinControlId(int id)
        {
            this.winGameControlId = id;
        }

        private void resetWins()
        {
            this.plusWinner.Clear();
            this.winGameControlId = int.MaxValue;
            this.disableWinCheck = false;
        }
    }
}
