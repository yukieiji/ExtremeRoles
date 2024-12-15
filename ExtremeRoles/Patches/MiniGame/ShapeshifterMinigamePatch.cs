using HarmonyLib;

using ExtremeRoles.Module.CustomMonoBehaviour.Overrider;

namespace ExtremeRoles.Patches.MiniGame;

[HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.Shapeshift))]
public static class ShapeshifterMinigameShapeshiftPatch
{
    public static bool Prefix(
		ShapeshifterMinigame __instance,
		[HarmonyArgument(0)] PlayerControl target)
    {
		if (!__instance.TryGetComponent<ShapeshifterMinigameShapeshiftOverride>(out var overrider))
		{
			return true;
		}
		overrider.OverrideShapeshift(__instance, target);
		return false;
    }
}
