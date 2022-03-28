using System.Collections.Generic;
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
        public const string LicenceFileName = "LICENCE.md";

        private const string repo = ""; // When using this repository with Fork, please follow the license of each hat
        private const string hatData = "hatData.json";
        private const string hatTransData = "hatTranData.json";

        /*

            フォルダーデータ構造
                ExtremeNamePlate/namePlateData.json
                ExtremeNamePlate/namePlateTransData.json
                ExtremeNamePlate/(グループ名キー1)/LICENCE.md
                ExtremeNamePlate/(グループ名キー1)/(namePlate1).png
                ExtremeNamePlate/(グループ名キー1)/(namePlate2).png
            
                ExtremeNamePlate/(グループ名キー2)/LICENCE.md
                ExtremeNamePlate/(グループ名キー2)/(namePlate1).png
                ExtremeNamePlate/(グループ名キー2)/(namePlate2).png
            
                ExtremeNamePlate/(グループ名キー3)/LICENCE.md
                ExtremeNamePlate/(グループ名キー2)/(namePlate1).png
                ExtremeNamePlate/(グループ名キー2)/(namePlate2).png
            

            データチェック：
                1. namePlate.jsonを落とす
                2. そこに書いてあるフォルダとデータがあるか確認
                3. 一個でも足りなかったらDL
            データDL
                1. namePlate.jsonを落とす
                2. namePlate.jsonに書いてあるフォルダを作る
                3. namePlate.jsonにデータを落としてくる
            ロード
                1. namePlateTransData.jsonを落とす
                2. フォルダ走査
                3. 各フォルダ内のpngファイル(名前はなんでも可)を検索
                4. Autherを「フォルダ名」、Nameをpngのファイル名としてネームプレート作る
        */



        public static void Initialize()
        {
            NamePlateData.Clear();
            IsLoaded = false;
        }

        public static bool IsUpdate()
        {
            if (!Directory.Exists(string.Concat(
                Path.GetDirectoryName(Application.dataPath), FolderPath))) { return true; }

            getJsonData(hatData).GetAwaiter().GetResult();
            
            byte[] byteHatArray = File.ReadAllBytes(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, hatData));
            string hatJsonString = System.Text.Encoding.UTF8.GetString(byteHatArray);

            return false;
        }

        public static void Load()
        {
            /* 
            getJsonData(hatTransData).GetAwaiter().GetResult();
            Helper.Translation.UpdateHatsTransData(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, @"\", hatTransData));
            */

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

            getJsonData(hatData).GetAwaiter().GetResult();

            byte[] byteHatArray = File.ReadAllBytes(
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, hatData));
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
                    Directory.Delete(getHatFolder, false);;
                }

                Directory.CreateDirectory(getHatFolder);

                await pullHat(getHatFolder, getHatData);

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

        private static async Task<HttpStatusCode> pullHat(
            string saveFolder,
            string hat)
        {
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            // インフォファイルを落とす
            await downLoadFileTo(http, hat, saveFolder, InfoFileName);

            // ライセンスファイルを落としてくる
            await downLoadFileTo(http, hat, saveFolder, LicenceFileName);

            await downLoadFileTo(http, hat, saveFolder, CustomHat.FrontImageName);

            var hatInfoResponse = await http.GetAsync(
                new System.Uri($"{repo}/hat/{hat}/{InfoFileName}"),
                HttpCompletionOption.ResponseContentRead);

            if (hatInfoResponse.Content == null)
            {
                System.Console.WriteLine("Server returned no data: " + hatInfoResponse.StatusCode.ToString());
                return HttpStatusCode.ExpectationFailed;
            }

            string json = await hatInfoResponse.Content.ReadAsStringAsync();
            JObject parseJson = JObject.Parse(json);

            if ((bool)parseJson["FrontFlip"])
            {
                await downLoadFileTo(http, hat, saveFolder, CustomHat.FrontFlipImageName);
            }
            if ((bool)parseJson["Back"])
            {
                await downLoadFileTo(http, hat, saveFolder, CustomHat.BackImageName);
            }
            if ((bool)parseJson["BackFlip"])
            {
                await downLoadFileTo(http, hat, saveFolder, CustomHat.BackFlipImageName);
            }
            if ((bool)parseJson["Climb"])
            {
                await downLoadFileTo(http, hat, saveFolder, CustomHat.ClimbImageName);
            }

            return HttpStatusCode.OK;

        }

        private static async Task<HttpStatusCode> downLoadFileTo(
            HttpClient http, string hat, string saveFolder, string fileName)
        {
            var fileResponse = await http.GetAsync(
                $"{repo}/hat/{hat}/{fileName}",
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
