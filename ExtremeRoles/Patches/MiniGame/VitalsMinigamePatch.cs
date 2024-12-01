using System.Collections.Generic;
using HarmonyLib;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.RoleAssign;

#nullable enable

namespace ExtremeRoles.Patches.MiniGame;

[HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
public static class VitalsMinigameBeginPatch
{
	public static void Postfix()
	{
		if (VitalDummySystem.TryGet(out var system))
		{
			system.VitalBeginPostfix();
		}
	}
}

[HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
public static class VitalsMinigameUpdatePatch
{
    private static float vitalTimer = 0.0f;
    private static TMPro.TextMeshPro? timerText;

    private static readonly IReadOnlySet<ExtremeRoleId> vitalUseRole = new HashSet<ExtremeRoleId>()
    {
        ExtremeRoleId.Traitor,
        ExtremeRoleId.Doll
    };

    public static bool Prefix(VitalsMinigame __instance)
    {
        if (!RoleAssignState.Instance.IsRoleSetUpEnd ||
			ExtremeRoleManager.GetLocalPlayerRole().CanUseVital() ||
			IRoleAbility.IsLocalPlayerAbilityUse(vitalUseRole))
		{
			if (VitalDummySystem.TryGet(out var system) &&
				system.IsActive)
			{
				system.OverrideVitalUpdate(__instance);
				return false;
			}

			return true;
		}

        __instance.SabText.text = Tr.GetString("youDonotUse");

        __instance.SabText.gameObject.SetActive(true);
        for (int j = 0; j < __instance.vitals.Length; j++)
        {
            __instance.vitals[j].gameObject.SetActive(false);
        }

        return false;
    }

    public static void Postfix(VitalsMinigame __instance)
    {

        if (ExtremeRoleManager.GameRole.Count == 0) { return; }

		var vitalOption = ExtremeGameModeManager.Instance.ShipOption.Vital;

		if (vitalOption.Disable || // バイタル無効化してる
            !vitalOption.EnableLimit || //バイタル制限あるか
            __instance.BatteryText.gameObject.active ||  //科学者の能力使用か
			IRoleAbility.IsLocalPlayerAbilityUse(vitalUseRole))
        {
            return;
        }

        if (timerText == null)
        {
            timerText = Object.Instantiate(
                FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
                __instance.transform);
            timerText.transform.localPosition = new Vector3(3.4f, 2.7f, -9.0f);
            timerText.name = "vitalTimer";
        }

        if (vitalTimer > 0.0f)
        {
            vitalTimer -= Time.deltaTime;
        }

        timerText.text = $"{Mathf.CeilToInt(vitalTimer)}";
        timerText.gameObject.SetActive(true);

        if (vitalTimer <= 0.0f)
        {
			Map.DisableVital();
            __instance.ForceClose();
        }
    }

    public static void Initialize()
    {
        Object.Destroy(timerText);
    }

    public static void LoadOptionValue()
    {
        var vitalOption = ExtremeGameModeManager.Instance.ShipOption.Vital;

        vitalTimer = vitalOption.LimitTime;

        Logging.Debug("---- VitalCondition ----");
        Logging.Debug($"IsRemoveVital:{vitalOption.Disable}");
        Logging.Debug($"EnableVitalLimit:{vitalOption.EnableLimit}");
        Logging.Debug($"VitalTime:{vitalTimer}");
    }
}
