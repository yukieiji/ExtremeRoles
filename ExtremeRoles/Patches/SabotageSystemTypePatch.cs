using HarmonyLib;

using ExtremeRoles.Module.SystemType;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.AnyActive), MethodType.Getter)]
public static class SabotageSystemTypeAnyActivePatch
{
	public static void Postfix(
		ref bool __result)
	{
		__result |= ExtremeSystemTypeManager.Instance.IsActiveSpecialSabotage;
	}
}
