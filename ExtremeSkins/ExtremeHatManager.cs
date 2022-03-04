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

                    HatData.Add(
                        parseList[1].TryCast<JProperty>().Value.ToString(),
                        new CustomHat(
                            hat,
                            parseList[0].TryCast<JProperty>().Value.ToString(),
                            parseList[1].TryCast<JProperty>().Value.ToString(),
                            (bool)(parseList[2].TryCast<JProperty>().Value),
                            (bool)(parseList[3].TryCast<JProperty>().Value),
                            (bool)(parseList[4].TryCast<JProperty>().Value),
                            (bool)(parseList[5].TryCast<JProperty>().Value),
                            (bool)(parseList[6].TryCast<JProperty>().Value),
                            (bool)(parseList[7].TryCast<JProperty>().Value)));

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
