using HarmonyLib;
using System.Reflection;

using ExtremeRoles.Compat.Interface;

namespace ExtremeRoles.Compat;

public class HarmonyPatchWrapper(Harmony harmony) : IHarmonyPatch
{
	private readonly Harmony harmony = harmony;
	public void Patch(MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null, HarmonyMethod finalizer = null, HarmonyMethod ilmanipulator = null)
		=> this.harmony.Patch(
			original, prefix, postfix, transpiler, finalizer, ilmanipulator);
}
