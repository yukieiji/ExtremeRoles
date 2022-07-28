using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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

        private const string repo = "https://raw.githubusercontent.com/yukieiji/ExtremeVisor/main"; // When using this repository with Fork, please follow the license of each hat
        private const string skinDlUrl = "https://github.com/yukieiji/ExtremeVisor/archive/refs/heads/main.zip";

        private const string workingFolder = @"\ExVWorking\";
        private const string dlZipName = "ExtremeVisor-main.zip";
        private const string visorDataPath = @"\ExtremeVisor-main\visor\";

        private const string visorRepoData = "visorData.json";
        private const string visorTransData = "visorTransData.json";

        public static void Initialize()
        {
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
            JObject visorFolder = JObject.Parse(visorJsonString);
            
            for(int i = 0; i < visorFolder.Count; ++i)
            {
                JProperty token = visorFolder.ChildrenTokens[i].TryCast<JProperty>();
                if (token == null) { continue; }

                string author = token.Name;

                if (author == "updateComitHash") { continue; }

                string checkVisorFolder = string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, author);

                if (!Directory.Exists(checkVisorFolder)) { return true; }

                if (!File.Exists(string.Concat(
                    checkVisorFolder, @"\", LicenseFileName))) { return true; }

                JArray visorImage = token.Value.TryCast<JArray>();
                for (int j = 0; j < visorImage.Count; ++j)
                {

                    if (!File.Exists(string.Concat(
                            checkVisorFolder, @"\",
                            visorImage[j].TryCast<JValue>().Value.ToString(),
                            ".png"))) { return true; }
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
        
            string[] visorFolder = Directory.GetDirectories(
                string.Concat(Path.GetDirectoryName(Application.dataPath), FolderPath));

            foreach (string authorPath in visorFolder)
            {
                if (string.IsNullOrEmpty(authorPath)) { continue; }
                
                string[] authorDirs = authorPath.Split(@"\");
                string author = authorDirs[authorDirs.Length - 1];

                string[] visorImage = Directory.GetFiles(
                    authorPath, "*.png");

                foreach (string visor in visorImage)
                {
                    string[] visorDir = visor.Split(@"\");
                    string imageName = visorDir[visorDir.Length - 1];
                    string name = imageName.Substring(0, imageName.Length - 4);
                    string productId = string.Concat("visor_", name);

                    if (VisorData.ContainsKey(productId)) { continue; }

                    VisorData.Add(
                        productId,  // Name
                        new CustomVisor(
                            productId, visor,
                            author, name));  // Name

                    ExtremeSkinsPlugin.Logger.LogInfo(
                        $"Visor Loaded:{name}, from:{visor}");
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
            if (!Directory.Exists(dlFolder))
            {
                Directory.CreateDirectory(dlFolder);
            }

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
                    new System.Uri($"{repo}/visor/{fileName}"),
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

            if (!Directory.Exists(dataSaveFolder))
            {
                Directory.CreateDirectory(dataSaveFolder);
            }

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

                if (author == "updateComitHash") { continue; }

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

            JObject visorFolder = JObject.Parse(visorJsonString);

            for (int i = 0; i < visorFolder.Count; ++i)
            {
                JProperty token = visorFolder.ChildrenTokens[i].TryCast<JProperty>();
                if (token == null) { continue; }

                string author = token.Name;

                if (author == "updateComitHash") { continue; }

                string visorMoveToFolder = string.Concat(installFolder, @"\", author);
                string visorSourceFolder = string.Concat(extractPath, visorDataPath, author);

                ExtremeSkinsPlugin.Logger.LogInfo($"Installing Visor:{author} Visors");

                Directory.Move(visorSourceFolder, visorMoveToFolder);
            }
        }

    }
#endif
}
