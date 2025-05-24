
using HarmonyLib;

using ExtremeRoles.Module;
using Il2CppSystem.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace ExtremeRoles.Patches;

#nullable enable

[HarmonyPatch(typeof(AnnouncementPopUp._Init_d__46), nameof(AnnouncementPopUp._Init_d__46.MoveNext))]
public static class AnnouncementPopUpInitPatch
{
	private static IEnumerator? enumerator;
	public static void Postfix(AnnouncementPopUp._Init_d__46 __instance, ref bool __result)
	{
		if (__result)
		{
			return;
		}
		if (enumerator == null)
		{
			enumerator = ModAnnounce.CoGetAnnounce().WrapToIl2Cpp();
		}
		__result = enumerator.MoveNext();
		__instance.__2__current = enumerator.Current;
		if (!__result)
		{
			enumerator = null;
		}
	}
}
