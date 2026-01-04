using HarmonyLib;

using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Extension.UnityEvents;

#nullable enable

namespace ExtremeRoles.Patches.MapOverlay;

[HarmonyPatch]
public static class InfectedOverlayPatch
{
	[HarmonyPostfix]

	[HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.OnEnable))]
	public static void EnablePostfix(InfectedOverlay __instance)
	{
		foreach (var button in __instance.allButtons)
		{
			button.OnClick.AddListener(InspectorInspectSystem.InspectSabotage);
		}
	}

	[HarmonyPostfix]

	[HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.OnDisable))]
	public static void DisablePostfix(InfectedOverlay __instance)
	{
		foreach (var button in __instance.allButtons)
		{
			button.OnClick.RemoveListener(InspectorInspectSystem.InspectSabotage);
		}
	}
}
