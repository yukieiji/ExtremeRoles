using Il2CppSystem.Collections;

using HarmonyLib;
using BepInEx.Unity.IL2CPP.Utils.Collections;

using ExtremeRoles.Compat;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;
using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Patches.Ship;

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
		ExtremeGameModeManager.Instance.ShipOption.Emergency.ChangeTime(__instance);
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
	public static bool Prefix(ref IEnumerator __result)
	{
		var spawnOpt = ExtremeGameModeManager.Instance.ShipOption.Spawn;
		if (!spawnOpt.EnableSpecialSetting)
		{
			GameProgressSystem.Current = GameProgressSystem.Progress.Task;
			return true;
		}

		bool enableRandomSpawn = GameManager.Instance.LogicOptions.MapId switch
		{
			0 => spawnOpt.Skeld,
			1 => spawnOpt.MiraHq,
			2 => spawnOpt.Polus,
			5 => spawnOpt.Fungle,
			_ => false,
		};

		if (enableRandomSpawn)
		{
			__result = ExtremeSpawnSelectorMinigame.WaiteSpawn().WrapToIl2Cpp();
			return false;
		}
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;
		return true;
	}
}