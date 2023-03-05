using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using BepInEx;
using BepInEx.Unity.IL2CPP.Utils;

using Il2CppInterop.Runtime.Attributes;

using UnityEngine;
using UnityEngine.Networking;

using AmongUs.Data;

using SemanticVersion = SemanticVersioning.Version;


namespace ExtremeRoles.Compat;

public sealed class BepInExUpdater : MonoBehaviour
{
    private const string minimumBepInExVersion = "6.0.0-be.667";
    private const string bepInExDownloadURL = "https://builds.bepinex.dev/projects/bepinex_be/667/BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.667%2B6b500b3.zip";

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
        string showStr = Helper.Translation.GetString("ReqBepInExUpdate");

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

        ZipFile.ExtractToDirectory(zipPath, extractPath);

        extractExtremeBepInExInstaller(tmpFolder);
        extractDefaultConfig(extractPath);

        Process.Start(
            Path.Combine(Paths.GameRootPath, "tmp", exeFileName),
            $"{Paths.GameRootPath} {extractPath} {(uint)DataManager.Settings.Language.CurrentLanguage}");

        Application.Quit();
    }

    private static IEnumerator dlBepInExZip(string saveZipPath)
    {

        UnityWebRequest www = UnityWebRequest.Get(bepInExDownloadURL);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            ExtremeRolesPlugin.Logger.LogInfo(www.error);
            yield break;
        }

        File.WriteAllBytes(saveZipPath, www.downloadHandler.data);
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
                string fileName = splitedFilePath[splitedFilePath.Length - 2];
                string extention = splitedFilePath[splitedFilePath.Length - 1];

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
        => new string[] { "ExtremeRoles.Resources.Config.BepInEx.cfg" };
}
