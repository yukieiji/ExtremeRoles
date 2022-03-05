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
