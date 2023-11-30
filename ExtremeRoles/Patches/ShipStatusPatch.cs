using Il2CppSystem.Collections;

using HarmonyLib;

using ExtremeRoles.Compat;
using ExtremeRoles.Performance;
using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;

using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
public static class ShipStatusAwakePatch
{
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void Postfix(ShipStatus __instance)
    {
		CachedShipStatus.SetUp(__instance);
        CompatModManager.Instance.SetUpMap(__instance);
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
public static class ShipStatusCalculateLightRadiusPatch
{
    public static bool Prefix(
        ref float __result,
        ShipStatus __instance,
        [HarmonyArgument(0)] GameData.PlayerInfo playerInfo)
    {
        return VisionComputer.Instance.IsVanillaVisionAndGetVision(
            __instance, playerInfo, out __result);
    }

}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnDestroy))]
public static class ShipStatusOnDestroyPatch
{
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void Postfix()
    {
        CachedShipStatus.Destroy();
		CompatModManager.Instance.RemoveMap();
		ExtremeSystemTypeManager.Instance.Reset();
	}
}


[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.PrespawnStep))]
public static class ShipStatusPrespawnStepPatch
{
	public static bool Prefix(ref IEnumerator __result)
	{
		var spawnOpt = ExtremeGameModeManager.Instance.ShipOption.Spawn;
		if (!spawnOpt.EnableRandom)
		{
			return true;
		}

		bool enableRandomSpawn = GameManager.Instance.LogicOptions.MapId switch
		{
			1 => spawnOpt.Skeld,
			2 => spawnOpt.MiraHq,
			3 => spawnOpt.Polus,
			5 => spawnOpt.Fungle,
			_ => false,
		};

		if (enableRandomSpawn)
		{
			__result = ExtremeSpawnSelectorMinigame.WaiteSpawn().WrapToIl2Cpp();
			return false;
		}
		return true;
	}
}