using UnityEngine;

using ExtremeRoles.Module;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;

using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Host;

public sealed partial class Xion : SingleRoleBase
{
    public static byte PlayerId = byte.MaxValue;
    private float defaultCameraZoom;

    public Xion(byte xionPlayerId) : base(
		new RoleCore(ExtremeRoleId.Xion, ExtremeRoleType.Null, ColorPalette.XionBlue),
        false, false, false, true,
        true, true, true, true, true)
    {
        this.MoveSpeed = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
            AmongUs.GameOptions.FloatOptionNames.PlayerSpeedMod);
        this.defaultCameraZoom = UnityEngine.Camera.main.orthographicSize;
        this.dummyDeadBody.Clear();
        PlayerId = xionPlayerId;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    { }

    public static void XionPlayerToGhostLayer()
    {
        PlayerControl player = Helper.Player.GetPlayerControlById(PlayerId);
        if (player != null)
        {
            player.gameObject.layer = LayerMask.NameToLayer("Ghost");
        }
    }

    public static void RemoveXionPlayerToAllPlayerControl()
    {
        bool isXion(PlayerControl x) => x.PlayerId == PlayerId;

        PlayerControl.AllPlayerControls.RemoveAll(
            (Il2CppSystem.Predicate<PlayerControl>)isXion);
		PlayerCache.AllPlayerControl.RemoveAll(isXion);
    }
}
