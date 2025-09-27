using HarmonyLib;

using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Patches.Button;

// VentButtonクラスに関するパッチ
[HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
public static class VentButtonDoClickPatch
{
    public static bool Prefix(VentButton __instance)
    {
		// Manually modifying the VentButton to use Vent.Use again in order to trigger the Vent.Use prefix patch

		var pc = PlayerControl.LocalPlayer;
		if (__instance.currentTarget == null ||
			pc == null ||
			pc.gameObject.TryGetComponent<BoxerButtobiBehaviour>(out _))
        {
			return false;
        }

		var (useRole, anotherUseRole) =
			ExtremeRoleManager.GetInterfaceCastedLocalRole<IRoleUsableOverride>();

		if (!(
				(useRole is null && anotherUseRole is null) ||
				(useRole.EnableVentButton && anotherUseRole is null) ||
				(useRole is null && anotherUseRole.EnableVentButton) ||
				(useRole.EnableVentButton && anotherUseRole.EnableVentButton)
			))
		{
			return false;
		}

		Helper.Logging.Debug($"VentButtonClicked");
		__instance.currentTarget.Use();

		return false;
    }
}
