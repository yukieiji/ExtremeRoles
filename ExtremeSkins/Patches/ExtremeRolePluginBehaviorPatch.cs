using HarmonyLib;
using UnityEngine;

using ExtremeRoles;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;

namespace ExtremeSkins.Patches;

[HarmonyPatch(
    typeof(ExtremeRolePluginBehavior),
    nameof(ExtremeRolePluginBehavior.Update))]
public static class ExtremeRolePluginBehaviorPatch
{
    public static void Postfix()
    {
        if (Key.IsAltDown() &&
            Input.GetKeyDown(KeyCode.F12))
        {
            CreatorModeManager.Instance.SwitchMode();
			StatusTextShower.Instance.RebuildVersionShower();
        }
    }
}

[HarmonyPatch(
	typeof(ModId),
	nameof(ModId.Register))]
public static class ModIdRegisterPatch
{
	public static void Postfix()
	{
		ModId.Combine(ExtremeSkinsPlugin.ModId);
	}
}
