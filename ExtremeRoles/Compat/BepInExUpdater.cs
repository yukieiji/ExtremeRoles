using AmongUs.Data;
using BepInEx;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using SemanticVersion = SemanticVersioning.Version;

#nullable enable


namespace ExtremeRoles.Compat;

#pragma warning disable ERA001
public sealed class BepInExUpdater : MonoBehaviour
{
	private const string minimumBepInExVersion = "6.0.0-be.735";
	private const string bepInExDownloadURL = "https://builds.bepinex.dev/projects/bepinex_be/735/BepInEx-Unity.IL2CPP-win-x{0}-6.0.0-be.735%2B5fef357.zip";

	private const string exeFileName = "ExtremeBepInExInstaller.exe";

	public static bool IsUpdateRquire()
	{
		string rawBepInExVersionStr = MetadataHelper.GetAttributes<
			AssemblyInformationalVersionAttribute>(typeof(Paths).Assembly)[0].InformationalVersion;
		int suffixIndex = rawBepInExVersionStr.IndexOf('+');
		return
			SemanticVersion.Parse(rawBepInExVersionStr.Substring(0, suffixIndex)) <
			SemanticVersion.Parse(minimumBepInExVersion);
	}

	public void Awake()
	{
		ExtremeRolesPlugin.Logger.LogInfo("BepInEx Update Required...");
		this.StartCoroutine(Excute());
	}

	[HideFromIl2Cpp]
	public IEnumerator Excute()
	{
		string showStr = Tr.GetString("ReqBepInExUpdate");

		Task.Run(() => Module.DllApi.MessageBox(
			IntPtr.Zero,
			showStr, "Extreme Roles", 0));

		string tmpFolder = Path.Combine(Paths.GameRootPath, "tmp");
		string zipPath = Path.Combine(tmpFolder, "BepInEx.zip");
		string extractPath = Path.Combine(tmpFolder, "BepInEx");

		if (Directory.Exists(tmpFolder))
		{
			Directory.Delete(tmpFolder, true);
		}
		Directory.CreateDirectory(tmpFolder);

		yield return dlBepInExZip(zipPath);
		if (!File.Exists(zipPath))
		{
			ExtremeRolesPlugin.Logger.LogError("Zip file not found");
			yield break;
		}

		ZipFile.ExtractToDirectory(zipPath, extractPath);

		extractExtremeBepInExInstaller(tmpFolder);
		extractDefaultConfig(extractPath);

		Process.Start(
			Path.Combine(Paths.GameRootPath, "tmp", exeFileName),
			$"{Paths.GameRootPath} {extractPath} {(uint)DataManager.Settings.Language.CurrentLanguage}");

		Application.Quit(0);
	}

	private static IEnumerator dlBepInExZip(string saveZipPath)
	{
		int cpu = File.Exists(
			Path.Combine(Paths.GameRootPath, "steam_appid.txt")) ? 86 : 64;

		UnityWebRequest www = UnityWebRequest.Get(string.Format(
			bepInExDownloadURL, cpu));
		yield return www.SendWebRequest();
		if (www.isNetworkError || www.isHttpError)
		{
			ExtremeRolesPlugin.Logger.LogInfo(www.error);
			yield break;
		}
		var handler = www.downloadHandler;
		Il2CppArrayBase<byte>? data;
		try
		{
			data = handler.GetData();
		}
		catch
		{
			try
			{
				data = handler.GetNativeData().ToArray();
			}
			catch
			{
				yield break;
			}
		}
		File.WriteAllBytes(saveZipPath, data);
	}

	private static void extractExtremeBepInExInstaller(string extractTmpFolder)
	{
		using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(
			$"ExtremeRoles.Resources.Installer.{exeFileName}");
		using var file = new FileStream(
			Path.Combine(extractTmpFolder, exeFileName),
			FileMode.OpenOrCreate, FileAccess.Write);
		resource!.CopyTo(file);
	}

	private static void extractDefaultConfig(string extractBepInExFolder)
	{
		var assembly = Assembly.GetExecutingAssembly();

		string configFolder = Path.Combine(extractBepInExFolder, "BepInEx", "config");

		if (Directory.Exists(configFolder))
		{
			Directory.Delete(configFolder, true);
		}
		Directory.CreateDirectory(configFolder);

		foreach (string filePath in getDefaultConfig())
		{
			using (var resource = assembly.GetManifestResourceStream(filePath))
			{
				string[] splitedFilePath = filePath.Split('.');
				string fileName = splitedFilePath[^2];
				string extention = splitedFilePath[^1];

				using (var file = new FileStream(
					Path.Combine(configFolder, $"{fileName}.{extention}"),
					FileMode.OpenOrCreate, FileAccess.Write))
				{
					resource!.CopyTo(file);
				}
			}
		}

	}

	private static string[] getDefaultConfig()
		=> [ "ExtremeRoles.Resources.Config.BepInEx.cfg" ];
}
#pragma warning restore ERA001

