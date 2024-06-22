using System.Collections;

using BepInEx.Unity.IL2CPP.Utils.Collections;

using UnityEngine;

using HarmonyLib;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;
using ExtremeRoles.Performance;

using Il2CppEnumerator = Il2CppSystem.Collections.IEnumerator;

namespace ExtremeRoles.Patches.Controller;

[HarmonyPatch(typeof(PbExileController), nameof(PbExileController.Animate))]
public static class PbExileControllerAnimePatch
{
	public static bool Prefix(PbExileController __instance, ref Il2CppEnumerator __result)
	{
		var spawnOpt = ExtremeGameModeManager.Instance.ShipOption.Spawn;

		if (spawnOpt.EnableSpecialSetting &&
			spawnOpt.Polus)
		{
			__result = animateWithRandomSpawn(__instance).WrapToIl2Cpp();
			return false;
		}
		return true;
	}

	private static IEnumerator animateWithRandomSpawn(PbExileController __instance)
	{
		var hud = FastDestroyableSingleton<HudManager>.Instance;

		yield return hud.CoFadeFullScreen(Color.black, Color.clear, 0.2f, false);
		yield return Effects.Wait(0.75f);
		yield return Effects.All(new Il2CppEnumerator[]
		{
			__instance.PlayerFall(),
			__instance.PlayerSpin(),
			__instance.HandleText(__instance.Duration * 0.5f, __instance.Duration * 0.5f)
		});

		if (GameManager.Instance.LogicOptions.GetConfirmImpostor())
		{
			__instance.ImpostorText.gameObject.SetActive(true);
		}

		yield return Effects.Bloop(0f, __instance.ImpostorText.transform, 1f, 0.5f);
		yield return new WaitForSeconds(0.5f);
		yield return hud.CoFadeFullScreen(Color.clear, Color.black, 0.2f, false);
		if (__instance.finalSinkCoroutine != null)
		{
			__instance.StopCoroutine(__instance.finalSinkCoroutine);
		}
		yield return ExtremeSpawnSelectorMinigame.WrapUpAndSpawn(__instance);

		yield break;
	}
}
