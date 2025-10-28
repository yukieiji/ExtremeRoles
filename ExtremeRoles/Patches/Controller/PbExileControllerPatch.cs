using System.Collections;

using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using UnityEngine;

using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles;

using Il2CppEnumerator = Il2CppSystem.Collections.IEnumerator;

namespace ExtremeRoles.Patches.Controller;

[HarmonyPatch(typeof(PbExileController), nameof(PbExileController.Animate))]
public static class PbExileControllerAnimePatch
{
	public static bool Prefix(PbExileController __instance, ref Il2CppEnumerator __result)
	{
		GameProgressSystem.Current = GameProgressSystem.Progress.Exiled;

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
		var hud = HudManager.Instance;

		if (hud != null)
		{
			yield return hud.CoFadeFullScreen(Color.black, Color.clear, 0.2f, false);
		}
		yield return Effects.Wait(0.75f);
		yield return Effects.All(
		[
			__instance.PlayerFall(),
			__instance.PlayerSpin(),
			__instance.HandleText(__instance.Duration * 0.5f, __instance.Duration * 0.5f)
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
		if (__instance.finalSinkCoroutine != null)
		{
			__instance.StopCoroutine(__instance.finalSinkCoroutine);
		}
		yield return ExtremeSpawnSelectorMinigame.WrapUpAndSpawn(__instance);

		yield break;
	}
}

[HarmonyPatch(typeof(PbExileController), nameof(PbExileController.PlayerSpin))]
public static class PbExileControllerPlayerSpinPatch
{
	public static bool Prefix(PbExileController __instance, ref Il2CppEnumerator __result)
	{
		var exileOption = ExtremeGameModeManager.Instance.ShipOption.Exile;
		if (!GameProgressSystem.IsRoleSetUpEnd ||
			!__instance.initData.confirmImpostor ||
			exileOption.Mode is ConfirmExileMode.Impostor)
		{
			return true;
		}

		__result = prefixPlayerSpin(__instance, exileOption.Mode).WrapToIl2Cpp();

		return false;
	}

	private static IEnumerator prefixPlayerSpin(PbExileController instance, ConfirmExileMode mode)
	{
		float d = instance.Duration / 1.8f;
		for (float t = 0f; t <= d; t += Time.deltaTime)
		{
			float num2 = t / d;
			float num3 = (t + 0.75f) * 25f / Mathf.Exp(t * 0.75f + 1f);
			instance.Player.transform.Rotate(new Vector3(0f, 0f, num3 * Time.deltaTime * 5f));
			yield return null;
		}

		var initData = instance.initData;
		if (initData != null &&
			initData.outfit != null &&
			initData.networkedPlayer != null &&
			ExtremeRoleManager.TryGetRole(initData.networkedPlayer.PlayerId, out var role))
		{
			bool isConfirmedTeam = mode switch
			{
				ConfirmExileMode.Neutral => role.IsNeutral(),
				ConfirmExileMode.Crewmate => role.IsCrewmate(),
				ConfirmExileMode.AllTeam => true,
				_ => false
			};

			instance.HandSlot.sprite = isConfirmedTeam ? instance.GoodHand : instance.BadHand;
			PlayerMaterial.SetColors(initData.outfit.ColorId, instance.HandSlot);
		}

		var player = instance.Player;
		player.transform.eulerAngles = new Vector3(0f, 0f, -10f);
		float num4 = instance.Duration / 4f;

		float num = Camera.main.orthographicSize + 1f;
		Vector2 top = Vector2.up * num;
		Vector2 bottom = Vector2.down * 2.81f;
		top.y = -1.78f;
		yield return Effects.Overlerp(num4, (Il2CppSystem.Action<float>)((p) =>
		{
			player.transform.localPosition = Vector2.LerpUnclamped(bottom, top, p);
		}), 0.05f);

		float d2 = instance.Duration / 2f;

		for (float t = 0f; t <= d2; t += Time.deltaTime)
		{
			float num5 = t / d2;
			Vector2 vector = Vector2.Lerp(top, bottom, num5);
			vector += Random.insideUnitCircle * 0.025f;
			player.transform.localPosition = vector;
			player.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(-10f, 17f, num5));
			yield return null;
		}
		instance.finalSinkCoroutine = instance.CoFinalSink();
		instance.StartCoroutine(instance.finalSinkCoroutine);

	}
}
