using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using UnityEngine;

using Newtonsoft.Json.Linq;


using ExtremeSkins.Module;

namespace ExtremeSkins
{
    public class ExtremeNamePlateManager
    {
        public static Dictionary<string, CustomNamePlate> NamePlateData = new Dictionary<string, CustomNamePlate>();
        public static bool IsLoaded = false;

        public const string FolderPath = @"\ExtremeNamePlate\";
        public const string InfoFileName = "info.json";
        public const string LicenseFileName = "LICENSE.md";

        private const string repo = "https://raw.githubusercontent.com/yukieiji/ExtremeNamePlate/main"; // When using this repository with Fork, please follow the license of each hat
        private const string namePlateData = "namePlateData.json";
        private const string namePlateTransData = "namePlateTransData.json";

        public static void Initialize()
        {
            NamePlateData.Clear();
            IsLoaded = false;
        }

        public static bool IsUpdate()
        {
            if (!Directory.Exists(string.Concat(
                Path.GetDirectoryName(Application.dataPath), FolderPath))) { return true; }

            getJsonData(namePlateData).GetAwaiter().GetResult();
            
            byte[] byteNamePlateArray = File.ReadAllBytes(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, namePlateData));
            string namePlateJsonString = System.Text.Encoding.UTF8.GetString(byteNamePlateArray);
            JObject namePlateFolder = JObject.Parse(namePlateJsonString);
            
            for(int i = 0; i < namePlateFolder.Count; ++i)
            {
                JProperty token = namePlateFolder.ChildrenTokens[i].TryCast<JProperty>();
                if (token == null) { continue; }

                string author = token.Name;

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

            getJsonData(namePlateTransData).GetAwaiter().GetResult();
            Helper.Translation.UpdateHatsTransData(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, namePlateTransData));

            string[] namePlateFolder = Directory.GetDirectories(
                string.Concat(Path.GetDirectoryName(Application.dataPath), FolderPath));

            foreach (string authorPath in namePlateFolder)
            {
                if (string.IsNullOrEmpty(authorPath)) { continue; }
                
                string[] authorDirs = authorPath.Split(@"\");
                string author = authorDirs[authorDirs.Length - 1];

                string[] namePlateImage = Directory.GetFiles(
                    authorPath, "*.png");

                foreach (string namePlate in namePlateImage)
                {
                    string[] namePlateDir = namePlate.Split(@"\");
                    string imageName = namePlateDir[namePlateDir.Length - 1];
                    string name = imageName.Substring(0, imageName.Length - 4);
                    string productId = string.Concat("namePlate_", name);

                    if (NamePlateData.ContainsKey(productId)) { continue; }

                    NamePlateData.Add(
                        productId,  // Name
                        new CustomNamePlate(
                            productId, namePlate,
                            author, name));  // Name

                    ExtremeSkinsPlugin.Logger.LogInfo(
                        $"NamePlate Loaded:{name}, from:{namePlate}");
                }
                
            }

            IsLoaded = true;
        }

        public static async Task PullAllData()
        {

            string dataSaveFolder = string.Concat(
                Path.GetDirectoryName(Application.dataPath), FolderPath);

            if (!Directory.Exists(dataSaveFolder))
            {
                Directory.CreateDirectory(dataSaveFolder);
            }

            getJsonData(namePlateData).GetAwaiter().GetResult();

            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };

            byte[] byteNamePlateArray = File.ReadAllBytes(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, namePlateData));
            string namePlateJsonString = System.Text.Encoding.UTF8.GetString(byteNamePlateArray);

            JObject namePlateFolder = JObject.Parse(namePlateJsonString);

            for (int i = 0; i < namePlateFolder.Count; ++i)
            {
                JProperty token = namePlateFolder.ChildrenTokens[i].TryCast<JProperty>();
                if (token == null) { continue; }

                string author = token.Name;

                if (author == "updateComitHash") { continue; }

                string checkNamePlateFolder = string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, author);

                // まずはフォルダとファイルを消す
                if (Directory.Exists(checkNamePlateFolder))
                {
                    string[] filePaths = Directory.GetFiles(checkNamePlateFolder);
                    foreach (string filePath in filePaths)
                    {
                        File.SetAttributes(filePath, FileAttributes.Normal);
                        File.Delete(filePath);
                    }
                    Directory.Delete(checkNamePlateFolder, false); ;
                }

                Directory.CreateDirectory(checkNamePlateFolder);

                await downLoadFileTo(http, author, checkNamePlateFolder, LicenseFileName);

                JArray namePlateImage = token.Value.TryCast<JArray>();

                for (int j = 0; j < namePlateImage.Count; ++j)
                {

                    string imgName = string.Concat(
                        namePlateImage[j].TryCast<JValue>().Value.ToString(),
                        ".png");

                    await downLoadFileTo(http, author, checkNamePlateFolder, imgName);
                }
            }

        }

        public static void UpdateTranslation()
        {
            foreach (var np in NamePlateData.Values)
            {
               if (np.Body != null)
               {
                    np.Body.name = Helper.Translation.GetString(
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

        private static async Task<HttpStatusCode> downLoadFileTo(
            HttpClient http, string author, string saveFolder, string fileName)
        {
            var fileResponse = await http.GetAsync(
                $"{repo}/namePlate/{author}/{fileName}",
                HttpCompletionOption.ResponseContentRead);

            if (fileResponse.StatusCode != HttpStatusCode.OK)
            {
                ExtremeSkinsPlugin.Logger.LogInfo($"Can't load {fileName}");
                return fileResponse.StatusCode;
            }

            if (fileResponse.Content == null)
            {
                ExtremeSkinsPlugin.Logger.LogInfo("Server returned no data: " + fileResponse.StatusCode.ToString());
                return HttpStatusCode.ExpectationFailed;
            }

            using (var responseStream = await fileResponse.Content.ReadAsStreamAsync())
            {
                using (var fileStream = File.Create($"{saveFolder}\\{fileName}"))
                {
                    responseStream.CopyTo(fileStream);
                }
            }

            return HttpStatusCode.OK;

        }

    }
}
