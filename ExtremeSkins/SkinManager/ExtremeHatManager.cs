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
using ExtremeSkins.Core.ExtremeHats;
using ExtremeSkins.Module;


namespace ExtremeSkins.SkinManager;

#if WITHHAT
public static class ExtremeHatManager
{
    public static readonly Dictionary<string, CustomHat> HatData = new Dictionary<string, CustomHat>();
    public static bool IsLoaded = false;

    private const string repo = "https://raw.githubusercontent.com/yukieiji/ExtremeHats/main"; // When using this repository with Fork, please follow the license of each hat
    private const string skinDlUrl = "https://github.com/yukieiji/ExtremeHats/archive/refs/heads/main.zip";

    public const string LicenseFileName = "LICENSE.md";

    private const string workingFolder = @"ExHWorking";
    private const string dlZipName = "ExtremeHats-main.zip";
    private const string hatDataPath = @"ExtremeHats-main\hat";
    
    private const string hatRepoData = "hatData.json";
    private const string hatTransData = "hatTranData.json";

    private static ConfigEntry<string> curUpdateHash;
    private const string updateComitKey = "ExHUpdateComitHash";
    private const string jsonUpdateComitKey = "updateComitHash";

    public static void Initialize()
    {
        curUpdateHash = ExtremeSkinsPlugin.Instance.Config.Bind(
            ExtremeSkinsPlugin.SkinComitCategory,
            updateComitKey, "NoHashData");

        HatData.Clear();
        IsLoaded = false;
    }

    public static bool IsUpdate()
    {

        ExtremeSkinsPlugin.Logger.LogInfo(
            "Extreme Hat Manager : Checking Update....");

        if (!Directory.Exists(Path.Combine(
                Path.GetDirectoryName(Application.dataPath),
                DataStructure.FolderName)))
        { 
            return true; 
        }

        getJsonData(hatRepoData).GetAwaiter().GetResult();

        string exhFolder = Path.Combine(
            Path.GetDirectoryName(Application.dataPath), DataStructure.FolderName);

        byte[] byteHatArray = File.ReadAllBytes(Path.Combine(exhFolder, hatRepoData));
        string hatJsonString = Encoding.UTF8.GetString(byteHatArray);

        JObject hatJObject = JObject.Parse(hatJsonString);
        JToken hatFolder = hatJObject["data"];
        JToken newHash = hatJObject[jsonUpdateComitKey];

        if ((string)newHash != curUpdateHash.Value) { return true; }

        JArray hatArray = hatFolder.TryCast<JArray>();

        for (int i = 0; i < hatArray.Count; ++i)
        {
            string hatData = hatArray[i].ToString();

            if (hatData == hatRepoData || hatData == hatTransData) { continue; }

            string checkHatFolder = Path.Combine(exhFolder, hatData);
            string jsonPath = Path.Combine(checkHatFolder, InfoBase.JsonName);
            string licenceFile = Path.Combine(checkHatFolder, LicenseFileName);

            if (!Directory.Exists(checkHatFolder) ||
                !File.Exists(licenceFile) ||
                !File.Exists(jsonPath) ||
                !File.Exists(Path.Combine(
                    checkHatFolder, DataStructure.FrontImageName)))
            { 
                return true; 
            }

            using var jsonReader = new StreamReader(jsonPath);
            HatInfo info = JsonSerializer.Deserialize<HatInfo>(
                jsonReader.ReadToEnd());
                
            if (info.FrontFlip &&
                !File.Exists(Path.Combine(checkHatFolder, DataStructure.FrontFlipImageName)))
            {
                return true;
            }
            if (info.Back &&
                !File.Exists(Path.Combine(checkHatFolder, DataStructure.BackImageName)))
            {
                return true;
            }
            if (info.BackFlip &&
                !File.Exists(Path.Combine(checkHatFolder, DataStructure.BackFlipImageName)))
            {
                return true;
            }
            if (info.Climb &&
                !File.Exists(Path.Combine(checkHatFolder, DataStructure.ClimbImageName)))
            {
                return true;
            }
        }
        return false;
    }

