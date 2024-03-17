using System.Collections;

using BepInEx.Unity.IL2CPP.Utils.Collections;

using UnityEngine;

using HarmonyLib;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;
using ExtremeRoles.Performance;

using Il2CppEnumerator = Il2CppSystem.Collections.IEnumerator;

namespace ExtremeRoles.Patches.Controller;

[HarmonyPatch(typeof(FungleExileController), nameof(SkeldExileController.Animate))]
public static class FungleExileControllerAnimePatch
{
	public static bool Prefix(FungleExileController __instance, ref Il2CppEnumerator __result)
	{
		var spawnOpt = ExtremeGameModeManager.Instance.ShipOption.Spawn;

		if (spawnOpt.EnableSpecialSetting && spawnOpt.Fungle)
		{
			__result = animateWithRandomSpawn(__instance).WrapToIl2Cpp();
			return false;
		}
		return true;
	}

	private static IEnumerator animateWithRandomSpawn(FungleExileController __instance)
	{
		var sound = SoundManager.Instance;
		var hud = FastDestroyableSingleton<HudManager>.Instance;

		sound.PlayNamedSound("ejection_beach_sfx", __instance.ambience, true, SoundManager.Instance.SfxChannel);
		if (__instance.exiled == null)
		{
			__instance.Player.gameObject.SetActive(false);
			__instance.raftAnimation.SetActive(false);
		}
		if (__instance.exiled != null && __instance.EjectSound)
		{
			sound.PlaySound(__instance.EjectSound, false, 1f, SoundManager.Instance.SfxChannel);
		}

		yield return hud.CoFadeFullScreen(Color.black, Color.clear, 0.2f, false);
		yield return Effects.Wait(0.5f);
		yield return Effects.All(new Il2CppEnumerator[]
		{
			__instance.FadeBlackRaftAndPlayer(),
			__instance.HandleText(0.2f, 2f)
		});
		if (GameManager.Instance.LogicOptions.GetConfirmImpostor())
		{
			__instance.ImpostorText.gameObject.SetActive(true);
		}

		yield return Effects.Bloop(0f, __instance.ImpostorText.transform, 1f, 0.5f);
		yield return new WaitForSeconds(2f);
		yield return hud.CoFadeFullScreen(Color.clear, Color.black, 0.2f, false);

		SoundManager.Instance.StopNamedSound("ejection_beach_sfx");
		SoundManager.Instance.StopNamedSound("ejection_fire_sfx");

		yield return ExtremeSpawnSelectorMinigame.WrapUpAndSpawn(__instance);

		yield break;
	}
}
