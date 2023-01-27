using System.Collections.Generic;

using ExtremeRoles.Helper;

namespace ExtremeRoles.Module.RoleAssign
{
    public sealed class RoleAssignState
    {
        public static RoleAssignState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RoleAssignState();
                }
                return instance;
            }
        }
        private static RoleAssignState instance = null;

        public bool IsRoleSetUpEnd => isRoleSetUpEnd;
        public bool IsReady => this.readyPlayer.Count ==
            (PlayerControl.AllPlayerControls.Count - 1);

        private HashSet<byte> readyPlayer = new HashSet<byte>();
        private bool isRoleSetUpEnd;

        public void SwitchRoleAssignToEnd()
        {
            isRoleSetUpEnd = true;
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

        internal static void SetLocalPlayerReady()
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
