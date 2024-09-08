using HarmonyLib;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomMonoBehaviour;
using UnityEngine;
using TMPro;



#nullable enable

namespace ExtremeRoles.Patches.Option;



[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
public static class GameSettingMenuStartPatch
{
	public static void Postfix(GameSettingMenu __instance)
	{
		ExtremeGameSettingMenu.Initialize(__instance);
	}
}

[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.ChangeTab))]
public static class GameSettingMenuChangeTabPatch
{
	public static void Prefix(
		GameSettingMenu __instance,
		[HarmonyArgument(0)] int tabNum,
		[HarmonyArgument(1)] bool previewOnly)
	{
		ExtremeGameSettingMenu.SwitchTabPrefix(__instance, previewOnly);
	}
}
