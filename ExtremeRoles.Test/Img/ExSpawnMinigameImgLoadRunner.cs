using System;
using System.Collections.Generic;
using System.Text.Json;

using ExtremeRoles.Helper;

using ExtremeRoles.Resources;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;
using UnityEngine;

namespace ExtremeRoles.Test.Img;

internal sealed class ExSpawnMinigameImgLoadRunner
	: TestRunnerBase
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:ExSpawnMinigameImgLoad Test -----");

		var assembly = ExtremeRolesPlugin.Instance.GetType().Assembly;
		using var stream = assembly.GetManifestResourceStream(
			ExtremeSpawnSelectorMinigame.JsonPath);

		if (stream is null)
		{
			Log.LogError("Selector Minigame json is Null");
			return;
		}

		var spawnInfo = JsonSerializer.Deserialize<
			Dictionary<string, ExtremeSpawnSelectorMinigame.SpawnPointInfo[]>>(stream);

		if (spawnInfo is null)
		{
			Log.LogError("Can't Deserialize Selector Minigame");
			return;
		}

		string[] allMap = [Map.SkeldKey, Map.MiraHqKey, Map.PolusKey, Map.FungleKey];


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
				tryImgLoad(lowerMap, spawnPoint.RoomName);
			}
		}
	}

	private void tryImgLoad(string lowerMap, string roomName)
	{
		try
		{
			var sprite = Loader.GetUnityObjectFromExRResources<Sprite>(
				string.Format(
					Path.ExtremeSelectorMinigameAssetFormat, lowerMap),
				string.Format(
					Path.ExtremeSelectorMinigameImgFormat, lowerMap, roomName));
			Log.LogInfo($"Img Loaded:{lowerMap}.{roomName}");
			if (sprite == null)
			{
				throw new Exception("Sprite is Null");
			}
		}
		catch (Exception ex)
		{
			Log.LogError(
				$"Img:{lowerMap}, {roomName} not load   {ex.Message}");
		}
	}
}
