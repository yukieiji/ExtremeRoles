using HarmonyLib;
using UnityEngine;

namespace ExtremeRoles.Patches.Manager
{
    [HarmonyPatch(typeof(MMOnlineManager), nameof(MMOnlineManager.Start))]
    public static class MMOnlineManagerStartPatch
    {
        public static void Postfix(MMOnlineManager __instance)
        {
            if (Module.Prefab.HelpButton == null)
            {
                GameObject button = GameObject.Find("HelpButton");

                Module.Prefab.HelpButton = Object.Instantiate(
                    button);
                Object.DontDestroyOnLoad(Module.Prefab.HelpButton);
                Module.Prefab.HelpButton.name = "HelpButtonPrefab";
                Module.Prefab.HelpButton.gameObject.SetActive(false);
            }
        }
    }
}
