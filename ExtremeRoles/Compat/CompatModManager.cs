using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

using BepInEx;
using BepInEx.Unity.IL2CPP;

using Hazel;

using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Compat.ModIntegrator;
using ExtremeRoles.Compat.Initializer;
using ExtremeRoles.Module.CustomOption.Factory;
using Microsoft.Extensions.DependencyInjection;
using ExtremeRoles.Core.Abstract;


namespace ExtremeRoles.Compat;

#nullable enable

public enum CompatModType
{
	ExtremeSkins,
	ExtremeVoiceEngine,
	Submerged,
	CrowdedMod
}

public sealed class CompatModManager
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
				typeof(SubmergedInitializer)
			)
		},
		{
			CompatModType.CrowdedMod,
			new CompatModInfo(
				CompatModType.CrowdedMod.ToString(),
				CrowdedMod.Guid,
				"https://api.github.com/repos/yukieiji/CrowdedMod/releases/latest",
				true,
				typeof(CrowedModInitializer)
			)
		},
	};

	private readonly Dictionary<CompatModType, ModIntegratorBase> loadedMod = new();

	private IMapMod? map;

	private int startOptionId;
	private int endOptionId;

#pragma warning disable CS8618
	public static CompatModManager Instance { get; private set; }
#pragma warning restore CS8618

	private const int optionOffset = 100;

	public static void Initialize()
	{
		Instance = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<CompatModManager>();
	}

	public CompatModManager(
		IPluginLoader loader,
		IModInitializerFactory factory,
		IModLogger logger)
	{
		RemoveMap();

		logger.LogInfo(
			$"---------- CompatModManager Initialize Start with AmongUs ver.{UnityEngine.Application.version} ----------");

		foreach (var (modType, modInfo) in ModInfo)
		{
			string guid = modInfo.Guid;

			if (!loader.TryGetPlugin(guid, out var plugin))
			{
				continue;
			}


			logger.LogInfo(
				$"---- CompatMod:{guid} integrater Start!! ----");

			var initializer = factory.Create(modInfo.ModIntegratorType, plugin);
			if (initializer == null)
			{
				continue;
			}

			var integrator = initializer.Initialize();

			this.loadedMod.Add(modType, integrator);

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
			if (mod is not IIntegrateOption option)
			{
				continue;
			}

			ExtremeRolesPlugin.Logger.LogInfo(
				$"CreateIntegrateOption:{mod.Name}");

			using (var factory = OptionCategoryAssembler.CreateSequentialOptionCategory(
				startId + index, $"{mod.Name}Category"))
			{
				option.CreateIntegrateOption(factory);
			}
		}

		this.startOptionId = startId;
		this.endOptionId = startId + this.loadedMod.Count - 1;
	}

	internal IEnumerable<int> GetIntegrateOptionCategoryId()
	{
		for (int id = this.startOptionId; id <= this.endOptionId; ++id)
		{
			yield return id;
		}
	}

	internal bool IsIntegrateOption(int optionId)
		=> this.startOptionId <= optionId && optionId <= this.endOptionId;

	internal bool IsModMap<T>()
		where T : ModIntegratorBase
		=> this.map is T;

	// ここでtrueが返ってきてる時点でIMapModはNullではない
	internal bool TryGetMod<T>(CompatModType modName, [NotNullWhen(true)] out T? mod) where T : ModIntegratorBase
	{
		if (!this.loadedMod.TryGetValue(modName, out var modBase))
		{
			mod = default;
			return false;
		}
		mod = modBase as T;
		return mod is not null;
	}

	// ここでtrueが返ってきてる時点でIMapModはNullではない
	internal bool TryGetModMap([NotNullWhen(true)] out IMapMod? mapMod)
	{
		mapMod = this.map;
		return mapMod != null;
	}

	internal bool TryGetModMap<T>([NotNullWhen(true)] out T? mapMod)
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
