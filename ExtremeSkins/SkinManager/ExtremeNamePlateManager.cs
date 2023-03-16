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
#if WITHNAMEPLATE
    public static class ExtremeNamePlateManager
    {
        public static readonly Dictionary<string, CustomNamePlate> NamePlateData = new Dictionary<string, CustomNamePlate>();
        public static bool IsLoaded = false;

        public const string FolderPath = @"\ExtremeNamePlate\";
        public const string InfoFileName = "info.json";
        public const string LicenseFileName = "LICENSE.md";

        private const string repo = "https://raw.githubusercontent.com/yukieiji/ExtremeNamePlate/main"; // When using this repository with Fork, please follow the license of each hat
        private const string skinDlUrl = "https://github.com/yukieiji/ExtremeNamePlate/archive/refs/heads/main.zip";

        private const string workingFolder = @"\ExNWorking\";
        private const string dlZipName = "ExtremeNamePlate-main.zip";
        private const string namePlateDataPath = @"\ExtremeNamePlate-main\namePlate\";

        private const string namePlateRepoData = "namePlateData.json";
        private const string namePlateTransData = "namePlateTransData.json";

        private static ConfigEntry<string> curUpdateHash;
        private const string updateComitKey = "ExNUpdateComitHash";
        private const string jsonUpdateComitKey = "updateComitHash";

        public static void Initialize()
        {
            curUpdateHash = ExtremeSkinsPlugin.Instance.Config.Bind(
                ExtremeSkinsPlugin.SkinComitCategory,
                updateComitKey, "NoHashData");
            NamePlateData.Clear();
            IsLoaded = false;
        }

        public static bool IsUpdate()
        {

            ExtremeSkinsPlugin.Logger.LogInfo("Extreme NamePlate Manager : Checking Update....");

            if (!Directory.Exists(string.Concat(
                Path.GetDirectoryName(Application.dataPath), FolderPath))) { return true; }

            getJsonData(namePlateRepoData).GetAwaiter().GetResult();
            
            byte[] byteNamePlateArray = File.ReadAllBytes(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, namePlateRepoData));
            string namePlateJsonString = System.Text.Encoding.UTF8.GetString(byteNamePlateArray);
            JObject namePlateFolder = JObject.Parse(namePlateJsonString);
            
            for(int i = 0; i < namePlateFolder.Count; ++i)
            {
                JProperty token = namePlateFolder.ChildrenTokens[i].TryCast<JProperty>();
                if (token == null) { continue; }

                string author = token.Name;

                if (author == jsonUpdateComitKey)
                {
                    if ((string)token.Value != curUpdateHash.Value)
                    {
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (author == namePlateRepoData ||
                    author == namePlateTransData) { continue; }

                string checkNamePlateFolder = string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, author);

                if (!Directory.Exists(checkNamePlateFolder)) { return true; }

                if (!File.Exists(string.Concat(
                    checkNamePlateFolder, @"\", LicenseFileName))) { return true; }

                JArray namePlateImage = token.Value.TryCast<JArray>();
                for (int j = 0; j < namePlateImage.Count; ++j)
                {

                    if (!File.Exists(string.Concat(
                            checkNamePlateFolder, @"\",
                            namePlateImage[j].TryCast<JValue>().Value.ToString(),
                            ".png"))) { return true; }
                }

            }

            return false;
        }

        public static void Load()
        {

            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme NamePlate Manager : NamePlate Loading Start!! ----------");

            getJsonData(namePlateTransData).GetAwaiter().GetResult();
            Helper.Translation.UpdateHatsTransData(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, namePlateTransData));

            // UpdateComitHash
            byte[] byteNpArray = File.ReadAllBytes(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, namePlateRepoData));
            curUpdateHash.Value = (string)(JObject.Parse(
                System.Text.Encoding.UTF8.GetString(byteNpArray))[jsonUpdateComitKey]);

            string[] namePlateFolder = Directory.GetDirectories(
                string.Concat(Path.GetDirectoryName(Application.dataPath), FolderPath));

            foreach (string authorPath in namePlateFolder)
            {
                if (string.IsNullOrEmpty(authorPath)) { continue; }
                
                string[] authorDirs = authorPath.Split(@"\");
                string author = authorDirs[^1];

                string[] namePlateImage = Directory.GetFiles(
                    authorPath, "*.png");

                foreach (string namePlate in namePlateImage)
                {
                    string[] namePlateDir = namePlate.Split(@"\");
                    string imageName = namePlateDir[^1];

                    CustomNamePlate customNamePlate = new CustomNamePlate(
                        namePlate, author,
                        imageName.Substring(0, imageName.Length - 4));
                    
                    if (NamePlateData.TryAdd(customNamePlate.Id, customNamePlate))
                    {
                        ExtremeSkinsPlugin.Logger.LogInfo(
                            $"NamePlate Loaded:{customNamePlate.Name}, from:{namePlate}");
                    }
                }
                
            }

            IsLoaded = true;

            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme NamePlate Manager : NamePlate Loading Complete!! ----------");
        }

        public static IEnumerator InstallData()
        {

            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme NamePlate Manager : NamePlateData Download Start!! ---------- ");

            string ausFolder = Path.GetDirectoryName(Application.dataPath);
            string dataSaveFolder = string.Concat(ausFolder, FolderPath);

            cleanUpCurSkinData(dataSaveFolder);

            string dlFolder = string.Concat(ausFolder, workingFolder);

            Helper.FileUtility.DeleteDir(dlFolder);
            Directory.CreateDirectory(dlFolder);

            string zipPath = string.Concat(dlFolder, dlZipName);

            yield return Helper.FileUtility.DlToZip(skinDlUrl, zipPath);

            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme NamePlate Manager : NamePlateData Download Complete!! ---------- ");

            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme NamePlate Manager : NamePlateData Install Start!! ---------- ");

            installVisorData(dlFolder, zipPath, dataSaveFolder);

            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme NamePlate Manager : NamePlateData Install Complete!! ---------- ");
#if RELEASE
            Helper.FileUtility.DeleteDir(dlFolder);
# endif
        }

        public static void UpdateTranslation()
        {
            foreach (var np in NamePlateData.Values)
            {
               if (np.Data != null)
               {
                    np.Data.name = Helper.Translation.GetString(
                        np.Name);
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
                    new System.Uri($"{repo}/namePlate/{fileName}"),
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

            getJsonData(namePlateRepoData).GetAwaiter().GetResult();

            byte[] byteVisorArray = File.ReadAllBytes(
                string.Concat(
                    dataSaveFolder, namePlateRepoData));
            string visorJsonString = System.Text.Encoding.UTF8.GetString(byteVisorArray);

            JObject visorFolder = JObject.Parse(visorJsonString);

            for (int i = 0; i < visorFolder.Count; ++i)
            {
                JProperty token = visorFolder.ChildrenTokens[i].TryCast<JProperty>();
                if (token == null) { continue; }

                string author = token.Name;

                if (author == jsonUpdateComitKey ||
                    author == namePlateRepoData ||
                    author == namePlateTransData) { continue; }

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
            string extractPath = string.Concat(workingDir, "namePlate");
            ZipFile.ExtractToDirectory(zipPath, extractPath);

            byte[] byteNamePlateArray = File.ReadAllBytes(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, namePlateRepoData));
            string namePlateJsonString = System.Text.Encoding.UTF8.GetString(byteNamePlateArray);

            JObject namePlateFolder = JObject.Parse(namePlateJsonString);

            for (int i = 0; i < namePlateFolder.Count; ++i)
            {
                JProperty token = namePlateFolder.ChildrenTokens[i].TryCast<JProperty>();
                if (token == null) { continue; }

                string author = token.Name;

                if (author == jsonUpdateComitKey) { continue; }

                string namePlateMoveToFolder = string.Concat(installFolder, @"\", author);
                string namePlateSourceFolder = string.Concat(extractPath, namePlateDataPath, author);

                ExtremeSkinsPlugin.Logger.LogInfo($"Installing NamePlate:{author} namePlate");

                Directory.Move(namePlateSourceFolder, namePlateMoveToFolder);
            }
        }
    }
#endif
}
