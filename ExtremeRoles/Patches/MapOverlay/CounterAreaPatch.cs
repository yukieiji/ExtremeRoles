using System.Collections.Generic;

using UnityEngine;

using HarmonyLib;
using ExtremeRoles.Performance.Il2Cpp;

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

            if (admin is null) { return; }

            PoolableBehavior pool = __instance.pool.Get<PoolableBehavior>();
            SpriteRenderer defaultRenderer = pool.GetComponent<SpriteRenderer>();
            Material defaultMat = Object.Instantiate(defaultRenderer.material);
            pool.OwnerPool.Reclaim(pool);

            if (!admin.Boosted || !admin.IsAbilityActive)
            {
                foreach (PoolableBehavior icon in __instance.myIcons.GetFastEnumerator())
                {
                    SpriteRenderer renderer = icon.GetComponent<SpriteRenderer>();

                    if (renderer is not null)
                    {
                        renderer.material = defaultMat;
                    }
                }
                return; 
            }

            if (!MapCountOverlayUpdatePatch.PlayerColor.TryGetValue(
                    __instance.RoomType, out List<Color> colors))
            {
                return;
            }

            for (int i = 0; i < __instance.myIcons.Count; i++)
            {
                PoolableBehavior icon = __instance.myIcons[i];
                SpriteRenderer renderer = icon.GetComponent<SpriteRenderer>();

                if (renderer is not null && colors.Count > i)
                {
                    renderer.material = Object.Instantiate(defaultMat);
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
