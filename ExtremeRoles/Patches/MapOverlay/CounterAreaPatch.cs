using System.Collections.Generic;

using UnityEngine;

using HarmonyLib;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Patches.MapOverlay;

[HarmonyPatch(typeof(CounterArea), nameof(CounterArea.UpdateCount))]
public static class CounterAreaUpdateCountPatch
{
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
                if (icon.TryGetComponent<SpriteRenderer>(out var renderer))
                {
                    renderer.material = defaultMat;
                }
            }
            return;
        }

        if (!MapCountOverlayUpdatePatch.PlayerColor.TryGetValue(
                __instance.RoomType, out List<int> colors))
        {
            return;
        }

        for (int i = 0; i < __instance.myIcons.Count; i++)
        {
            PoolableBehavior icon = __instance.myIcons[i];

            if (icon.TryGetComponent<SpriteRenderer>(out var renderer) &&
                colors.Count > i)
            {
                renderer.material = Object.Instantiate(defaultMat);
				PlayerMaterial.SetColors(colors[i], renderer);
            }
        }
    }
}
