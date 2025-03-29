using System.Collections;

using BepInEx.Unity.IL2CPP.Utils.Collections;

using UnityEngine;

using HarmonyLib;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;
using ExtremeRoles.Performance;

using Il2CppEnumerator = Il2CppSystem.Collections.IEnumerator;
using ExtremeRoles.GameMode;

namespace ExtremeRoles.Patches.Controller;

[HarmonyPatch(typeof(MiraExileController), nameof(MiraExileController.Animate))]
public static class MiraExileControllerAnimePatch
{
	public static bool Prefix(MiraExileController __instance, ref Il2CppEnumerator __result)
	{
		var spawnOpt = ExtremeGameModeManager.Instance.ShipOption.Spawn;

		if (spawnOpt.EnableSpecialSetting &&
			spawnOpt.MiraHq)
		{
			__result = animateWithRandomSpawn(__instance).WrapToIl2Cpp();
			return false;
		}
		return true;
	}

	private static IEnumerator animateWithRandomSpawn(MiraExileController __instance)
	{
		var hud = HudManager.Instance;

		if (hud != null)
		{
			yield return hud.CoFadeFullScreen(Color.black, Color.clear, 0.2f, false);
		}
		yield return Effects.All(
		[
			__instance.PlayerSpin(),
			__instance.HandleText(__instance.Duration * 0.5f, __instance.Duration * 0.5f),
			Effects.Slide2D(__instance.BackgroundClouds, new Vector2(0f, -3f), new Vector2(0f, 0.5f), __instance.Duration),
			Effects.Sequence(
			[
				Effects.Wait(2f),
				Effects.Slide2D(__instance.ForegroundClouds, new Vector2(0f, -7f), new Vector2(0f, 2.5f), 0.75f)
			])
		]);
		if (__instance.initData != null &&
			__instance.initData.confirmImpostor)
		{
			__instance.ImpostorText.gameObject.SetActive(true);
		}
		yield return Effects.Bloop(0f, __instance.ImpostorText.transform, 1f, 0.5f);
		yield return new WaitForSeconds(0.5f);

		if (hud != null)
		{
			yield return hud.CoFadeFullScreen(Color.clear, Color.black, 0.2f, false);
		}
		else
		{
			yield return Effects.Wait(0.2f);
		}

		yield return ExtremeSpawnSelectorMinigame.WrapUpAndSpawn(__instance);

		yield break;
	}
}
