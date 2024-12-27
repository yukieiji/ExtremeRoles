using HarmonyLib;

using BepInEx;


#nullable enable

namespace ExtremeRoles.Compat.ModIntegrator;

public sealed class CrowdedMod(PluginInfo plugin) : ModIntegratorBase(Guid, plugin)
{
	public const string Guid = "CrowdedMod";

	protected override void PatchAll(Harmony harmony)
	{ }
}