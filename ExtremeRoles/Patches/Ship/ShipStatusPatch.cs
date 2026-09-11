using Il2CppSystem.Collections;

using HarmonyLib;
using BepInEx.Unity.IL2CPP.Utils.Collections;

using ExtremeRoles.Compat;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;
using ExtremeRoles.Performance;
using ExtremeRoles.Core.Abstract;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace ExtremeRoles.Patches.Ship;

public class ShipStatusOnEnablePatchBody(IGameRuntime runtime)
{
	private readonly IGameRuntime _runtime = runtime;
	private static ShipStatusOnEnablePatchBody? body;

	public static void StaticPostfix(ShipStatus __instance)
	{
		if (body is null)
		{
			body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<ShipStatusOnEnablePatchBody>();
		}
		body.Postfix(__instance);
	}

	private void Postfix(ShipStatus __instance)
	{
		if (_runtime.TryGetGameContext(out var ctx))
		{
			ctx.GlobalOption.Emergency.ChangeTime(__instance);
		}
	}
}

public class ShipStatusPrespawnStepPatchBody(IGameRuntime runtime)
{
	private readonly IGameRuntime _runtime = runtime;

	public bool Postfix(ref IEnumerator __result)
	{
		var gmg = GameManager.Instance;
		if (gmg == null ||
			gmg.LogicOptions == null ||
			!_runtime.TryGetGameContext(out var ctx))
		{
			return true;
		}

		var spawnOpt = ctx.GlobalOption.Spawn;
		if (!spawnOpt.EnableSpecialSetting)
		{
			return true;
		}

		if (gmg.LogicOptions.MapId switch
		{
			0 => spawnOpt.Skeld,
			1 => spawnOpt.MiraHq,
			2 => spawnOpt.Polus,
			5 => spawnOpt.Fungle,
			_ => false,
		})
		{
			__result = ExtremeSpawnSelectorMinigame.WaiteSpawn().WrapToIl2Cpp();
			return false;
		}
		return true;
	}
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
public static class ShipStatusAwakePatch
{
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void Postfix(ShipStatus __instance)
    {
		ShipStatusCache.SetUp(__instance);
        CompatModManager.Instance.SetUpMap(__instance);
	}
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnEnable))]
public static class ShipStatusOnEnablePatch
{
	public static void Postfix(ShipStatus __instance)
	{
		ShipStatusOnEnablePatchBody.StaticPostfix(__instance);
	}
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
public static class ShipStatusCalculateLightRadiusPatch
{
    public static bool Prefix(
        ref float __result,
        ShipStatus __instance,
        [HarmonyArgument(0)] NetworkedPlayerInfo playerInfo)
		=> ExtremeVisionModder.Instance.IsVanillaVisionAndGetVision(__instance, playerInfo, out __result);

}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnDestroy))]
public static class ShipStatusOnDestroyPatch
{
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void Postfix()
    {
        ShipStatusCache.Destroy();
		CompatModManager.Instance.RemoveMap();
		ExtremeSystemTypeManager.Instance.RemoveSystem();
	}
}


[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.PrespawnStep))]
public static class ShipStatusPrespawnStepPatch
{
	private static ShipStatusPrespawnStepPatchBody? _body;

	public static bool Prefix(ref IEnumerator __result)
	{
		if (_body is null)
		{
			_body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<ShipStatusPrespawnStepPatchBody>();
		}
		return _body.Postfix(ref __result);
	}
}