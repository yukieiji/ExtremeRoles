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
    public class ExtremeHatManager
    {
        public static Dictionary<string, CustomHat> HatData = new Dictionary<string, CustomHat>();
        public static bool IsLoaded = false;

        public const string FolderPath = @"\ExtremeHat\";
        public const string InfoFileName = "info.json";
        public const string LicenceFileName = "LICENCE.md";

        private const string repo = "https://raw.githubusercontent.com/yukieiji/ExtremeHats/main"; // When using this repository with Fork, please follow the license of each hat
        private const string hatData = "hatData.json";
        private const string hatTransData = "hatTranData.json";

        /*
            ・リポジトリ構造    
                ・hat/(各種スキンフォルダ)/(各種ファイル)
                ・hat/transData.json
                ・hatData.json
                ・HatTransData.xlsx
            
            ・hatData.json
            {
                data:
                    [(スキン0),(スキン1),(スキン2),]
            }
            // stringのArray

            ・翻訳ロジック
            1.transData.jsonをGitHubから落としてくる(毎回)
            2.そのデータを最初にロードしていたマスターの翻訳データ(色のデータ)に統合及び更新
            
            ・スキンデータ更新チェックロジック
            1.ディレクトリがあるか
            2.hatData.jsonをGitHubから落としてくる(毎回)
            3.hatData.jsonに記載されているフォルダが全てあるか
            4.各種データに破損が無いか
            5.全てパスすれば更新しない

            ・スキンデータダウンロードロジック(全更新)
            1.フォルダが無かったら作る
            2.hatData.jsonをGitHubから落としてくる(毎回)
            3.hatData.jsonに従ってスキンのinfo.json及びLICENCE.mdのDL用URLを動的生成
            4.フォルダを作成(合った場合は消して作る)、LICENCE.mdとinfo.jsonをダウンロードしてそこに置く
            5.info.jsonをロード
            6.info.jsonを元にスキン本体画像のDL用URLを動的生成
            7.DL用URLからデータを落とす
         */


        public static void Initialize()
        {
            HatData.Clear();
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

            JToken hatFolder = JObject.Parse(hatJsonString)["data"];
            JArray hatArray = hatFolder.TryCast<JArray>();

            for (int i = 0; i < hatArray.Count; ++i)
            {
                string checkHatFolder = string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    FolderPath, hatData, @"\", hatArray[i].ToString());

                if (!Directory.Exists(checkHatFolder)) { return true; }

                if(!File.Exists(string.Concat(
                        checkHatFolder, @"\", LicenceFileName))) { return true; }

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
                    byte[] byteArray = File.ReadAllBytes(
                        string.Concat(hat, @"\", InfoFileName));
                    string json = System.Text.Encoding.UTF8.GetString(byteArray);
                    JObject parseJson = JObject.Parse(json);

                    string name = parseJson["Name"].ToString();
                    string productId = string.Concat(
                        "hat_", name);

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
                        $"Skin Loaded:{parseJson.ChildrenTokens[1].TryCast<JProperty>().Value.ToString()}, from:{hat}");
                }
            }

            IsLoaded = true;
        }

        public static void UpdateTranslation()
        {
            foreach (var hat in HatData.Values)
            {
               if (hat.Body != null)
               {
                    hat.Body.name = Helper.Translation.GetString(
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

        private static void downLoad()
        {

        }
    }
}
