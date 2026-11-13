using HarmonyLib;

using ExtremeRoles.Module.CustomMonoBehaviour;

namespace ExtremeRoles.Patches;


[HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.AppendTaskText))]
public static class NormalPlayerTaskPatch
{
	public static bool Prefix(NormalPlayerTask __instance, [HarmonyArgument(0)] Il2CppSystem.Text.StringBuilder sb)
	{
		if (__instance.TryGetComponent<TaskTextOverrider>(out var overrider))
		{
			overrider.AppendText(sb);
			return false;
		}
		return true;
	}
}
