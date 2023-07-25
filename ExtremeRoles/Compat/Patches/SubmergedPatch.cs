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


#nullable enable

namespace ExtremeRoles.Compat.Patches;

public static class DisplayPrespawnStepPatchesCustomPrespawnStepPatch
{
	public static bool Prefix(ref IEnumerator __result)
	{
		if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return true; }
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

		if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger)
		{
			HudManagerUpdatePatchPostfixPatch.ButtonTriggerReset();
		}
	}
}

public static class HudManagerUpdatePatchPostfixPatch
{
	private static bool changed = false;
#pragma warning disable CS8618
	private static FieldInfo floorButtonInfo;
#pragma warning restore CS8618

	public static void Postfix(object __instance)
	{
		if (!CompatModManager.Instance.IsModMap<Submerged>()) { return; }

		object? buttonOjb = floorButtonInfo.GetValue(__instance);

		if (!Helper.GameSystem.IsFreePlay &&
			buttonOjb is GameObject floorButton &&
			floorButton != null && !changed)
		{
			changed = true;
			floorButton.transform.localPosition -= new Vector3(0.0f, 0.75f, 0.0f);
		}

	}
	public static void SetType(System.Type type)
	{
		changed = false;
		floorButtonInfo = AccessTools.Field(type, "_floorButton");
	}

	public static void ButtonTriggerReset()
	{
		changed = false;
	}
}

public static class SubmarineSpawnInSystemDetorioratePatch
{
#pragma warning disable CS8618
	private static FieldInfo submarineSpawnInSystemTimer;
#pragma warning restore CS8618

	public static void Postfix(object __instance)
	{
		if (!CompatModManager.Instance.IsModMap<Submerged>() ||
			!ExtremeGameModeManager.Instance.ShipOption.IsAutoSelectRandomSpawn) { return; }

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
			!playersWithMask.Contains(CachedPlayerControl.LocalPlayer.PlayerId))
		{
			submergedMod!.RepairCustomSabotage(
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
				comText.text = Helper.Translation.GetString("youDonotUse");
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
