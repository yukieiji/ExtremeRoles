using UnityEngine;

using HarmonyLib;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Core.Abstract;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace ExtremeRoles.Patches.MapOverlay;

public class CounterAreaUpdatePatchBody(IGameRuntime runtime, MapCountOverlayUpdatePatchBoidy countOverlayUpdatePatchBoidy)
{
	private readonly IGameRuntime _runtime = runtime;
	private readonly MapCountOverlayUpdatePatchBoidy _countOverlayUpdatePatchBoidy = countOverlayUpdatePatchBoidy;

	public void Postfix(CounterArea __instance)
	{
		if (!_runtime.TryGetGameContext(out var ctx) ||
			ctx.Roles.All.Count == 0) // <= ここは後でRoleAssignStateに置き換える
		{
			return;
		}

		var admin = ctx.Roles.GetSafeCastedLocalPlayerRole<Roles.Solo.Crewmate.Supervisor>();
		if (admin is null)
		{
			return;
		}
		var pool = __instance.pool.Get<PoolableBehavior>();
		if (!pool.TryGetComponent<SpriteRenderer>(out var defaultRenderer))
		{
			return;
		}

		var defaultMat = Object.Instantiate(defaultRenderer.material);
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
		if (!_countOverlayUpdatePatchBoidy.PlayerColors.TryGetValue(
				__instance.RoomType, out var colors))
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


[HarmonyPatch(typeof(CounterArea), nameof(CounterArea.UpdateCount))]
public static class CounterAreaUpdateCountPatch
{
	private static CounterAreaUpdatePatchBody? body;

    public static void Postfix(CounterArea __instance)
    {
		if (body is null)
		{
			body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<CounterAreaUpdatePatchBody>();
		}
		body.Postfix(__instance);
	}
}
