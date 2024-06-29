using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using AmongUs.GameOptions;
using Newtonsoft.Json.Linq;

using ExtremeRoles.Extension.Json;

using ExtremeRoles.Compat;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Compat.ModIntegrator;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Helper;

#nullable enable

public static class Map
{
	public const string SkeldKey = "Skeld";
	public const string MiraHqKey = "MiraHQ";
	public const string PolusKey = "Polus";
	public const string AirShipKey = "AirShip";
	public const string FungleKey = "Fungle";
	public const string SubmergedKey = "Submerged";

	public const string SkeldAdmin = "MapRoomConsole";
	public const string SkeldSecurity = "SurvConsole";

	public const string MiraHqAdmin = "AdminMapConsole";
	public const string MiraHqSecurity = "SurvLogConsole";

	public const string PolusAdmin1 = "panel_map";
	public const string PolusAdmin2 = "panel_map (1)";
	public const string PolusSecurity = "Surv_Panel";
	public const string PolusVital = "panel_vitals";

	public const string AirShipSecurity = "task_cams";
	public const string AirShipVital = "panel_vitals";
	public const string AirShipArchiveAdmin = "records_admin_map";
	public const string AirShipCockpitAdmin = "panel_cockpit_map";

	public const string FangleSecurity = "BinocularsSecurityConsole";
	public const string FangleVital = "VitalsConsole";

	private const string airShipSpawnJson =
		"ExtremeRoles.Resources.JsonData.AirShipSpawnPoint.json";
	private const string airShipRandomSpawnKey = "VanillaRandomSpawn";

	private const string ventInfoJson =
		"ExtremeRoles.Resources.JsonData.AllVentLinkInfo.json";

	public static string Name
	{
		get
		{
			string key = string.Empty;

			if (CompatModManager.Instance.TryGetModMap(out var modMap))
			{
				if (modMap is SubmergedIntegrator)
				{
					key = "Submerged";
				}
			}
			else
			{
				key = Id switch
				{
					0 => SkeldKey,
					1 => MiraHqKey,
					2 => PolusKey,
					4 => AirShipKey,
					5 => FungleKey,
					_ => string.Empty,
				};
			}
			return key;
		}
	}

	public static byte Id => GameOptionsManager.Instance.CurrentGameOptions.GetByte(
		ByteOptionNames.MapId);

	public static void AddSpawnPoint(in List<Vector2> pos, in byte playerId)
	{
		int playerNum = PlayerCache.AllPlayerControl.Count;

		if (CompatModManager.Instance.TryGetModMap(out var modMap))
		{
			pos.AddRange(modMap!.GetSpawnPos(playerId));
		}
		else
		{
			var ship = CachedShipStatus.Instance;

			switch (Id)
			{
				case 4:
					pos.AddRange(GetAirShipRandomSpawn());
					break;
				default:
					Vector2 baseVec = Vector2.up;
					baseVec = baseVec.Rotate(
						(float)(playerId - 1) * (360f / (float)playerNum));
					Vector2 offset = baseVec * ship.SpawnRadius + new Vector2(
						0f, 0.3636f);
					pos.Add(ship.InitialSpawnCenter + offset);
					pos.Add(ship.MeetingSpawnCenter + offset);
					break;
			}
		}
	}

	public static void AddSpawnPoint(in IEnumerable<Vector2> pos, in byte playerId)
	{
		var spawnPoint = new List<Vector2>(5);
		AddSpawnPoint(spawnPoint, playerId);
		pos.Concat(spawnPoint);
	}

	public static void DisableSecurity()
	{
		var security = GetSecuritySystemConsole();
		if (security == null) { return; }

		Unity.SetColliderActive(security.gameObject, false);
	}
	public static void DisableVital()
	{
		HashSet<string> vitalObj = new HashSet<string>(2);
		if (CompatModManager.Instance.TryGetModMap(out var modMap))
		{
			vitalObj = modMap!.GetSystemObjectName(
				SystemConsoleType.Vital);
		}
		else
		{
			switch (Id)
			{
				case 2:
					vitalObj.Add(PolusVital);
					break;
				case 4:
					vitalObj.Add(AirShipVital);
					break;
				case 5:
					vitalObj.Add(FangleVital);
					break;
				default:
					break;
			}
		}

		if (vitalObj.Count == 0)
		{
			return;
		}

		DisableSystemConsole(vitalObj);
	}

