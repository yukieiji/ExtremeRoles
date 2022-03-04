using System;
using System.IO;
using System.Reflection;
using System.Linq;
using HarmonyLib;



namespace ExtremeRoles.Patches.Manager
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class MainMenuManagerStartPatch
    {
        public static void Postfix(MainMenuManager __instance)
        {
            ExtremeSkins.ExtremeHatManager.CheckUpdate();




            ExtremeSkins.ExtremeHatManager.Load();
        }
    }
}
