using UnityEngine;
using System.Collections;

using BepInEx.Unity.IL2CPP.Utils.Collections;

using ExtremeRoles.Performance;

using Il2CppEnumerator = Il2CppSystem.Collections.IEnumerator;
using HarmonyLib;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigame;

namespace ExtremeRoles.Patches.Controller;

public static class RandomSpawn
{
	public static IEnumerator WrapUpAndSpawn(ExileController instance)
	{
		ExileControllerWrapUpPatch.WrapUpPrefix();
		if (instance.exiled != null)
		{
			PlayerControl @object = instance.exiled.Object;
			if (@object)
			{
				@object.Exiled();
			}
			instance.exiled.IsDead = true;
		}
		ExileControllerWrapUpPatch.WrapUpPostfix(instance.exiled);

		bool meeting = ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger;
		if (meeting)
		{
			yield break;
		}

		if (DestroyableSingleton<TutorialManager>.InstanceExists ||
			!GameManager.Instance.LogicFlow.IsGameOverDueToDeath())
		{
			yield return WaiteSpawn();
			instance.ReEnableGameplay();
		}
		Object.Destroy(instance.gameObject);
		yield break;
	}

	private static IEnumerator WaiteSpawn()
	{
		GameObject obj = new GameObject("SpawnSelector");
		var spawnSelector = obj.AddComponent<ExtremeSpawnSelectorMinigame>();
		spawnSelector.transform.SetParent(Camera.main.transform, false);
		spawnSelector.transform.localPosition = new Vector3(0f, 0f, -600f);

		spawnSelector.Begin(null);

		yield return spawnSelector.WaitForFinish();
		yield break;
	}
}

[HarmonyPatch(typeof(SkeldExileController), nameof(SkeldExileController.Animate))]
public static class SkeldExileControllerAnimePatch
{
	public static bool Prefix(SkeldExileController __instance, ref Il2CppEnumerator __result)
	{
		__result = randomSpanw(__instance).WrapToIl2Cpp();
		return false;
	}

	private static IEnumerator randomSpanw(SkeldExileController __instance)
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
			SoundManager.Instance.PlayDynamicSound(
				"PlayerEjected",
				__instance.EjectSound, true,
				(DynamicSound.GetDynamicsFunction)__instance.SoundDynamics,
				SoundManager.Instance.SfxChannel);
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
		yield return RandomSpawn.WrapUpAndSpawn(__instance);
		yield break;
	}
}
