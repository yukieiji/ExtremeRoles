using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;
using UnityEngine;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;

using Submerged = ExtremeRoles.Compat.ModIntegrator.SubmergedIntegrator;
using SpawnPoint = ExtremeRoles.Compat.ModIntegrator.SubmergedIntegrator.SpawnSetting;


#nullable enable

namespace ExtremeRoles.Compat.Patches;

public static class ExileControllerPatchesPatch
{
	public static bool ExileController_BeginPrefix(ExileController __instance, NetworkedPlayerInfo exiled, bool tie)
	{
		return ExtremeRoles.Patches.Controller.ExileControllerBeginePatch.PrefixRun(__instance, exiled, tie);
	}
}

public static class DisplayPrespawnStepPatchesCustomPrespawnStepPatch
{
	public static bool Prefix(ref IEnumerator __result)
	{
		if (!CompatModManager.Instance.IsModMap<Submerged>() ||
			!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return true; }
		__result = assassinMeetingEnumerator();
		return false;
	}
	public static IEnumerator assassinMeetingEnumerator()
	{
		// 真っ暗になるのでそれを解除する
		var hud = FastDestroyableSingleton<HudManager>.Instance;
		hud.StartCoroutine(hud.CoFadeFullScreen(Color.black, Color.clear, 0.2f));
		yield break;
	}
}

public static class SubmarineSelectOnDestroyPatch
{
	public static void Prefix()
	{
		ExtremeRoles.Patches.Controller.ExileControllerReEnableGameplayPatch.ReEnablePostfix();
	}
}


public static class SubmergedExileControllerWrapUpAndSpawnPatch
{
	public static void Prefix()
	{
		ExtremeRoles.Patches.Controller.ExileControllerWrapUpPatch.WrapUpPrefix();
	}

	public static void Postfix(ExileController __instance)
	{
		ExtremeRoles.Patches.Controller.ExileControllerWrapUpPatch.WrapUpPostfix(
			__instance.exiled);
	}
}

public static class HudManagerUpdatePatchPostfixPatch
{
#pragma warning disable CS8618
	private static FieldInfo floorButtonInfo;
#pragma warning restore CS8618
	private static AspectPosition? posSetter;

	public static void Postfix(object __instance)
	{
		if (!CompatModManager.Instance.IsModMap<Submerged>()) { return; }

		object? buttonOjb = floorButtonInfo.GetValue(__instance);

		if (buttonOjb is GameObject floorButton &&
			floorButton != null &&
			posSetter == null)
		{
			posSetter = floorButton.GetComponent<AspectPosition>();
			Vector3 distanceFromEdge = posSetter.DistanceFromEdge;
			distanceFromEdge.y = 2.4f;
			posSetter.DistanceFromEdge = distanceFromEdge;
			posSetter.AdjustPosition();
		}

	}
	public static void SetType(System.Type type)
	{
		floorButtonInfo = AccessTools.Field(type, "_floorButton");
	}
}

public static class SubmarineSpawnInSystemDetorioratePatch
{
#pragma warning disable CS8618
	private static FieldInfo submarineSpawnInSystemTimer;
#pragma warning restore CS8618

	public static void Postfix(object __instance)
	{
		if (!CompatModManager.Instance.TryGetModMap<Submerged>(out var submergedMod)) { return; }

		var spawnOpt = ExtremeGameModeManager.Instance.ShipOption.Spawn;

		// ランダムスポーンが有効かつ自動選択がオフだけ処理飛ばす
		if (spawnOpt.EnableSpecialSetting &&
			submergedMod.Spawn is
			SpawnPoint.DefaultKey &&
			!spawnOpt.IsAutoSelectRandom) { return; }

		submarineSpawnInSystemTimer.SetValue(__instance, 0.0f);
	}
	public static void SetType(System.Type type)
	{
		submarineSpawnInSystemTimer = AccessTools.Field(type, "timer");
	}
}

public static class SubmarineOxygenSystemDetorioratePatch
{
#pragma warning disable CS8618
	private static FieldInfo submarineOxygenSystemPlayersWithMask;
#pragma warning restore CS8618

	public static void Postfix(object __instance)
	{
		if (!CompatModManager.Instance.TryGetModMap<Submerged>(out var submergedMod) ||
			!RoleAssignState.Instance.IsRoleSetUpEnd ||
			Roles.ExtremeRoleManager.GetLocalPlayerRole().Id != Roles.ExtremeRoleId.Assassin) { return; }

		HashSet<byte>? playersWithMask =
			submarineOxygenSystemPlayersWithMask.GetValue(__instance) as HashSet<byte>;

		if (playersWithMask != null &&
			!playersWithMask.Contains(PlayerControl.LocalPlayer.PlayerId))
		{
			submergedMod.RepairCustomSabotage(
				submergedMod.RetrieveOxygenMask);
		}
	}
	public static void SetType(System.Type type)
	{
		submarineOxygenSystemPlayersWithMask = AccessTools.Field(type, "playersWithMask");
	}
}

public static class SubmarineSurvillanceMinigamePatch
{
#pragma warning disable CS8618
	private static FieldInfo screenStaticInfo;
	private static FieldInfo screenTextInfo;
#pragma warning restore CS8618

	public static bool Prefix(Minigame __instance)
	{
		if (Roles.ExtremeRoleManager.GameRole.Count == 0 ||
			Roles.ExtremeRoleManager.GetLocalPlayerRole().CanUseSecurity()) { return true; }


		GameObject? screenStatic = screenStaticInfo.GetValue(__instance) as GameObject;
		GameObject? screenText = screenTextInfo.GetValue(__instance) as GameObject;

		if (screenStatic != null)
		{
			TMPro.TextMeshPro comText = screenStatic.GetComponentInChildren<TMPro.TextMeshPro>();
			if (comText != null)
			{
				comText.text = Helper.OldTranslation.GetString("youDonotUse");
			}

			screenStatic.SetActive(true);
		}
		if (screenText != null)
		{
			screenText.SetActive(true);
		}

		return false;
	}

	public static void Postfix(Minigame __instance)
	{
		ExtremeRoles.Patches.MiniGame.SecurityHelper.PostUpdate(__instance);

		var timer = ExtremeRoles.Patches.MiniGame.SecurityHelper.GetTimerText();
		if (timer != null)
		{
			timer.gameObject.layer = 5;
			timer.transform.localPosition = new Vector3(15.3f, 9.3f, -900.0f);
			timer.transform.localScale = new Vector3(3.0f, 3.0f, 3.0f);
		}
	}

	public static void SetType(System.Type type)
	{
		screenStaticInfo = AccessTools.Field(type, "screenStatic");
		screenTextInfo = AccessTools.Field(type, "screenText");
	}
}

public static class SubmarineSelectSpawnCoSelectLevelPatch
{
	public static void Prefix(ref bool upperSelected)
	{
		if (!CompatModManager.Instance.TryGetModMap<Submerged>(out var submergedMod)) { return; }

		var spawnOpt = ExtremeGameModeManager.Instance.ShipOption.Spawn;
		var spawnPoint = submergedMod.Spawn;

		if (spawnPoint is SpawnPoint.DefaultKey)
		{
			return;
		}
		else if (!spawnOpt.EnableSpecialSetting || spawnPoint is SpawnPoint.LowerCentralOnly)
		{
			upperSelected = false;
		}
		else if (spawnPoint is SpawnPoint.UpperCentralOnly)
		{
			upperSelected = true;
		}
	}
}
