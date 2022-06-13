using System;
using System.Collections.Generic;

using BepInEx;
using BepInEx.IL2CPP;

using Hazel;

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
            { SubmergedMap.Guid, typeof(SubmergedMap) },
        };

        internal CompatModManager()
        {
            RemoveMap();
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

        internal void SetUpMap(ShipStatus shipStatus)
        {
            foreach (var mod in loadedMod)
            {
                IMapMod mapMod = mod as IMapMod;
                if (mapMod != null && 
                    mapMod.MapType == shipStatus.Type)
                {
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
