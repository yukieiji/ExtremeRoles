using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

using BepInEx.Configuration;

using UnityEngine;

using Newtonsoft.Json.Linq;

using ExtremeSkins.Core;
using ExtremeSkins.Core.ExtremeVisor;
using ExtremeSkins.Module;

namespace ExtremeSkins.Loader;

public sealed class VisorLoader : ICosmicLoader
{
	private const string repo = "https://raw.githubusercontent.com/yukieiji/ExtremeVisor/main"; // When using this repository with Fork, please follow the license of each hat
	private const string skinDlUrl = "https://github.com/yukieiji/ExtremeVisor/archive/refs/heads/main.zip";

	private const string workingFolder = "ExVWorking";
	private const string dlZipName = "ExtremeVisor-main.zip";
	private const string visorDataPath = "ExtremeVisor-main";
	private const string visorDataFolderPath = "new_visor";

	private const string visorRepoData = "visorData.json";
	private const string visorTransData = "visorTransData.json";

	private const string updateComitKey = "ExVUpdateComitHash";
	private const string jsonUpdateComitKey = "updateComitHash";

	private readonly ConfigEntry<string> curUpdateHash;

	private const string defaultHash = "NoHashData";

	public VisorLoader()
	{
		this.curUpdateHash = ExtremeSkinsPlugin.Instance.Config.Bind(
			ExtremeSkinsPlugin.SkinComitCategory,
			updateComitKey, defaultHash);
	}

	public IEnumerator Fetch()
	{
		if (!this.isUpdate())
		{
			yield break;
		}

		var logger = ExtremeSkinsPlugin.Logger;
		logger.LogInfo(
			"---------- Extreme Visor Manager : VisorData Download Start!! ---------- ");

		string? ausFolder = Path.GetDirectoryName(Application.dataPath);
		if (string.IsNullOrEmpty(ausFolder))
		{
			yield break;
		}

		string dataSaveFolder = Path.Combine(ausFolder, DataStructure.FolderName);

		cleanUpCurSkinData(dataSaveFolder);

		string dlFolder = Path.Combine(ausFolder, workingFolder);

		Helper.FileUtility.DeleteDir(dlFolder);
		Directory.CreateDirectory(dlFolder);

		string zipPath = Path.Combine(dlFolder, dlZipName);

		yield return Helper.FileUtility.DlToZip(skinDlUrl, zipPath);
		if (!File.Exists(zipPath))
		{
			logger.LogInfo(
				"---------- Extreme Visor Manager : VisorData Download ERROR ---------- ");
			yield break;
		}

		logger.LogInfo(
			"---------- Extreme Visor Manager : VisorData Download Complete!! ---------- ");

		logger.LogInfo("---------- Extreme Visor Manager : VisorData Install Start!! ---------- ");

		installVisorData(dlFolder, zipPath, dataSaveFolder);

		logger.LogInfo("---------- Extreme Visor Manager : VisorData Install Complete!! ---------- ");
#if RELEASE
        Helper.FileUtility.DeleteDir(dlFolder);
#endif
	}

	public IReadOnlyDictionary<string, T> Load<T>() where T : class
	{
		if (typeof(T) != typeof(CustomVisor))
		{
			throw new System.ArgumentException(
				$"Type {typeof(T)} is not supported.");
		}
		var result = new Dictionary<string, T>();

		ExtremeSkinsPlugin.Logger.LogInfo(
			"---------- Extreme Visor Manager : Visor Loading Start!! ----------");

		getJsonData(visorTransData).GetAwaiter().GetResult();

		string? auPath = Path.GetDirectoryName(Application.dataPath);
		if (string.IsNullOrEmpty(auPath)) { return result; }

		string exvFolder = Path.Combine(auPath, DataStructure.FolderName);

		Helper.Translation.UpdateHatsTransData(
			Path.Combine(exvFolder, visorTransData));

		// UpdateComitHash
		byte[] byteVisorArray = File.ReadAllBytes(
			Path.Combine(exvFolder, visorRepoData));
		curUpdateHash.Value = (string)(JObject.Parse(
			Encoding.UTF8.GetString(byteVisorArray))[jsonUpdateComitKey]);

		string[] visorFolder = Directory.GetDirectories(exvFolder);

		foreach (string visor in visorFolder)
		{
			if (string.IsNullOrEmpty(visor)) { continue; }

			string infoJsonFile = Path.Combine(visor, InfoBase.JsonName);

			if (!File.Exists(infoJsonFile))
			{
				ExtremeSkinsPlugin.Logger.LogInfo(
					$"Error Detected!!:Can't load info.json for:{infoJsonFile}");
				continue;
			}

			using var jsonReader = new StreamReader(infoJsonFile);
			VisorInfo? info = JsonSerializer.Deserialize<VisorInfo>(
				jsonReader.ReadToEnd());

			if (info is null) { continue; }

			CustomVisor customVisor =
				info.Animation == null ?
				new CustomVisor(visor, info) : new AnimationVisor(visor, info);
			var castedVisor = Unsafe.As<CustomVisor, T>(ref customVisor);
			if (result.TryAdd(customVisor.Id, castedVisor))
			{
				ExtremeSkinsPlugin.Logger.LogInfo($"Visor Loaded :\n{customVisor}");
			}
		}

		ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme Visor Manager : Visor Loading Complete!! ----------");

		return result;
	}

