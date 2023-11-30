using ExtremeRoles.Module.SystemType;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.AnyActive), MethodType.Getter)]
public static class SabotageSystemTypeAnyActivePatch
{
	public static void Postfix(
		ref bool __result)
	{
		__result &= ExtremeSystemTypeManager.Instance.IsActiveSpecialSabotage;
	}
}
