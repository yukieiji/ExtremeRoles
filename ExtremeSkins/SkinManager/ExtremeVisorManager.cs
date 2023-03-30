using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

using BepInEx.Configuration;

using UnityEngine;

using Newtonsoft.Json.Linq;

using ExtremeSkins.Core;
using ExtremeSkins.Core.ExtremeVisor;
using ExtremeSkins.Module;

namespace ExtremeSkins.SkinManager;

#if WITHVISOR
public static class ExtremeVisorManager
{
    public static readonly Dictionary<string, CustomVisor> VisorData = new Dictionary<string, CustomVisor>();
    public static bool IsLoaded = false;

    private const string repo = "https://raw.githubusercontent.com/yukieiji/ExtremeVisor/main"; // When using this repository with Fork, please follow the license of each hat
    private const string skinDlUrl = "https://github.com/yukieiji/ExtremeVisor/archive/refs/heads/main.zip";

    private const string workingFolder = "ExVWorking";
    private const string dlZipName = "ExtremeVisor-main.zip";
    private const string visorDataPath = "ExtremeVisor-main";
    private const string visorDataFolderPath = "new_visor";

    private const string visorRepoData = "visorData.json";
    private const string visorTransData = "visorTransData.json";

    private static ConfigEntry<string> curUpdateHash;
    private const string updateComitKey = "ExVUpdateComitHash";
    private const string jsonUpdateComitKey = "updateComitHash";

    public static void Initialize()
    {
        curUpdateHash = ExtremeSkinsPlugin.Instance.Config.Bind(
            ExtremeSkinsPlugin.SkinComitCategory,
            updateComitKey, "NoHashData");
        VisorData.Clear();
        IsLoaded = false;
    }

    public static bool IsUpdate()
    {

        ExtremeSkinsPlugin.Logger.LogInfo(
            "Extreme Visor Manager : Checking Update....");

        string exvFolder = Path.Combine(
            Path.GetDirectoryName(Application.dataPath), DataStructure.FolderName);

        if (!Directory.Exists(exvFolder)) { return true; }

        getJsonData(visorRepoData).GetAwaiter().GetResult();

        byte[] byteVisorArray = File.ReadAllBytes(
            Path.Combine(exvFolder, visorRepoData));
        string visorJsonString = Encoding.UTF8.GetString(byteVisorArray);
        JObject visorJObject = JObject.Parse(visorJsonString);

        JToken visorFolder = visorJObject["data"];
        JToken newHash = visorJObject[jsonUpdateComitKey];

        if ((string)newHash != curUpdateHash.Value) { return true; }

        JArray visorArray = visorFolder.TryCast<JArray>();

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
            VisorInfo info = JsonSerializer.Deserialize<VisorInfo>(
                jsonReader.ReadToEnd());

            if (info.LeftIdle &&
                !File.Exists(Path.Combine(
                    checkVisorFolder, DataStructure.FlipIdleImageName)))
            {
                return true;
            }

        }
        return false;
    }

    public static void Load()
    {

        ExtremeSkinsPlugin.Logger.LogInfo(
            "---------- Extreme Visor Manager : Visor Loading Start!! ----------");

        getJsonData(visorTransData).GetAwaiter().GetResult();

        string exvFolder = Path.Combine(
            Path.GetDirectoryName(Application.dataPath), DataStructure.FolderName);

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
            VisorInfo info = JsonSerializer.Deserialize<VisorInfo>(
                jsonReader.ReadToEnd());

            CustomVisor customVisor = new CustomVisor(visor, info);

            if (VisorData.TryAdd(customVisor.Id, customVisor))
            {
                ExtremeSkinsPlugin.Logger.LogInfo(
                    $"Visor Loaded:{customVisor.Name}, from:{visor}");
            }
        }

        IsLoaded = true;

        ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme Visor Manager : Visor Loading Complete!! ----------");

    }

    public static IEnumerator InstallData()
    {

        ExtremeSkinsPlugin.Logger.LogInfo(
            "---------- Extreme Visor Manager : VisorData Download Start!! ---------- ");

        string ausFolder = Path.GetDirectoryName(Application.dataPath);
        string dataSaveFolder = Path.Combine(ausFolder, DataStructure.FolderName);

        cleanUpCurSkinData(dataSaveFolder);

        string dlFolder = Path.Combine(ausFolder, workingFolder);

        Helper.FileUtility.DeleteDir(dlFolder);
        Directory.CreateDirectory(dlFolder);

        string zipPath = string.Concat(dlFolder, dlZipName);

        yield return Helper.FileUtility.DlToZip(skinDlUrl, zipPath);

        ExtremeSkinsPlugin.Logger.LogInfo(
            "---------- Extreme Visor Manager : VisorData Download Complete!! ---------- ");

        ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme Visor Manager : VisorData Install Start!! ---------- ");

        installVisorData(dlFolder, zipPath, dataSaveFolder);

        ExtremeSkinsPlugin.Logger.LogInfo(
            "---------- Extreme Visor Manager : VisorData Install Complete!! ---------- ");
#if RELEASE
        Helper.FileUtility.DeleteDir(dlFolder);
# endif
    }

    public static void UpdateTranslation()
    {
        foreach (var vi in VisorData.Values)
        {
            if (vi.Data != null)
            {
                vi.Data.name = Helper.Translation.GetString(
                    vi.Name);
            }
        }
    }

    private static async Task getJsonData(string fileName)
    {
        try
        {
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue 
            { 
                NoCache = true 
            };
            var response = await http.GetAsync(
                new System.Uri($"{repo}/{visorDataFolderPath}/{fileName}"),
                HttpCompletionOption.ResponseContentRead);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                ExtremeSkinsPlugin.Logger.LogInfo($"Can't load json");
            }
            if (response.Content == null)
            {
                ExtremeSkinsPlugin.Logger.LogInfo(
                    $"Server returned no data: {response.StatusCode}");
            }

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                using (var fileStream = File.Create(
                    Path.Combine(
                        Path.GetDirectoryName(Application.dataPath),
                        DataStructure.FolderName, fileName)))
                {
                    responseStream.CopyTo(fileStream);
                }
            }
        }
        catch (System.Exception e)
        {
            ExtremeSkinsPlugin.Logger.LogInfo(
                $"Unable to fetch hats from repo: {repo}\n{e.Message}");
        }
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
            JProperty token = visorFolder.ChildrenTokens[i].TryCast<JProperty>();
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
        JArray visorArray = visorFolder.TryCast<JArray>();

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

}
#endif
