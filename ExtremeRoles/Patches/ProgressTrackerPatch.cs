using System.Linq;

using AmongUs.GameOptions;
using HarmonyLib;

using UnityEngine;

using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(ProgressTracker), nameof(ProgressTracker.FixedUpdate))]
public static class ProgressTrackerFixedUpdatePatch
{
	public static bool Prefix(ProgressTracker __instance)
		=>
			GameManager.Instance != null &&
			GameManager.Instance.LogicOptions != null &&
			__instance.TileParent != null &&
			PlayerControl.LocalPlayer != null;

    public static void Postfix(ProgressTracker __instance)
    {
		if (!(
				GameProgressSystem.IsGameNow && (
				(
					ExtremeRoleManager.TryGetSafeCastedLocalRole<Agency>(out var agency) &&
					agency.CanSeeTaskBar
				) ||
				(
					ExtremeRoleManager.TryGetSafeCastedLocalRole<SlaveDriver>(out var slaveDriver) &&
					slaveDriver.CanSeeTaskBar
				))
			))
		{
			return;
		}

        if (!__instance.TileParent.enabled)
        {
            __instance.TileParent.enabled = true;
        }

        GameData gameData = GameData.Instance;
        if (gameData && gameData.TotalTasks > 0)
        {
            __instance.gameObject.SetActive(true);
            int num = (TutorialManager.InstanceExists ?
                1 : (gameData.PlayerCount - GameOptionsManager.Instance.CurrentGameOptions.GetInt(
                        Int32OptionNames.NumImpostors)));
            num -= gameData.AllPlayers.ToArray().ToList().Count(
                (NetworkedPlayerInfo p) => p.Disconnected);

            float curProgress = (float)gameData.CompletedTasks /
				(float)gameData.TotalTasks * (float)num;
            __instance.curValue = Mathf.Lerp(
                __instance.curValue, curProgress, Time.fixedDeltaTime * 2f);
            __instance.TileParent.material.SetFloat("_Buckets", (float)num);
            __instance.TileParent.material.SetFloat("_FullBuckets", __instance.curValue);
        }
    }
}
