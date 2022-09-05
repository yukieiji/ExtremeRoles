using System;
using System.Collections.Generic;

using BepInEx;
using BepInEx.IL2CPP;

using Hazel;

using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Compat.Mods;

namespace ExtremeRoles.Compat
{

    internal enum CompatModType
    {
        Submerged,
    }

    internal sealed class CompatModManager
    {
        public bool IsModMap => this.map != null;
        public IMapMod ModMap => this.map;

        public readonly Dictionary<CompatModType, CompatModBase> LoadedMod = new Dictionary<CompatModType, CompatModBase>();

        private IMapMod map;

        public static readonly Dictionary<CompatModType, (string, string)> ModInfo = new Dictionary<CompatModType, (string, string)>
        {
            { CompatModType.Submerged, ("Submerged", "https://api.github.com/repos/SubmergedAmongUs/Submerged/releases/latest") },
        };

        private static HashSet<(string, CompatModType, Type)> compatMod = new HashSet<(string, CompatModType, Type)>()
        {
            (SubmergedMap.Guid, CompatModType.Submerged, typeof(SubmergedMap)),
        };

        internal CompatModManager()
        {
            RemoveMap();
            foreach (var (guid, modType, mod) in compatMod)
            {
                PluginInfo plugin;

                if (IL2CPPChainloader.Instance.Plugins.TryGetValue(guid, out plugin))
                {
                    this.LoadedMod.Add(
                        modType,
                        (CompatModBase)Activator.CreateInstance(
                            mod, new object[] { plugin }));

                    Helper.Logging.Debug($"CompatMod:{guid} loaded!!");
                }
            }
        }

        internal void SetUpMap(ShipStatus shipStatus)
        {
            this.map = null;

            foreach (var mod in LoadedMod.Values)
            {
                IMapMod mapMod = mod as IMapMod;
                if (mapMod != null && 
                    mapMod.MapType == shipStatus.Type)
                {
                    ExtremeRolesPlugin.Logger.LogInfo(
                        $"Awake modmap:{mapMod}");
                    mapMod.Awake(shipStatus);
                    this.map = mapMod;
                    break;
                }
            }
        }
        internal void RemoveMap()
        {
            if (this.map == null) { return; }

            this.map.Destroy();
            this.map = null;
        }

        internal void IntegrateModCall(ref MessageReader reader)
        {
            byte callType = reader.ReadByte();

            switch (callType)
            {
                case IMapMod.RpcCallType:
                    byte mapRpcType = reader.ReadByte();
                    switch ((MapRpcCall)mapRpcType)
                    {
                        case MapRpcCall.RepairAllSabo:
                            this.ModMap.RepairCustomSabotage();
                            break;
                        case MapRpcCall.RepairCustomSaboType:
                            int repairSaboType = reader.ReadInt32();
                            this.ModMap.RepairCustomSabotage(
                                (TaskTypes)repairSaboType);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

        }

    }
}
