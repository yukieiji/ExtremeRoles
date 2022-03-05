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

        public const string FolderPath = @"\ExtremeHat\";
        public const string InfoFileName = "info.json";
        public const string LicenceFileName = "LICENCE.md";

        private const string repo = ""; // When using this repository with Fork, please follow the license of each hat
        private const string hatData = "hatData.json";
        private const string hatTransData = "hatTranData.json";

        /*
            ・リポジトリ構造    
                ・Hat/(各種スキンフォルダ)/(各種ファイル)
                ・Hat/transData.json
                ・Hat/hatData.json
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
        }

        public static bool IsUpdate()
        {
            if (!Directory.Exists(string.Concat(
                Path.GetDirectoryName(Application.dataPath), FolderPath))) { return true; }

            // updateJson().GetAwaiter().GetResult();

            string[] hatsFolder = Directory.GetDirectories(
                string.Concat(Path.GetDirectoryName(Application.dataPath), FolderPath));

            foreach (string hat in hatsFolder)
            {
                if (!string.IsNullOrEmpty(hat))
                {
                    if (!File.Exists(string.Concat(
                            hat, @"\", LicenceFileName))) { return true; }
                    if (!File.Exists(string.Concat(
                            hat, @"\", CustomHat.FrontImageName))) { return true; }

                    byte[] byteArray = File.ReadAllBytes(
                        string.Concat(hat, @"\", InfoFileName));
                    string json = System.Text.Encoding.UTF8.GetString(byteArray);
                    JObject parseJson = JObject.Parse(json);

                    var parseList = parseJson.ChildrenTokens;
                    
                    if ((bool)(parseList[2].TryCast<JProperty>().Value) &&
                        !File.Exists(string.Concat(hat, @"\", CustomHat.FrontFlipImageName)))
                    {
                        return true;
                    }
                    if ((bool)(parseList[3].TryCast<JProperty>().Value) &&
                        !File.Exists(string.Concat(hat, @"\", CustomHat.BackImageName)))
                    {
                        return true;
                    }
                    if ((bool)(parseList[4].TryCast<JProperty>().Value) &&
                        !File.Exists(string.Concat(hat, @"\", CustomHat.BackFlipImageName)))
                    {
                        return true;
                    }
                    if ((bool)(parseList[5].TryCast<JProperty>().Value) &&
                        !File.Exists(string.Concat(hat, @"\", CustomHat.ClimbImageName)))
                    {
                        return true;
                    }

                }
            }
            return false;
        }

        public static void Load()
        {
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
                    var parseList = parseJson.ChildrenTokens;
                    string name = parseList[1].TryCast<JProperty>().Value.ToString();
                    string productId = string.Concat(
                        "hat_", parseList[1].TryCast<JProperty>().Value.ToString());

                    if (HatData.ContainsKey(productId)) { continue; }

                    HatData.Add(
                        productId,  // Name
                        new CustomHat(
                            productId, hat,
                            parseList[0].TryCast<JProperty>().Value.ToString(),  // Author
                            name,  // Name
                            (bool)(parseList[2].TryCast<JProperty>().Value),  // FrontFlip
                            (bool)(parseList[3].TryCast<JProperty>().Value),  // Back
                            (bool)(parseList[4].TryCast<JProperty>().Value),  // BackFlip
                            (bool)(parseList[5].TryCast<JProperty>().Value),  // Climb
                            (bool)(parseList[6].TryCast<JProperty>().Value),  // Bound
                            (bool)(parseList[7].TryCast<JProperty>().Value))); // Shader

                    ExtremeSkinsPlugin.Logger.LogInfo(
                        $"Skin Loaded:{parseJson.ChildrenTokens[1].TryCast<JProperty>().Value.ToString()}, from:{hat}");
                }
            }
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

        private static async Task updateJson()
        {
            try
            {
                HttpClient http = new HttpClient();
                http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
                var response = await http.GetAsync(
                    new System.Uri($"{repo}/hatData.json"),
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
                        Path.GetDirectoryName(Application.dataPath), FolderPath, hatData)))
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
