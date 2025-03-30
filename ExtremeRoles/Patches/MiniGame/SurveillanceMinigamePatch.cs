using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using HarmonyLib;

using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

using ExtremeRoles.Module.SystemType.SecurityDummySystem;

namespace ExtremeRoles.Patches.MiniGame;

#nullable enable

public static class SecurityHelper
{
    private static float cameraTimer = 0.0f;
    private static TMPro.TextMeshPro? timerText;

    private static readonly IReadOnlySet<ExtremeRoleId> securityUseRole =
        new HashSet<ExtremeRoleId>()
    {
        ExtremeRoleId.Traitor,
        ExtremeRoleId.Watchdog,
        ExtremeRoleId.Doll
    };

    public static void LoadOptionValue()
    {
        var securityOption = ExtremeGameModeManager.Instance.ShipOption.Security;

        cameraTimer = securityOption.LimitTime;

        Logging.Debug("---- SecurityCondition ----");
        Logging.Debug($"IsRemoveSecurity:{securityOption.Disable}");
        Logging.Debug($"EnableSecurityLimit:{securityOption.EnableLimit}");
        Logging.Debug($"SecurityTime:{cameraTimer}");
    }

    public static void PostUpdate(Minigame instance)
    {

        if (ExtremeRoleManager.GameRole.Count == 0) { return; }

		var securityOpt = ExtremeGameModeManager.Instance.ShipOption.Security;

		if (securityOpt.Disable || // セキュリティ無効化してる
            !securityOpt.EnableLimit ||  // セキュリティ制限あるか
			IsAbilityUse())
        {
            return;
        }

        if (timerText == null)
        {
            timerText = Object.Instantiate(
                HudManager.Instance.KillButton.cooldownTimerText,
                instance.transform);
            timerText.transform.localPosition = new Vector3(3.4f, 2.7f, -9.0f);
            timerText.name = "securityTimer";
        }

        if (cameraTimer > 0.0f)
        {
            cameraTimer -= Time.deltaTime;
        }

        timerText.text = $"{Mathf.CeilToInt(cameraTimer)}";
        timerText.gameObject.SetActive(true);

        if (cameraTimer <= 0.0f)
        {
			Map.DisableSecurity();
            instance.ForceClose();
        }
    }
	public static bool IsAbilityUse()
		=> IRoleAbility.IsLocalPlayerAbilityUse(securityUseRole);

	public static TMPro.TextMeshPro? GetTimerText() => timerText;
}

[HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
public static class SurveillanceMinigameBeginPatch
{
    public static void Postfix(SurveillanceMinigame __instance)
    {
        SurveillanceMinigameUpdatePatch.Timer = SurveillanceMinigameUpdatePatch.ChangeTime;
        SurveillanceMinigameUpdatePatch.Page = 0;

        if (ShipStatus.Instance.AllCameras.Length > 4 && __instance.FilteredRooms.Length > 0)
        {
            __instance.textures = __instance.textures.ToList().Concat(
                new RenderTexture[ShipStatus.Instance.AllCameras.Length - 4]).ToArray();
            __instance.ViewPorts = __instance.ViewPorts.ToList().Concat(
                new MeshRenderer[ShipStatus.Instance.AllCameras.Length - 4]).ToArray();
            for (int i = 4; i < ShipStatus.Instance.AllCameras.Length; i++)
            {
                SurvCamera surv = ShipStatus.Instance.AllCameras[i];
                Camera camera = Object.Instantiate(__instance.CameraPrefab);
                camera.transform.SetParent(__instance.transform);
                camera.transform.position = new Vector3(
                    surv.transform.position.x,
                    surv.transform.position.y, 8f);
                camera.orthographicSize = 2.35f;
                RenderTexture temporary = RenderTexture.GetTemporary(256, 256, 16, (RenderTextureFormat)0);
                __instance.textures[i] = temporary;
                camera.targetTexture = temporary;
                __instance.ViewPorts[i].material.SetTexture("_MainTex", temporary);
            }
        }

		if (SecurityDummySystemManager.TryGet(out var system) &&
			system.IsActive)
		{
			system.PostfixBegin();
		}
	}
}

[HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Close))]
public static class SurveillanceMinigameClosePatch
{
	public static void Postfix()
	{
		if (SecurityDummySystemManager.TryGet(out var system) &&
			system.IsActive)
		{
			system.PostfixClose();
		}
	}
}

[HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
public static class SurveillanceMinigameUpdatePatch
{
    public const float ChangeTime = 4.0f;
    public static float Timer;
    public static int Page;

    public static bool Prefix(SurveillanceMinigame __instance)
    {
        if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

        if (ExtremeRoleManager.GetLocalPlayerRole().CanUseSecurity() ||
            SecurityHelper.IsAbilityUse())
        {
			// Update内でやっている処理に特殊な処理がなくサボの処理だけなのでオーバーライドしなくてよし
			updateCamera(__instance);
            return false;
        }

        __instance.isStatic = true;
        for (int i = 0; i < __instance.ViewPorts.Length; ++i)
        {
            __instance.ViewPorts[i].sharedMaterial = __instance.StaticMaterial;
			var text = __instance.SabText[i];
			text.text = Tr.GetString("youDonotUse");
			text.gameObject.SetActive(true);
        }

        return false;
    }

    private static void updateCamera(SurveillanceMinigame instance)
    {
        Timer -= Time.deltaTime;
        int numberOfPages = Mathf.CeilToInt(ShipStatus.Instance.AllCameras.Length / 4f);

        bool update = false;

        if (Timer < 0.0f || Input.GetKeyDown(KeyCode.RightArrow))
        {
            update = true;
            Timer = ChangeTime;
            Page = (Page + 1) % numberOfPages;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Page = (Page + numberOfPages - 1) % numberOfPages;
            update = true;
            Timer = ChangeTime;
        }

		bool isSab = PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(
			PlayerControl.LocalPlayer);


		if ((instance.isStatic || update) && !isSab)
        {
            instance.isStatic = false;
            for (int i = 0; i < instance.ViewPorts.Length; i++)
            {
				var port = instance.ViewPorts[i];
				var text = instance.SabText[i];

				port.sharedMaterial = instance.DefaultMaterial;
				text.gameObject.SetActive(false);
                if (Page * 4 + i < instance.textures.Length)
                {
					port.material.SetTexture(
                        "_MainTex", instance.textures[Page * 4 + i]);
                }
                else
                {
                    port.sharedMaterial = instance.StaticMaterial;
                }
            }
        }
        else if (!instance.isStatic && isSab)
        {
            instance.isStatic = true;
            for (int j = 0; j < instance.ViewPorts.Length; j++)
            {
                instance.ViewPorts[j].sharedMaterial = instance.StaticMaterial;
                instance.SabText[j].gameObject.SetActive(true);
            }
        }
    }

    public static void Postfix(SurveillanceMinigame __instance)
    {
        SecurityHelper.PostUpdate(__instance);
    }
}
