using System;
using System.Linq;

using UnityEngine;

using HarmonyLib;

namespace ExtremeRoles.Patches.Option
{
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
    class GameSettingMenuStartPatch
    {
        public static void Prefix(GameSettingMenu __instance)
        {
            __instance.HideForOnline = new Transform[] { };
        }

        public static void Postfix(GameSettingMenu __instance)
        {
            // Setup mapNameTransform
            var mapNameTransform = __instance.AllItems.FirstOrDefault(
                x => x.name.Equals("MapName", StringComparison.OrdinalIgnoreCase));

            if (mapNameTransform == null) { return; }

            var options = new Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.KeyValuePair<string, int>>();
            for (int i = 0; i < Constants.MapNames.Length; ++i)
            {
                var kvp = new Il2CppSystem.Collections.Generic.KeyValuePair<string, int>();
                kvp.key = Constants.MapNames[i];
                kvp.value = i;
                options.Add(kvp);
            }
            mapNameTransform.GetComponent<KeyValueOption>().Values = options;
            mapNameTransform.gameObject.SetActive(true);

            if (AmongUsClient.Instance.GameMode != GameModes.OnlineGame) { return; }

            foreach (Transform opt in __instance.AllItems.ToList())
            {
                float offset = -0.5f;
                string name = opt.name;

                if (name.Equals("MapName", StringComparison.OrdinalIgnoreCase))
                {
                    offset = 0.25f;
                }
                if (name.Equals("ResetToDefault", StringComparison.OrdinalIgnoreCase))
                {
                    offset = 0f;
                }
                opt.position += new Vector3(0, offset, 0);
            }

            __instance.Scroller.ContentYBounds.max += 0.5F;


        }
    }
}
