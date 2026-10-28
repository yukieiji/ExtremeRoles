using UnityEngine;

using ExtremeRoles.Module;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;

namespace ExtremeRoles.Roles.Solo.Host;

public sealed partial class Xion : SingleRoleBase
{
    public static byte PlayerId = byte.MaxValue;
    private float defaultCameraZoom;

	private readonly PlayerSpeedSystem speedSystem;

    public Xion(byte xionPlayerId) : base(
		new RoleArgs(
			new RoleCore(ExtremeRoleId.Xion, ExtremeRoleType.Null, ColorPalette.XionBlue),
			RolePropPresets.OptionalDefault | RoleProp.UseSabotage)
		)
    {
		this.speedSystem = PlayerSpeedSystem.Get();
        this.defaultCameraZoom = UnityEngine.Camera.main.orthographicSize;
        this.dummyDeadBody.Clear();
        PlayerId = xionPlayerId;
    }

	public void speedUp()
	{
		if (speedSystem.TryGetSpeed(PlayerControl.LocalPlayer.PlayerId, out float curSpeed))
		{
			speedSystem.UpdateSelfSpeed(Mathf.Clamp(curSpeed * 1.25f, MinSpeed, MaxSpeed));
		}
	}

	public void speedDown()
	{
		if (speedSystem.TryGetSpeed(PlayerControl.LocalPlayer.PlayerId, out float curSpeed))
		{
			speedSystem.UpdateSelfSpeed(Mathf.Clamp(curSpeed * 0.75f, MinSpeed, MaxSpeed));
		}
	}

	public void resetSpeed()
	{
		speedSystem.UpdateSelfSpeed(0.0f);
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
		PlayerCache.RemovePlayerControl(isXion);
    }
}
