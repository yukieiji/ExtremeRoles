using System.Collections.Generic;
using System.Text.Json;

using ExtremeRoles.Helper;

using ExtremeRoles.Resources;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;

namespace ExtremeRoles.Test.Lobby.Asset;

public class ExSpawnMinigameLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:ExSpawnMinigameImgLoad Test -----");

		var assembly = ExtremeRolesPlugin.Instance.GetType().Assembly;
		using var stream = assembly.GetManifestResourceStream(
			ExtremeSpawnSelectorMinigame.JsonPath);

		if (stream is null)
		{
			Log.LogError("Selector Minigame json is Null");
			yield break;
		}

		var spawnInfo = JsonSerializer.Deserialize<
			Dictionary<string, ExtremeSpawnSelectorMinigame.SpawnPointInfo[]>>(stream);

		if (spawnInfo is null)
		{
			Log.LogError("Can't Deserialize Selector Minigame");
			yield break;
		}

		string[] allMap = [Map.SkeldKey, Map.MiraHqKey, Map.PolusKey, Map.FungleKey];

#if DEBUG
		foreach (string map in allMap)
		{

			if (!spawnInfo.TryGetValue(map, out var spawnPoints) ||
				spawnPoints is null)
			{
				Log.LogError($"Can't get SpawnPoints :{map}");
				continue;
			}

			string lowerMap = map.ToLower();

			foreach (var spawnPoint in spawnPoints)
			{
				LoadFromExR(
					string.Format(
						ObjectPath.ExtremeSelectorMinigameAssetFormat, lowerMap),
					string.Format(
						ObjectPath.ExtremeSelectorMinigameImgFormat, lowerMap, spawnPoint.RoomName));
			}
		}
#endif
		yield break;
	}
}
