using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
	public IReadOnlyDictionary<CompatModType, ModIntegratorBase> LoadedMod => this.loadedMod;
	public static IReadOnlyDictionary<CompatModType, CompatModInfo> ModInfo =>
		new Dictionary<CompatModType, CompatModInfo>
	{
		{
			CompatModType.Submerged,
			new CompatModInfo(
				CompatModType.Submerged.ToString(),
				SubmergedIntegrator.Guid,
				"https://api.github.com/repos/SubmergedAmongUs/Submerged/releases/latest",
				true,
				typeof(SubmergedIntegrator)
			)
		},
	};

	private Dictionary<CompatModType, ModIntegratorBase> loadedMod = new();

	private IMapMod? map;

	private int startOptionId;
	private int endOptionId;

#pragma warning disable CS8618
	public static CompatModManager Instance { get; private set; }
#pragma warning restore CS8618

	private const int optionOffset = 100;

	public static void Initialize()
	{
		Instance = new CompatModManager();
	}

	private CompatModManager()
	{
		RemoveMap();
		var logger = ExtremeRolesPlugin.Logger;


		logger.LogInfo(
			$"---------- CompatModManager Initialize Start with AmongUs ver.{UnityEngine.Application.version} ----------");

		foreach (var (modType, modInfo) in ModInfo)
		{
			PluginInfo? plugin;

			string guid = modInfo.Guid;

			if (!IL2CPPChainloader.Instance.Plugins.TryGetValue(guid, out plugin))
			{ continue; }


			logger.LogInfo(
				$"---- CompatMod:{guid} integrater Start!! ----");

			object? instance = Activator.CreateInstance(
				modInfo.ModIntegratorType, [ plugin ]);
			if (instance == null) { continue; }

			this.loadedMod.Add(modType, (ModIntegratorBase)instance);

			logger.LogInfo(
				$"---- CompatMod:{guid} integrated!! ----");

		}
		logger.LogInfo(
			$"---------- CompatModManager Initialize End!! ----------");
	}

	internal void CreateIntegrateOption(int startId)
	{
		foreach (var (mod, index) in this.loadedMod.Values.Select((value, index) => (value, index)))
		{
			using (var factory = OptionManager.CreateSequentialOptionCategory(
				startId + index, mod.Name))
			{
				mod.CreateIntegrateOption(factory);
			}
		}

		this.startOptionId = startId;
		this.endOptionId = startId + this.loadedMod.Count - 1;
	}

	internal string GetIntegrateOptionHudString()
	{
		StringBuilder builder = new StringBuilder();
		for (int id = this.startOptionId; id <= this.endOptionId; ++id)
		{
			if (!OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, id, out var cate))
			{
				continue;
			}
			cate.AddHudString(builder);
		}
		return builder.ToString().Trim('\r', '\n');
	}

	internal bool IsIntegrateOption(int optionId)
		=> this.startOptionId <= optionId && optionId <= this.endOptionId;

	internal bool IsModMap<T>()
		where T : ModIntegratorBase
		=> this.map is T;

	// ここでtrueが返ってきてる時点でIMapModはNullではない
	internal bool TryGetModMap(out IMapMod? mapMod)
	{
		mapMod = this.map;
		return mapMod != null;
	}

	// ここでtrueが返ってきてる時点でT?はNullではない
	internal bool TryGetModMap<T>(out T? mapMod)
		where T : ModIntegratorBase
	{
		mapMod = this.map as T;
		return mapMod != null;
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
						this.map?.RepairCustomSabotage();
						break;
					case MapRpcCall.RepairCustomSaboType:
						int repairSaboType = reader.ReadInt32();
						this.map?.RepairCustomSabotage(
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
