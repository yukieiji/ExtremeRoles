using System.Linq;

using UnityEngine;
using HarmonyLib;
using AmongUs.GameOptions;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;


namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(ProgressTracker), nameof(ProgressTracker.FixedUpdate))]
public static class ProgressTrackerFixedUpdatePatch
{
    public static void Postfix(ProgressTracker __instance)
    {
        if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return; }

        Agency agency = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<Agency>();
		SlaveDriver slaveDriver = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<SlaveDriver>();

		if ((agency is null && slaveDriver is null) ||
			(agency is not null && !agency.CanSeeTaskBar) ||
			(slaveDriver is not null && !slaveDriver.CanSeeTaskBar)) { return; }

        if (!__instance.TileParent.enabled)
        {
            __instance.TileParent.enabled = true;
        }

        GameData gameData = GameData.Instance;
        if (gameData && gameData.TotalTasks > 0)
        {
            __instance.gameObject.SetActive(true);
            int num = (DestroyableSingleton<TutorialManager>.InstanceExists ?
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
