using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using HarmonyLib;

using UnityEngine;

using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

namespace ExtremeRoles.Patches.Option
{

    /*
    [HarmonyPatch(typeof(GameOptionsData), "GameHostOptions", MethodType.Getter)]
    public static class GameOptionsDataGameHostOptionsPatch
    {
        private static int numImpostors;
        public static void Prefix()
        {
            if (GameOptionsData.hostOptionsData == null)
            {
                GameOptionsData.hostOptionsData = GameOptionsData.LoadGameHostOptions();
            }

            numImpostors = GameOptionsData.hostOptionsData.NumImpostors;
        }

        public static void Postfix(ref GameOptionsData __result)
        {
            __result.NumImpostors = numImpostors;
        }
    }
    */
}
