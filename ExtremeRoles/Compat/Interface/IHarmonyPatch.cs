using HarmonyLib;
using System.Reflection;

#nullable enable

namespace ExtremeRoles.Compat.Interface;

public interface IHarmonyPatch
{
	public void Patch(
		MethodBase original,
		HarmonyMethod? prefix = null,
		HarmonyMethod? postfix = null,
		HarmonyMethod? transpiler = null,
		HarmonyMethod? finalizer = null,
		HarmonyMethod? ilmanipulator = null);
}
