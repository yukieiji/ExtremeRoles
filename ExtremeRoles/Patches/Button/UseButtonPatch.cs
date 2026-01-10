using HarmonyLib;

using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Core.Service.SystemType;

namespace ExtremeRoles.Patches.Button;

[HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
public static class UseButtonReceiveClickDownPatch
{
    public static bool Prefix()
    {
        if (!GameProgressSystem.IsTaskPhase)
		{
			return true;
		}

		var pc = PlayerControl.LocalPlayer;
		if (pc == null || pc.gameObject.TryGetComponent<BoxerButtobiBehaviour>(out _))
		{
			return false;
		}

		return ExtremeRoleManager.GetLocalRoleCastedStatusFlag<IUsableOverrideStatus>(x => x.EnableUseButton);
    }
}