	public static void DisableAdmin()
	{
		HashSet<string> adminObj = new HashSet<string>(2);
		if (CompatModManager.Instance.TryGetModMap(out var modMap))
		{
			adminObj = modMap!.GetSystemObjectName(
				SystemConsoleType.Admin);
		}
		else
		{
			switch (Id)
			{
				case 0:
					adminObj.Add(SkeldAdmin);
					break;
				case 1:
					adminObj.Add(MiraHqAdmin);
					break;
				case 2:
					adminObj.Add(PolusAdmin1);
					adminObj.Add(PolusAdmin2);
					break;
				case 4:
					adminObj.Add(AirShipArchiveAdmin);
					adminObj.Add(AirShipCockpitAdmin);
					break;
				default:
					break;
			}
		}

		if (adminObj.Count == 0)
		{
			return;
		}

		DisableMapConsole(adminObj);
	}

	public static void DisableConsole(string mapModuleName)
	{
		var mapConsoleArray = Object.FindObjectsOfType<MapConsole>();
		Unity.FindAndDisableComponent(
			mapConsoleArray, mapModuleName);
	}
	public static void DisableMapConsole(IReadOnlySet<string> mapModuleName)
	{
		var mapConsoleArray = Object.FindObjectsOfType<MapConsole>();
		Unity.FindAndDisableComponent(
			mapConsoleArray, mapModuleName);
	}
	public static void DisableSystemConsole(IReadOnlySet<string> mapModuleName)
	{
		var systemConsoleArray = Object.FindObjectsOfType<SystemConsole>();
		Unity.FindAndDisableComponent(
			systemConsoleArray, mapModuleName);
	}

	public static List<Vector2> GetAirShipRandomSpawn()
	{
		JObject? json = JsonParser.GetJObjectFromAssembly(airShipSpawnJson);

		List<Vector2> result = new List<Vector2>();

		if (json == null) { return result; }

		JArray? airShipSpawn = json.Get<JArray>(airShipRandomSpawnKey);

		if (airShipSpawn == null) { return result; }
		result.Capacity = airShipSpawn.Count;

		for (int i = 0; i < airShipSpawn.Count; ++i)
		{
			JArray? pos = airShipSpawn.Get<JArray>(i);
			if (pos == null) { continue; }
			result.Add(new Vector2((float)pos[0], (float)pos[1]));
		}

		return result;
	}

	public static SystemConsole? GetSecuritySystemConsole()
	{
		SystemConsole? watchConsole;
		if (CompatModManager.Instance.TryGetModMap(out var modMap))
		{
			watchConsole = modMap!.GetSystemConsole(SystemConsoleType.SecurityCamera);
		}
		else
		{
			watchConsole = getVanillaSecurityConsole();
		}
		return watchConsole;
	}

	private static SystemConsole? getVanillaSecurityConsole()
	{
		// 0 = Skeld
		// 1 = Mira HQ
		// 2 = Polus
		// 3 = Dleks - deactivated
		// 4 = Airship
		string key = Id switch
		{
			0 or 3 => SkeldSecurity,
			1 => MiraHqSecurity,
			2 => PolusSecurity,
			4 => AirShipSecurity,
			5 => FangleSecurity,
			_ => string.Empty,
		};

		var systemConsoleArray = Object.FindObjectsOfType<SystemConsole>();

		return systemConsoleArray.FirstOrDefault(
			x => x.gameObject.name.Contains(key));

	}

	public static void RelinkVent()
	{
		var mapVent = CachedShipStatus.Instance.AllVents;
		var ventIdMapping = new Dictionary<int, Vent>(mapVent.Count);
		foreach (Vent vent in mapVent)
		{
			ventIdMapping.Add(vent.Id, vent);
		}

		JObject? linkInfoJson = JsonParser.GetJObjectFromAssembly(ventInfoJson);
		string key = Name;
		if (linkInfoJson == null || key == MiraHqKey) { return; }

		JArray? linkInfo = linkInfoJson.Get<JArray>(key);
		if (linkInfo == null) { return; }

		for (int i = 0; i < linkInfo.Count; ++i)
		{
			JArray? ventLinkedId = linkInfo.Get<JArray>(i);
			if (ventLinkedId == null) { continue; }

			if (ventIdMapping.TryGetValue((int)ventLinkedId[0], out Vent? from) &&
				ventIdMapping.TryGetValue((int)ventLinkedId[1], out Vent? target) &&
				from != null && target != null)
			{
				linkVent(from, target);
			}
		}
	}

	private static void linkVent(Vent from, Vent target)
	{
		if (from == null || target == null) { return; }

		linkVentToEmptyTarget(from, target);
		linkVentToEmptyTarget(target, from);
	}

	private static void linkVentToEmptyTarget(Vent from, Vent target)
	{
		if (from.Right == null)
		{
			from.Right = target;
		}
		else if (from.Center == null)
		{
			from.Center = target;
		}
		else if (from.Left == null)
		{
			from.Left = target;
		}
		else
		{
			ExtremeRolesPlugin.Logger.LogInfo("Vent Link fail!!");
		}
	}
}
