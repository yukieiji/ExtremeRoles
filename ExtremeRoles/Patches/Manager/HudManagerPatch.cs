using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using TMPro;
using HarmonyLib;

using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.InGameVisualUpdater;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.CheckPoint;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Patches.MapOverlay;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.Solo.Host;

using PlayerHelper = ExtremeRoles.Helper.Player;

#nullable enable

namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.OnGameStart))]
public static class HudManagerOnGameStartPatch
{
	public static void Postfix()
	{
		if (PlayerControl.LocalPlayer == null)
		{
			return;
		}

		var gameStart = ExtremeGameModeManager.Instance.ShipOption.GameStart;

		if (!gameStart.IsKillCoolDownIsTen)
		{
			if (GameProgressSystem.IsTaskPhase)
			{
				var role = ExtremeRoleManager.GetLocalPlayerRole();
				PlayerControl.LocalPlayer.SetKillTimer(
					role.TryGetKillCool(out float killCoolTime) ?
					killCoolTime : PlayerHelper.DefaultKillCoolTime);
			}

			if (gameStart.RemoveSomeoneButton)
			{
				RemoveMeetingNumCheckpoint.RpcCheckpoint(gameStart.ReduceNum);
			}
		}
		ShipStatus.Instance.Timer = 15 - gameStart.FirstButtonCoolDown;
	}
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.SetAlertOverlay))]
public static class HudManagerSetAlertOverlayPatch
{
	public static bool Prefix()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null ||
			!GameProgressSystem.IsGameNow)
		{
			return true;
		}
		return Xion.PlayerId != localPlayer.PlayerId;
	}
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.SetTouchType))]
public static class HudManagerSetTouchTypePatch
{
	public static void Postfix()
	{
		if (GameSystem.IsFreePlay)
		{
			InfoOverlay.Instance.InitializeToGame();
		}
	}
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class HudManagerStartPatch
{
    public static void Postfix()
    {
		InfoOverlay.Instance.InitializeToLobby();
	}
}


[HarmonyPatch(typeof(HudManager), nameof(HudManager.ToggleMapVisible))]
public static class HudManagerToggleMapVisibletPatch
{
	public static void Prefix([HarmonyArgument(0)] ref MapOptions options)
	{
		if (options.Mode != MapOptions.Modes.CountOverlay ||
		   ExtremeRoleManager.GameRole.Count == 0 ||
		   ExtremeRoleManager.GetLocalPlayerRole().CanUseAdmin() ||
		   MapCountOverlayUpdatePatch.IsAbilityUse()) { return; }

		options.Mode = MapOptions.Modes.Normal;
	}
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManagerUpdatePatch
{
    private static bool isActiveUpdate = true;

	private static List<InGameVisualUpdatorBase> allUpdator = [];

    public static void Reset()
    {
        allUpdator.Clear();
        isActiveUpdate = true;
    }

    public static void SetBlockUpdate(bool isBlock)
    {
        isActiveUpdate = !isBlock;
    }

    public static bool Prefix(HudManager __instance)
    {
        if (OnemanMeetingSystemManager.IsActive)
        {
            __instance.UseButton.ToggleVisible(false);
            __instance.AbilityButton.ToggleVisible(false);
            __instance.ReportButton.ToggleVisible(false);
            __instance.KillButton.ToggleVisible(false);
            __instance.SabotageButton.ToggleVisible(false);
            __instance.ImpostorVentButton.ToggleVisible(false);
            __instance.TaskPanel.gameObject.SetActive(false);
            __instance.roomTracker.gameObject.SetActive(false);

            IVirtualJoystick virtualJoystick = __instance.joystick;

            if (virtualJoystick != null)
            {
                virtualJoystick.ToggleVisuals(false);
            }
        }
        return isActiveUpdate;
    }

    public static void Postfix()
    {
		var local = PlayerControl.LocalPlayer;

		if (!GameProgressSystem.IsGameNow || local == null)
		{
			return;
		}

		if (allUpdator.Count == 0)
		{
			foreach (var pc in PlayerCache.AllPlayerControl)
			{
				allUpdator.Add(
					pc.PlayerId == local.PlayerId ?
						new LocalPlayerVisualUpdator(local) :
						new OtherPlayerVisualUpdator(local, pc));
			}
		}

		foreach (var updator in allUpdator)
		{
			updator.Update();
		}
    }

	public static bool TryGetRoleInfo(byte playerId, [NotNullWhen(true)] out TextMeshPro? text)
	{
		text = null;
		var target = allUpdator.FirstOrDefault(x => x.PlayerId == playerId);
		if (target == null)
		{
			return false;
		}
		text = target.Info;
		return true;
	}
}
