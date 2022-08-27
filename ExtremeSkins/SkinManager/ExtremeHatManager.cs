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
#if WITHHAT
    public static class ExtremeHatManager
    {
        public static readonly Dictionary<string, CustomHat> HatData = new Dictionary<string, CustomHat>();
        public static bool IsLoaded = false;

        public const string FolderPath = @"\ExtremeHat\";
        public const string InfoFileName = "info.json";
        public const string LicenseFileName = "LICENSE.md";

        private const string repo = "https://raw.githubusercontent.com/yukieiji/ExtremeHats/main"; // When using this repository with Fork, please follow the license of each hat
        private const string skinDlUrl = "https://github.com/yukieiji/ExtremeHats/archive/refs/heads/main.zip";
        
        private const string workingFolder = @"\ExHWorking\";
        private const string dlZipName = "ExtremeHats-main.zip";
        private const string hatDataPath = @"\ExtremeHats-main\hat\";
        
        private const string hatRepoData = "hatData.json";
        private const string hatTransData = "hatTranData.json";

        public static void Initialize()
        {
            HatData.Clear();
            IsLoaded = false;
        }

        public static bool IsUpdate()
        {

            ExtremeSkinsPlugin.Logger.LogInfo("Extreme Hat Manager : Checking Update....");

            if (!Directory.Exists(string.Concat(
                Path.GetDirectoryName(Application.dataPath), FolderPath))) { return true; }

            getJsonData(hatRepoData).GetAwaiter().GetResult();
            
            byte[] byteHatArray = File.ReadAllBytes(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, hatRepoData));
            string hatJsonString = System.Text.Encoding.UTF8.GetString(byteHatArray);

            JToken hatFolder = JObject.Parse(hatJsonString)["data"];
            JArray hatArray = hatFolder.TryCast<JArray>();

            for (int i = 0; i < hatArray.Count; ++i)
            {
                string checkHatFolder = string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, @"\", hatArray[i].ToString());

                if (!Directory.Exists(checkHatFolder)) { return true; }

                if(!File.Exists(string.Concat(
                        checkHatFolder, @"\", LicenseFileName))) { return true; }

                if (!File.Exists(string.Concat(
                        checkHatFolder, @"\", InfoFileName))) { return true; }
                if (!File.Exists(string.Concat(
                        checkHatFolder, @"\", CustomHat.FrontImageName))) { return true; }

                byte[] byteArray = File.ReadAllBytes(
                    string.Concat(checkHatFolder, @"\", InfoFileName));
                string json = System.Text.Encoding.UTF8.GetString(byteArray);
                JObject parseJson = JObject.Parse(json);
                    
                if (((bool)parseJson["FrontFlip"]) &&
                    !File.Exists(string.Concat(checkHatFolder, @"\", CustomHat.FrontFlipImageName)))
                {
                    return true;
                }
                if (((bool)parseJson["Back"]) &&
                    !File.Exists(string.Concat(checkHatFolder, @"\", CustomHat.BackImageName)))
                {
                    return true;
                }
                if (((bool)parseJson["BackFlip"]) &&
                    !File.Exists(string.Concat(checkHatFolder, @"\", CustomHat.BackFlipImageName)))
                {
                    return true;
                }
                if (((bool)parseJson["Climb"]) &&
                    !File.Exists(string.Concat(checkHatFolder, @"\", CustomHat.ClimbImageName)))
                {
                    return true;
                }
            }
            return false;
        }

        public static void Load()
        {

            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme Hat Manager : Hat Loading Start!! ----------");

            getJsonData(hatTransData).GetAwaiter().GetResult();
            Helper.Translation.UpdateHatsTransData(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, @"\", hatTransData));

            string[] hatsFolder = Directory.GetDirectories(
                string.Concat(Path.GetDirectoryName(Application.dataPath), FolderPath));

            foreach (string hat in hatsFolder)
            {
                if (!string.IsNullOrEmpty(hat))
                {
                    string infoJsonFile = string.Concat(hat, @"\", InfoFileName);

                    if (!File.Exists(infoJsonFile))
                    {
                        ExtremeSkinsPlugin.Logger.LogInfo(
                            $"Error Detected!!:Can't load info.json for:{infoJsonFile}");
                        continue;
                    }

                    byte[] byteArray = File.ReadAllBytes(infoJsonFile);
                    string json = System.Text.Encoding.UTF8.GetString(byteArray);
                    JObject parseJson = JObject.Parse(json);

                    string name = parseJson["Name"].ToString();
                    string productId = string.Concat("hat_", name);

                    if (HatData.ContainsKey(productId)) { continue; }

                    HatData.Add(
                        productId,  // Name
                        new CustomHat(
                            productId, hat,
                            parseJson["Author"].ToString(),  // Author
                            name,  // Name
                            (bool)parseJson["FrontFlip"],  // FrontFlip
                            (bool)parseJson["Back"],  // Back
                            (bool)parseJson["BackFlip"],  // BackFlip
                            (bool)parseJson["Climb"],  // Climb
                            (bool)parseJson["Bound"],  // Bound
                            (bool)parseJson["Shader"])); // Shader

                    ExtremeSkinsPlugin.Logger.LogInfo(
                        $"Hat Loaded:{parseJson.ChildrenTokens[1].TryCast<JProperty>().Value.ToString()}, from:{hat}");
                }
            }

            IsLoaded = true;
            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme Hat Manager : Hat Loading Complete!! ----------");
        }

        public static IEnumerator InstallData()
        {

            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme Hat Manager : HatData Download Start!! ---------- ");

            string ausFolder = Path.GetDirectoryName(Application.dataPath);
            string dataSaveFolder = string.Concat(ausFolder, FolderPath);

            cleanUpCurSkinData(ausFolder, dataSaveFolder);

            string dlFolder = string.Concat(ausFolder, workingFolder);
            
            Helper.FileUtility.DeleteDir(dlFolder);
            Directory.CreateDirectory(dlFolder);

            string zipPath = string.Concat(dlFolder, dlZipName);

            yield return Helper.FileUtility.DlToZip(skinDlUrl, zipPath);

            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme Hat Manager : HatData Download Complete!! ---------- ");
            
            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme Hat Manager : HatData Install Start!! ---------- ");

            installHatData(dlFolder, zipPath, dataSaveFolder);

            ExtremeSkinsPlugin.Logger.LogInfo("---------- Extreme Hat Manager : HatData Install Complete!! ---------- ");
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
                http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };

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
            string ausFolder, string dataSaveFolder)
        {

            if (!Directory.Exists(dataSaveFolder))
            {
                Directory.CreateDirectory(dataSaveFolder);
            }

            getJsonData(hatRepoData).GetAwaiter().GetResult();

            byte[] byteHatArray = File.ReadAllBytes(
                string.Concat(ausFolder, FolderPath, hatRepoData));
            string hatJsonString = System.Text.Encoding.UTF8.GetString(byteHatArray);

            JToken hatFolder = JObject.Parse(hatJsonString)["data"];
            JArray hatArray = hatFolder.TryCast<JArray>();

            for (int i = 0; i < hatArray.Count; ++i)
            {
                string getHatData = hatArray[i].ToString();

                string getHatFolder = string.Concat(
                    dataSaveFolder, @"\", getHatData);

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
            string extractPath = string.Concat(workingDir, "hats");
            ZipFile.ExtractToDirectory(zipPath, extractPath);

            byte[] byteHatArray = File.ReadAllBytes(
               string.Concat(installFolder, hatRepoData));
            string hatJsonString = System.Text.Encoding.UTF8.GetString(byteHatArray);

            JToken hatFolder = JObject.Parse(hatJsonString)["data"];
            JArray hatArray = hatFolder.TryCast<JArray>();

            for (int i = 0; i < hatArray.Count; ++i)
            {
                string hatData = hatArray[i].ToString();

                if (hatData == hatRepoData || hatData == hatTransData) { continue; }

                string hatMoveToFolder = string.Concat(installFolder, @"\", hatData);
                string hatSourceFolder = string.Concat(extractPath, hatDataPath, hatData);
                
                ExtremeSkinsPlugin.Logger.LogInfo($"Installing Hat:{hatData}");

                Directory.Move(hatSourceFolder, hatMoveToFolder);
            }
        }
    }
#endif
}
