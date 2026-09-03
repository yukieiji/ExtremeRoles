using BepInEx;

namespace ExtremeRoles.Compat.Interface;

public interface IHarmonyPatchProvider
{
	public IHarmonyPatch Get(PluginInfo plugin);
}
