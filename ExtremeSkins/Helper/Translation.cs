using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;


namespace ExtremeSkins.Helper
{
    public static class Translation
    {
        private static Dictionary<string, Dictionary<uint, string>> stringData = new Dictionary<string, Dictionary<uint, string>>();

        private const uint defaultLanguage = (uint)SupportedLangs.English;
        private const string dataPath = "ExtremeSkins.Resources.LangData.stringData.json";

        public static void Initialize()
        {
            stringData.Clear();
        }


        public static void CreateColorTransData()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(dataPath);
            var byteArray = new byte[stream.Length];
            stream.Read(byteArray, 0, (int)stream.Length);
            string json = System.Text.Encoding.UTF8.GetString(byteArray);

            JObject parsed = JObject.Parse(json);
            addJsonToTransData(parsed);
        }

        public static void UpdateHatsTransData(string path)
        {
            byte[] byteArray = File.ReadAllBytes(path);
            string json = System.Text.Encoding.UTF8.GetString(byteArray);
            JObject parseJson = JObject.Parse(json);
            addJsonToTransData(parseJson);
        }

        public static string GetString(string key)
        {

            if (stringData.Count == 0)
            {
                return key;
            }

            string keyClean = Regex.Replace(key, "<.*?>", "");
            keyClean = Regex.Replace(keyClean, "^-\\s*", "");
            keyClean = keyClean.Trim();

            if (!stringData.TryGetValue(keyClean, out var data))
            {
                return key;
            }

            if (data.TryGetValue(SaveManager.LastLanguage, out string transStr))
            {
                return key.Replace(keyClean, transStr);
            }
            else if (data.TryGetValue(defaultLanguage, out string defaultStr))
            {
                return key.Replace(keyClean, defaultStr);
            }

            return key;
        }

        private static void addJsonToTransData(JObject parsed)
        {
            for (int i = 0; i < parsed.Count; i++)
            {
                JProperty token = parsed.ChildrenTokens[i].TryCast<JProperty>();
                if (token == null) { continue; }

                string stringName = token.Name;
                var val = token.Value.TryCast<JObject>();

                if (token.HasValues)
                {
                    var strings = new Dictionary<uint, string>();

                    for (uint j = 0; j < (uint)SupportedLangs.Irish + 1; j++)
                    {
                        string key = j.ToString();
                        var text = val[key]?.TryCast<JValue>().Value.ToString();

                        if (text != null && text.Length > 0)
                        {
                            strings.Add(j, text);
                        }
                    }

                    if (stringData.ContainsKey(stringName))
                    {
                        stringData[stringName] = strings;
                    }
                    else
                    {
                        stringData.Add(stringName, strings);
                    }
                }
            }
        }

    }
}
