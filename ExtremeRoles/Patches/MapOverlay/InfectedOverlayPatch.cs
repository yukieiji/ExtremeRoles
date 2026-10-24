using HarmonyLib;

using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Extension.UnityEvents;
using UnityEngine;

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

		UpdateSabotageButtonState(__instance);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.Update))]
	public static void UpdatePostfix(InfectedOverlay __instance)
	{
		UpdateSabotageButtonState(__instance);
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

	private static void UpdateSabotageButtonState(InfectedOverlay instance)
	{
		bool isActive = !(ExtremeSystemTypeManager.Instance.TryGet<ForestallerSystem>(ForestallerSystem.Type, out var system) &&
			system.IsForestallActive);

		foreach (var button in instance.allButtons)
		{
			button.gameObject.SetActive(isActive);
		}
	}
}
