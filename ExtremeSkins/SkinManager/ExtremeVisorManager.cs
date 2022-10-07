using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using BepInEx.Configuration;

using UnityEngine;

using Newtonsoft.Json.Linq;

using ExtremeSkins.Module;

namespace ExtremeSkins.SkinManager
{
#if WITHVISOR
    public static class ExtremeVisorManager
    {
        public static readonly Dictionary<string, CustomVisor> VisorData = new Dictionary<string, CustomVisor>();
        public static bool IsLoaded = false;

        public const string FolderPath = @"\ExtremeVisor\";
        public const string LicenseFileName = "LICENSE.md";
        public const string InfoFileName = "info.json";

        private const string repo = "https://raw.githubusercontent.com/yukieiji/ExtremeVisor/main"; // When using this repository with Fork, please follow the license of each hat
        private const string skinDlUrl = "https://github.com/yukieiji/ExtremeVisor/archive/refs/heads/main.zip";

        private const string workingFolder = @"\ExVWorking\";
        private const string dlZipName = "ExtremeVisor-main.zip";
        private const string visorDataPath = @"\ExtremeVisor-main\new_visor\";

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

            ExtremeSkinsPlugin.Logger.LogInfo("Extreme Visor Manager : Checking Update....");

            if (!Directory.Exists(string.Concat(
                Path.GetDirectoryName(Application.dataPath), FolderPath))) { return true; }

            getJsonData(visorRepoData).GetAwaiter().GetResult();
            
            byte[] byteVisorArray = File.ReadAllBytes(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, visorRepoData));
            string visorJsonString = System.Text.Encoding.UTF8.GetString(byteVisorArray);
            JObject visorJObject = JObject.Parse(visorJsonString);

            JToken visorFolder = visorJObject["data"];
            JToken newHash = visorJObject[jsonUpdateComitKey];

            if ((string)newHash != curUpdateHash.Value) { return true; }

            JArray visorArray = visorFolder.TryCast<JArray>();

            for (int i = 0; i < visorArray.Count; ++i)
            {
                string visorData = visorArray[i].ToString();

                if (visorData == visorRepoData || visorData == visorTransData) { continue; }

                string checkHatFolder = string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, @"\", visorData);

                if (!Directory.Exists(checkHatFolder)) { return true; }

                if (!File.Exists(string.Concat(
                        checkHatFolder, @"\", LicenseFileName))) { return true; }

                if (!File.Exists(string.Concat(
                        checkHatFolder, @"\", InfoFileName))) { return true; }
                if (!File.Exists(string.Concat(
                        checkHatFolder, @"\", CustomVisor.IdleName))) { return true; }

                byte[] byteArray = File.ReadAllBytes(
                   string.Concat(checkHatFolder, @"\", InfoFileName));
                string json = System.Text.Encoding.UTF8.GetString(byteArray);
                JObject parseJson = JObject.Parse(json);

                if (((bool)parseJson["IdleFlip"]) &&
                    !File.Exists(string.Concat(
                        checkHatFolder, @"\", CustomVisor.FlipIdleName)))
                {
                    return true;
                }

            }
            return false;
        }

        public static void Load()
        {

            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme Visor Manager : Visor Loading Start!! ----------");

            getJsonData(visorTransData).GetAwaiter().GetResult();
            Helper.Translation.UpdateHatsTransData(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, visorTransData));

            // UpdateComitHash
            byte[] byteVisorArray = File.ReadAllBytes(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, visorRepoData));
            curUpdateHash.Value = (string)(JObject.Parse(
                System.Text.Encoding.UTF8.GetString(byteVisorArray))[jsonUpdateComitKey]);

            string[] visorFolder = Directory.GetDirectories(
                string.Concat(Path.GetDirectoryName(Application.dataPath), FolderPath));


            foreach (string visor in visorFolder)
            {
                if (string.IsNullOrEmpty(visor)) { continue; }

                string infoJsonFile = string.Concat(visor, @"\", InfoFileName);

                if (!File.Exists(infoJsonFile))
                {
                    ExtremeSkinsPlugin.Logger.LogInfo(
                        $"Error Detected!!:Can't load info.json for:{infoJsonFile}");
                    continue;
                }

                byte[] byteArray = File.ReadAllBytes(infoJsonFile);
                string json = System.Text.Encoding.UTF8.GetString(byteArray);
                JObject parseJson = JObject.Parse(json);

                CustomVisor customVisor = new CustomVisor(
                    visor,
                    parseJson["Author"].ToString(),  // Author
                    parseJson["Name"].ToString(),  // Name
                    (bool)parseJson["LeftIdle"],
                    (bool)parseJson["Shader"], // Shader
                    (bool)parseJson["BehindHat"]); // BehindHat

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

            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme Visor Manager : VisorData Download Start!! ---------- ");

            string ausFolder = Path.GetDirectoryName(Application.dataPath);
            string dataSaveFolder = string.Concat(ausFolder, FolderPath);

            cleanUpCurSkinData(dataSaveFolder);

            string dlFolder = string.Concat(ausFolder, workingFolder);

            Helper.FileUtility.DeleteDir(dlFolder);
            Directory.CreateDirectory(dlFolder);

            string zipPath = string.Concat(dlFolder, dlZipName);

            yield return Helper.FileUtility.DlToZip(skinDlUrl, zipPath);

            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme Visor Manager : VisorData Download Complete!! ---------- ");

            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme Visor Manager : VisorData Install Start!! ---------- ");

            installVisorData(dlFolder, zipPath, dataSaveFolder);

            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme Visor Manager : VisorData Install Complete!! ---------- ");
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
                http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
                var response = await http.GetAsync(
                    new System.Uri($"{repo}/new_visor/{fileName}"),
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
                    using (var fileStream = File.Create(string.Concat(
                        Path.GetDirectoryName(Application.dataPath), FolderPath, fileName)))
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
                string.Concat(
                    dataSaveFolder, visorRepoData));
            string visorJsonString = System.Text.Encoding.UTF8.GetString(byteVisorArray);

            JObject visorFolder = JObject.Parse(visorJsonString);

            for (int i = 0; i < visorFolder.Count; ++i)
            {
                JProperty token = visorFolder.ChildrenTokens[i].TryCast<JProperty>();
                if (token == null) { continue; }

                string author = token.Name;

                if (author == jsonUpdateComitKey) { continue; }

                string checkVisorFolder = string.Concat(dataSaveFolder, author);

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
            string extractPath = string.Concat(workingDir, "visor");
            ZipFile.ExtractToDirectory(zipPath, extractPath);

            byte[] byteVisorArray = File.ReadAllBytes(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, visorRepoData));
            string visorJsonString = System.Text.Encoding.UTF8.GetString(byteVisorArray);

            JToken visorFolder = JObject.Parse(visorJsonString)["data"];
            JArray visorArray = visorFolder.TryCast<JArray>();

            for (int i = 0; i < visorArray.Count; ++i)
            {
                string visorData = visorArray[i].ToString();

                if (visorData == visorRepoData || visorData == visorTransData) { continue; }

                string visorMoveToFolder = string.Concat(
                    installFolder, @"\", visorData);
                string visorSourceFolder = string.Concat(
                    extractPath, visorDataPath, visorData);

                ExtremeSkinsPlugin.Logger.LogInfo($"Installing Visor:{visorData}");

                Directory.Move(visorSourceFolder, visorMoveToFolder);
            }
        }

    }
#endif
}
