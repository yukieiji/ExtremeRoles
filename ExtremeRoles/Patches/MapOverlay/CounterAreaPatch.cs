using System.Collections.Generic;

using UnityEngine;

using HarmonyLib;

namespace ExtremeRoles.Patches.MapOverlay
{
    [HarmonyPatch(typeof(CounterArea), nameof(CounterArea.UpdateCount))]
    public static class CounterAreaUpdateCountPatch
    {
        private static Material defaultMat;
        private static Material newMat;

        public static void Postfix(CounterArea __instance)
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return; }

            var admin = Roles.ExtremeRoleManager.GetSafeCastedLocalPlayerRole<
                Roles.Solo.Crewmate.Supervisor>();

            if (admin == null || !admin.Boosted) { return; }

            if (MapCountOverlayUpdatePatch.PlayerColor.ContainsKey(__instance.RoomType))
            {
                List<Color> colors = MapCountOverlayUpdatePatch.PlayerColor[__instance.RoomType];
              
                for (int i = 0; i < __instance.myIcons.Count; i++)
                {
                    PoolableBehavior icon = __instance.myIcons[i];
                    SpriteRenderer renderer = icon.GetComponent<SpriteRenderer>();

                    if (renderer != null)
                    {
                        if (defaultMat == null)
                        {
                            defaultMat = renderer.material;
                        }
                        if (newMat == null)
                        {
                            newMat = Object.Instantiate(defaultMat);
                        }
                        if (colors.Count > i)
                        {
                            renderer.material = newMat;
                            var color = colors[i];
                            renderer.material.SetColor("_BodyColor", color);
                            var id = Palette.PlayerColors.IndexOf(color);
                            if (id < 0)
                            {
                                renderer.material.SetColor("_BackColor", color);
                            }
                            else
                            {
                                renderer.material.SetColor("_BackColor", Palette.ShadowColors[id]);
                            }
                            renderer.material.SetColor("_VisorColor", Palette.VisorColor);
                        }
                    }
                }
            }
        }
    }
}
