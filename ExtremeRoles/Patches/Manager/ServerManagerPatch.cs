using System.Linq;

using HarmonyLib;

using Innersloth.IO;
using Newtonsoft.Json;

namespace ExtremeRoles.Patches.Manager;

// v16だとカスタムサーバー等が生き残ってると予想外の動作が発生するのでバニラサーバー以外があると消し飛ばす処理を実装しておく
[HarmonyPatch(typeof(ServerManager), nameof(ServerManager.LoadServers))]
public static class ServerManagerLoadServersPatch
{
	public static void Prefix(ServerManager __instance)
	{
		if (!FileIO.Exists(__instance.serverInfoFileJson))
		{
			return;
		}

		var jsonServerData = JsonConvert.DeserializeObject<ServerManager.JsonServerData>(
			FileIO.ReadAllText(__instance.serverInfoFileJson), new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.Auto
			});
		jsonServerData.CleanAndMerge(ServerManager.DefaultRegions);

		var validData = jsonServerData.Regions.Where(
			x => x.TranslateName is
				StringNames.ServerAS or
				StringNames.ServerEU or
				StringNames.ServerNA
			).ToArray();

		if (validData.Length != jsonServerData.Regions.Count)
		{
			FileIO.Delete(__instance.serverInfoFileJson);
		}
	}
}
