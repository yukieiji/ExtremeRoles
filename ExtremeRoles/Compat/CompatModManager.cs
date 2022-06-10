using System;
using System.Collections.Generic;
using System.Reflection;

using BepInEx;
using BepInEx.IL2CPP;

using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Compat.Mods;

namespace ExtremeRoles.Compat
{
    internal class CompatModManager
    {
        public bool IsModMap => this.map != null;
        public IMapMod ModMap => this.map;

        private IMapMod map;

        private HashSet<CompatModBase> loadedMod = new HashSet<CompatModBase>();

        private static Dictionary<string, Type> compatMod = new Dictionary<string, Type>()
        {
            { Submerged.Guid, typeof(Submerged) },
        };

        internal CompatModManager()
        {
            foreach (var (guid, mod) in compatMod)
            {
                PluginInfo plugin;

                if (IL2CPPChainloader.Instance.Plugins.TryGetValue(guid, out plugin))
                {
                    this.loadedMod.Add(
                        (CompatModBase)Activator.CreateInstance(
                            mod, new object[] { plugin }));
                }
            }
        }

    }
}
