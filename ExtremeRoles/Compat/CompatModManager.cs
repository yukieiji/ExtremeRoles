using System;
using System.Collections.Generic;

using BepInEx;
using BepInEx.Unity.IL2CPP;

using Hazel;

using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Compat.ModIntegrator;

namespace ExtremeRoles.Compat;

#nullable enable

internal enum CompatModType
{
    ExtremeSkins,
    ExtremeVoiceEngine,
    Submerged,
}

internal sealed class CompatModManager
{
	public bool IsModMap => this.map != null;
    public IMapMod? ModMap => this.map;

    public readonly Dictionary<CompatModType, ModIntegratorBase> LoadedMod = new Dictionary<CompatModType, ModIntegratorBase>();

    private IMapMod? map;

    public static readonly Dictionary<CompatModType, CompatModInfo> ModInfo = new Dictionary<CompatModType, CompatModInfo>
    {
        {
			CompatModType.Submerged,
			new CompatModInfo(
				CompatModType.Submerged.ToString(),
				SubmergedIntegrator.Guid,
				"https://api.github.com/repos/SubmergedAmongUs/Submerged/releases/latest",
				true,
				typeof(SubmergedIntegrator))
		},
    };

#pragma warning disable CS8618
	public static CompatModManager Instance { get; private set; }
#pragma warning restore CS8618

	internal CompatModManager()
    {
        RemoveMap();

        ExtremeRolesPlugin.Logger.LogInfo(
            $"---------- CompatModManager Initialize Start with AmongUs ver.{UnityEngine.Application.version} ----------");

        foreach (var (modType, modInfo) in ModInfo)
        {
            PluginInfo? plugin;

			string guid = modInfo.Guid;

			if (!IL2CPPChainloader.Instance.Plugins.TryGetValue(guid, out plugin))
			{ continue;  }


			ExtremeRolesPlugin.Logger.LogInfo(
					$"---- CompatMod:{guid} integrater Start!! ----");

			object? instance = Activator.CreateInstance(
				modInfo.ModIntegratorType, new object[] { plugin });
			if (instance == null) { continue; }

			this.LoadedMod.Add(modType, (ModIntegratorBase)instance);

			ExtremeRolesPlugin.Logger.LogInfo(
				$"---- CompatMod:{guid} integrated!! ----");

		}
        ExtremeRolesPlugin.Logger.LogInfo(
            $"---------- CompatModManager Initialize End!! ----------");
		Instance = this;
	}

    internal void SetUpMap(ShipStatus shipStatus)
    {
        this.map = null;

        foreach (var mod in LoadedMod.Values)
        {
            if (mod is IMapMod mapMod &&
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
                        this.ModMap?.RepairCustomSabotage();
                        break;
                    case MapRpcCall.RepairCustomSaboType:
                        int repairSaboType = reader.ReadInt32();
                        this.ModMap?.RepairCustomSabotage(
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
