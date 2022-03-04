using System.Collections.Generic;
using System.IO;

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

        public static void Initialize()
        {
            HatData.Clear();
        }

        public static void CheckUpdate()
        {

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
        private static void downLoad()
        {

        }
    }
}
