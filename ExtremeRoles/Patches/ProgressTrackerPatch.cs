using System.Linq;

using UnityEngine;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Crewmate;

using HarmonyLib;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(ProgressTracker), nameof(ProgressTracker.FixedUpdate))]
    public static class ProgressTrackerFixedUpdatePatch
    {
        public static void Postfix(ProgressTracker __instance)
        {
            Agency agency = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<Agency>();

            if (agency != null)
            {
                GameData instance = GameData.Instance;
                if (instance && instance.TotalTasks > 0)
                {
                    __instance.gameObject.SetActive(true);
                    int num = (DestroyableSingleton<TutorialManager>.InstanceExists ? 1 : 
                        (instance.AllPlayers.Count - PlayerControl.GameOptions.NumImpostors));
                    num -= instance.AllPlayers.ToArray().ToList().Count(
                        (GameData.PlayerInfo p) => p.Disconnected);

                    float curProgress = (float)instance.CompletedTasks / (float)instance.TotalTasks * (float)num;
                    __instance.curValue = Mathf.Lerp(
                        __instance.curValue, curProgress, Time.fixedDeltaTime * 2f);
                    __instance.TileParent.material.SetFloat("_Buckets", (float)num);
                    __instance.TileParent.material.SetFloat("_FullBuckets", __instance.curValue);
                }
            }
        }
    }
}
