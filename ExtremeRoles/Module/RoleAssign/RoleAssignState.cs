using System.Collections.Generic;

using ExtremeRoles.Helper;

namespace ExtremeRoles.Module.RoleAssign
{
    public sealed class RoleAssignState
    {
        public static RoleAssignState Instance => instance;
        private static RoleAssignState instance = new RoleAssignState();

        public bool IsRoleSetUpEnd => isRoleSetUpEnd;

        // ホスト以外の準備ができてるか
        public bool IsReady => this.readyPlayer.Count ==
            (PlayerControl.AllPlayerControls.Count - 1);

        private HashSet<byte> readyPlayer = new HashSet<byte>();
        private bool isRoleSetUpEnd = false;

        public void SwitchRoleAssignToEnd()
        {
            isRoleSetUpEnd = true;
            this.readyPlayer.Clear();
        }

        public void Reset()
        {
            isRoleSetUpEnd = false;
            this.readyPlayer.Clear();
        }

        internal void AddReadyPlayer(byte playerId)
        {
            if (!AmongUsClient.Instance.AmHost) { return; }

            Logging.Debug($"ReadyPlayer:{playerId}");

            this.readyPlayer.Add(playerId);
        }

        public static void SetLocalPlayerReady()
        {
            using (var caller = RPCOperator.CreateCaller(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.SetUpReady))
            {
                caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
            }
        }
    }
}
