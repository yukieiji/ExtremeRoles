using System.Reflection;

using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;

namespace ExtremeRoles.Compat.Mods
{
    internal abstract class CompatModBase
    {
        protected BasePlugin Plugin;
        protected SemanticVersioning.Version Version;
        protected Assembly Dll;
        protected System.Type[] Function;

        internal CompatModBase(
            string guid, PluginInfo plugin)
        {
            this.Plugin = plugin!.Instance as BasePlugin;
            this.Version = plugin.Metadata.Version;
            this.Dll = Plugin!.GetType().Assembly;
            this.Function = AccessTools.GetTypesFromAssembly(this.Dll);

            this.PatchAll(new Harmony(guid));
        }
        protected abstract void PatchAll(Harmony harmony);
    }
}
