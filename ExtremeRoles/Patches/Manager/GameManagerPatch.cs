using HarmonyLib;

namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch(typeof(NormalGameManager), nameof(NormalGameManager.GetDeadBody))]
public static class NormalGameManagerGetDeadBodyPatch
{
	public static bool ForceDefault { private get; set; } = false;
    public static void Postfix(NormalGameManager __instance , ref DeadBody __result)
    {
		if (ForceDefault)
		{
			__result = __instance.deadBodyPrefab[0];
		}
    }
}
