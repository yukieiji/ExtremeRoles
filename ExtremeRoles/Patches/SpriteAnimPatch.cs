using HarmonyLib;

using PowerTools;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomMonoBehaviour;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(SpriteAnim), nameof(SpriteAnim.Initialize))]
public static class SpriteAnimAwakePatch
{
	public static void Postfix(SpriteAnim __instance)
	{
		var cleaner =　__instance.gameObject.TryAddComponent<SpriteAnimCleaner>();
		cleaner.Anim = __instance;
	}
}
