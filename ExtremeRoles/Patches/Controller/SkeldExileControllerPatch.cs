using System.Collections;

using BepInEx.Unity.IL2CPP.Utils.Collections;

using UnityEngine;

using HarmonyLib;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;
using ExtremeRoles.Performance;

using Il2CppEnumerator = Il2CppSystem.Collections.IEnumerator;

namespace ExtremeRoles.Patches.Controller;

[HarmonyPatch(typeof(SkeldExileController), nameof(SkeldExileController.Animate))]
public static class SkeldExileControllerAnimePatch
{
	public static bool Prefix(SkeldExileController __instance, ref Il2CppEnumerator __result)
	{
		var spawnOpt = ExtremeGameModeManager.Instance.ShipOption.Spawn;

		if (spawnOpt.EnableSpecialSetting &&
			spawnOpt.Skeld)
		{
			__result = animateWithRandomSpawn(__instance).WrapToIl2Cpp();
			return false;
		}
		return true;
	}

	private static IEnumerator animateWithRandomSpawn(SkeldExileController __instance)
	{
		float num = Camera.main.orthographicSize * Camera.main.aspect + 1f;
		Vector2 left = Vector2.left * num;
		Vector2 right = Vector2.right * num;
		__instance.Player.transform.localPosition = left;

		var hud = FastDestroyableSingleton<HudManager>.Instance;

		yield return hud.CoFadeFullScreen(Color.black, Color.clear, 0.2f, false);
		yield return new WaitForSeconds(0.2f);
		if (__instance.exiled != null && __instance.EjectSound)
		{
			var snd = SoundManager.Instance;
			snd.PlayDynamicSound(
				"PlayerEjected",
				__instance.EjectSound, true,
				(DynamicSound.GetDynamicsFunction)__instance.SoundDynamics,
				snd.SfxChannel);
		}
		yield return new WaitForSeconds(0.8f);

		yield return Effects.All(new Il2CppEnumerator[]
		{
			__instance.PlayerSpin(left, right),
			__instance.HandleText(__instance.Duration * 0.3f, __instance.Duration * 0.5f)
		});

		if (GameManager.Instance.LogicOptions.GetConfirmImpostor())
		{
			__instance.ImpostorText.gameObject.SetActive(true);
		}

		yield return Effects.Bloop(0f, __instance.ImpostorText.transform, 1f, 0.5f);
		yield return new WaitForSeconds(0.5f);
		yield return hud.CoFadeFullScreen(Color.clear, Color.black, 0.2f, false);
		yield return ExtremeSpawnSelectorMinigame.WrapUpAndSpawn(__instance);

		yield break;
	}
}