	private static async Task getJsonData(string fileName)
	{
		await ICosmicLoader.getData(
			$"{repo}/{visorDataFolderPath}/{fileName}",
			Path.Combine(DataStructure.FolderName, fileName));
	}

	private static void cleanUpCurSkinData(
		string dataSaveFolder)
	{

		Helper.FileUtility.DeleteDir(dataSaveFolder);
		Directory.CreateDirectory(dataSaveFolder);

		getJsonData(visorRepoData).GetAwaiter().GetResult();

		byte[] byteVisorArray = File.ReadAllBytes(
			Path.Combine(dataSaveFolder, visorRepoData));
		string visorJsonString = Encoding.UTF8.GetString(byteVisorArray);

		JObject visorFolder = JObject.Parse(visorJsonString);

		for (int i = 0; i < visorFolder.Count; ++i)
		{
			JProperty? token = visorFolder.ChildrenTokens[i].TryCast<JProperty>();
			if (token == null) { continue; }

			string author = token.Name;

			if (author == jsonUpdateComitKey) { continue; }

			string checkVisorFolder = Path.Combine(dataSaveFolder, author);

			// まずはフォルダとファイルを消す
			if (Directory.Exists(checkVisorFolder))
			{
				string[] filePaths = Directory.GetFiles(checkVisorFolder);
				foreach (string filePath in filePaths)
				{
					File.SetAttributes(filePath, FileAttributes.Normal);
					File.Delete(filePath);
				}
				Directory.Delete(checkVisorFolder, false); ;
			}
		}
	}

	private static void installVisorData(
		string workingDir,
		string zipPath,
		string installFolder)
	{
		string extractPath = Path.Combine(workingDir, visorDataFolderPath);
		ZipFile.ExtractToDirectory(zipPath, extractPath);

		byte[] byteVisorArray = File.ReadAllBytes(
			Path.Combine(installFolder, visorRepoData));
		string visorJsonString = Encoding.UTF8.GetString(byteVisorArray);

		JToken visorFolder = JObject.Parse(visorJsonString)["data"];
		JArray? visorArray = visorFolder.TryCast<JArray>();

		if (visorArray == null) { return; }

		for (int i = 0; i < visorArray.Count; ++i)
		{
			string visorData = visorArray[i].ToString();

			if (visorData == visorRepoData || visorData == visorTransData)
			{
				continue;
			}

			string visorMoveToFolder = Path.Combine(
				installFolder, visorData);
			string visorSourceFolder = Path.Combine(
				extractPath, visorDataPath, visorDataFolderPath, visorData);

			ExtremeSkinsPlugin.Logger.LogInfo($"Installing Visor:{visorData}");

			Directory.Move(visorSourceFolder, visorMoveToFolder);
		}
	}

	private bool isUpdate()
	{
		ExtremeSkinsPlugin.Logger.LogInfo(
			"Extreme Visor Manager : Checking Update....");

		string? auPath = Path.GetDirectoryName(Application.dataPath);
		if (string.IsNullOrEmpty(auPath))
		{ 
			return true;
		}

		string exvFolder = Path.Combine(auPath, DataStructure.FolderName);

		if (!Directory.Exists(exvFolder))
		{
			return true;
		}

		getJsonData(visorRepoData).GetAwaiter().GetResult();

		byte[] byteVisorArray = File.ReadAllBytes(
			Path.Combine(exvFolder, visorRepoData));
		string visorJsonString = Encoding.UTF8.GetString(byteVisorArray);
		JObject visorJObject = JObject.Parse(visorJsonString);

		if (!(
				visorJObject.TryGetValue("data", out JToken visorFolder) &&
				visorJObject.TryGetValue(jsonUpdateComitKey, out JToken newHash)
			))
		{
			return true;
		}

		string curValue = curUpdateHash.Value;
		string newHashStr = (string)newHash;
		if (string.IsNullOrEmpty(newHashStr) ||
			(curValue != defaultHash && newHashStr != curValue))
		{
			return true;
		}

		JArray? visorArray = visorFolder.TryCast<JArray>();

		if (visorArray == null) { return true; }

		for (int i = 0; i < visorArray.Count; ++i)
		{
			string visorData = visorArray[i].ToString();

			if (visorData == visorRepoData ||
				visorData == visorTransData) { continue; }

			string checkVisorFolder = Path.Combine(exvFolder, visorData);
			string jsonPath = Path.Combine(checkVisorFolder, InfoBase.JsonName);
			if (!Directory.Exists(checkVisorFolder) ||
				!File.Exists(Path.Combine(
					checkVisorFolder, License.FileName)) ||
				!File.Exists(jsonPath) ||
				!File.Exists(Path.Combine(
					checkVisorFolder, DataStructure.IdleImageName)))
			{
				return true;
			}

			using var jsonReader = new StreamReader(jsonPath);
			VisorInfo? info = JsonSerializer.Deserialize<VisorInfo>(
				jsonReader.ReadToEnd());

			if (info is null) { return true; }

			if (info.LeftIdle &&
				!File.Exists(Path.Combine(
					checkVisorFolder, DataStructure.FlipIdleImageName)))
			{
				return true;
			}

		}
		return false;
	}
}
