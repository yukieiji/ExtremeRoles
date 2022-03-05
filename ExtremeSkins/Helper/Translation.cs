using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;


namespace ExtremeSkins.Helper
{
    public class Translation
    {
        private static Dictionary<string, Dictionary<int, string>> stringData = new Dictionary<string, Dictionary<int, string>>();

        private const int defaultLanguage = (int)SupportedLangs.English;
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

        public static string GetString(string key)
        {

            if (stringData.Count == 0)
            {
                return key;
            }

            string keyClean = Regex.Replace(key, "<.*?>", "");
            keyClean = Regex.Replace(keyClean, "^-\\s*", "");
            keyClean = keyClean.Trim();

            if (!stringData.ContainsKey(keyClean))
            {
                return key;
            }

            var data = stringData[keyClean];
            int lang = (int)SaveManager.LastLanguage;

            if (data.ContainsKey(lang))
            {
                return key.Replace(keyClean, data[lang]);
            }
            else if (data.ContainsKey(defaultLanguage))
            {
                return key.Replace(keyClean, data[defaultLanguage]);
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
                    var strings = new Dictionary<int, string>();

                    for (int j = 0; j < (int)SupportedLangs.Irish + 1; j++)
                    {
                        string key = j.ToString();
                        var text = val[key]?.TryCast<JValue>().Value.ToString();

                        if (text != null && text.Length > 0)
                        {
                            strings.Add(j, text);
                        }
                    }

                    stringData.Add(stringName, strings);
                }
            }
        }

    }
}
