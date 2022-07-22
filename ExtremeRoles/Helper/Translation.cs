using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;


namespace ExtremeRoles.Helper
{
    public static class Translation
    {
        private static uint defaultLanguage = (uint)SupportedLangs.English;
        private static Dictionary<string, Dictionary<uint, string>> stringData = new Dictionary<string, Dictionary<uint, string>>();

        private const string dataPath = "ExtremeRoles.Resources.LangData.stringData.json";

        public static void Load()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(dataPath);
            var byteArray = new byte[stream.Length];
            stream.Read(byteArray, 0, (int)stream.Length);
            string json = System.Text.Encoding.UTF8.GetString(byteArray);

            stringData.Clear();
            JObject parsed = JObject.Parse(json);

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
                            strings.Add(j,text);
                        }
                    }

                    stringData.Add(stringName, strings);
                }
            }
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

            if (data.TryGetValue(SaveManager.LastLanguage, out string transData))
            {
                return key.Replace(keyClean, transData);
            }
            else if (data.TryGetValue(defaultLanguage, out string defaultStr))
            {
                return key.Replace(keyClean, defaultStr);
            }

            return key;
        }

    }
}
