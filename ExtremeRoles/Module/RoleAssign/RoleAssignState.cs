using System.Collections.Generic;

using ExtremeRoles.Helper;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class RoleAssignState
{
    public static RoleAssignState Instance => instance;
    private static RoleAssignState instance = new RoleAssignState();

    public bool IsRoleSetUpEnd { get; private set; } = false;

    // ホスト以外の準備ができてるか
    public bool IsReady => this.readyPlayer.Count ==
        (PlayerControl.AllPlayerControls.Count - 1);

    private HashSet<byte> readyPlayer = new HashSet<byte>();

    public void SwitchRoleAssignToEnd()
    {
        this.IsRoleSetUpEnd = true;
        this.readyPlayer.Clear();
    }

    public void Reset()
    {
        this.IsRoleSetUpEnd = false;
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
