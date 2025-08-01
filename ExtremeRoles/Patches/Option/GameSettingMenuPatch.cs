using System.Collections;

using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using UnityEngine;
using ExtremeRoles.Module.CustomMonoBehaviour;

#nullable enable

using ControllerAU = Controller;

namespace ExtremeRoles.Patches.Option;



[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
public static class GameSettingMenuStartPatch
{
	public static void Postfix(GameSettingMenu __instance)
	{
		ExtremeGameSettingMenu.Initialize(__instance);

		// 最初にゲーム設定画面を出すようにする、OnEnableにパッチを当てると処理の都合でバグるので処理が終わってからここで呼び出す
		ControllerManager.Instance.OpenOverlayMenu(
			__instance.name, __instance.BackButton,
			__instance.GameSettingsButton,
			__instance.ControllerSelectable, false);
		if (ControllerAU.currentTouchType != ControllerAU.TouchType.Joystick)
		{
			// 0はプリセット、1はゲーム設定
			__instance.ChangeTab(1, ControllerAU.currentTouchType == ControllerAU.TouchType.Joystick);
		}
		__instance.StartCoroutine(coSelectDefault(__instance));
	}

	private static IEnumerator coSelectDefault(GameSettingMenu __instance)
	{
		yield return new WaitForEndOfFrame();
		ControllerManager.Instance.SetCurrentSelected(__instance.GameSettingsButton);
		yield break;
	}
}

// ここの処理は使わないので消す
[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.OnEnable))]
public static class GameSettingMenuOnEnablePatch
{
	public static bool Prefix(GameSettingMenu __instance)
	{
		return false;
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
