
using HarmonyLib;
using Il2CppSystem.Collections;

using ExtremeRoles.Module;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.Init))]
public static class AnnouncementPopUpInitPatch
{
	public static void Postfix(ref IEnumerator __result)
	{
		__result = Effects.Sequence(
			ModAnnounce.CoGetAnnounce().WrapToIl2Cpp(),
			__result);
	}
}