    public static void Load()
    {

        ExtremeSkinsPlugin.Logger.LogInfo(
            "---------- Extreme Hat Manager : Hat Loading Start!! ----------");

        getJsonData(hatTransData).GetAwaiter().GetResult();

        string exhFolder = Path.Combine(
            Path.GetDirectoryName(Application.dataPath), DataStructure.FolderName);

        Helper.Translation.UpdateHatsTransData(Path.Combine(exhFolder, hatTransData));

        // UpdateComitHash
        byte[] byteHatArray = File.ReadAllBytes(Path.Combine(exhFolder, hatRepoData));
        
        curUpdateHash.Value = (string)(JObject.Parse(
            Encoding.UTF8.GetString(byteHatArray))[jsonUpdateComitKey]);

        string[] hatsFolder = Directory.GetDirectories(exhFolder);

        foreach (string hat in hatsFolder)
        {
            if (string.IsNullOrEmpty(hat)) { continue; }

            string hatJsonPath = Path.Combine(hat, InfoBase.JsonName);

            if (!File.Exists(hatJsonPath))
            {
                ExtremeSkinsPlugin.Logger.LogInfo(
                    $"Error Detected!!:Can't load info.json for:{hatJsonPath}");
                continue;
            }

            using var jsonReader = new StreamReader(hatJsonPath);
            HatInfo info = JsonSerializer.Deserialize<HatInfo>(
                jsonReader.ReadToEnd());

            CustomHat customHat = new CustomHat(hat, info);

            if (HatData.TryAdd(customHat.Id, customHat))
            {
                ExtremeSkinsPlugin.Logger.LogInfo(
                    $"Hat Loaded :\n{customHat}");
            }
        }

        IsLoaded = true;
        ExtremeSkinsPlugin.Logger.LogInfo(
            "---------- Extreme Hat Manager : Hat Loading Complete!! ----------");
    }

    public static IEnumerator InstallData()
    {

        ExtremeSkinsPlugin.Logger.LogInfo(
            "---------- Extreme Hat Manager : HatData Download Start!! ---------- ");

        string ausFolder = Path.GetDirectoryName(Application.dataPath);
        string dataSaveFolder = Path.Combine(ausFolder, DataStructure.FolderName);

        cleanUpCurSkinData(dataSaveFolder);

        string dlFolder = Path.Combine(ausFolder, workingFolder);
        
        Helper.FileUtility.DeleteDir(dlFolder);
        Directory.CreateDirectory(dlFolder);

        string zipPath = Path.Combine(dlFolder, dlZipName);

        yield return Helper.FileUtility.DlToZip(skinDlUrl, zipPath);

        ExtremeSkinsPlugin.Logger.LogInfo(
            "---------- Extreme Hat Manager : HatData Download Complete!! ---------- ");
        
        ExtremeSkinsPlugin.Logger.LogInfo(
            "---------- Extreme Hat Manager : HatData Install Start!! ---------- ");

        installHatData(dlFolder, zipPath, dataSaveFolder);

        ExtremeSkinsPlugin.Logger.LogInfo(
            "---------- Extreme Hat Manager : HatData Install Complete!! ---------- ");
#if RELEASE
        Helper.FileUtility.DeleteDir(dlFolder);
# endif
    }

    public static void UpdateTranslation()
    {
        foreach (var hat in HatData.Values)
        {
           if (hat.Data != null)
           {
                hat.Data.name = Helper.Translation.GetString(
                    hat.Name);
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
                new System.Uri($"{repo}/hat/{fileName}"),
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

    private static void cleanUpCurSkinData(string dataSaveFolder)
    {

        Helper.FileUtility.DeleteDir(dataSaveFolder);
        Directory.CreateDirectory(dataSaveFolder);

        getJsonData(hatRepoData).GetAwaiter().GetResult();

        byte[] byteHatArray = File.ReadAllBytes(
            Path.Combine(dataSaveFolder, hatRepoData));
        string hatJsonString = Encoding.UTF8.GetString(byteHatArray);

        JToken hatFolder = JObject.Parse(hatJsonString)["data"];
        JArray hatArray = hatFolder.TryCast<JArray>();

        for (int i = 0; i < hatArray.Count; ++i)
        {
            string getHatData = hatArray[i].ToString();
            string getHatFolder = Path.Combine(dataSaveFolder, getHatData);

            // まずはフォルダとファイルを消す
            if (Directory.Exists(getHatFolder))
            {
                string[] filePaths = Directory.GetFiles(getHatFolder);
                foreach (string filePath in filePaths)
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                }
                Directory.Delete(getHatFolder, false); ;
            }
        }
    }

    private static void installHatData(
        string workingDir,
        string zipPath,
        string installFolder)
    {
        string extractPath = Path.Combine(workingDir, "hats");
        ZipFile.ExtractToDirectory(zipPath, extractPath);

        byte[] byteHatArray = File.ReadAllBytes(
           Path.Combine(installFolder, hatRepoData));
        string hatJsonString = Encoding.UTF8.GetString(byteHatArray);

        JToken hatFolder = JObject.Parse(hatJsonString)["data"];
        JArray hatArray = hatFolder.TryCast<JArray>();

        for (int i = 0; i < hatArray.Count; ++i)
        {
            string hatData = hatArray[i].ToString();

            if (hatData == hatRepoData || hatData == hatTransData) { continue; }

            string hatMoveToFolder = Path.Combine(installFolder, hatData);
            string hatSourceFolder = Path.Combine(extractPath, hatDataPath, hatData);
            
            ExtremeSkinsPlugin.Logger.LogInfo($"Installing Hat:{hatData}");

            Directory.Move(hatSourceFolder, hatMoveToFolder);
        }
    }
}
#endif
