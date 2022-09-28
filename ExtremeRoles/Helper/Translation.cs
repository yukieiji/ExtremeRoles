using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

namespace ExtremeRoles.Helper
{
    public static class Translation
    {
        private static uint defaultLanguage = (uint)SupportedLangs.English;
        private static Dictionary<string, Dictionary<uint, string>> stringData = new Dictionary<string, Dictionary<uint, string>>();

        private const string dataPath = "ExtremeRoles.Resources.JsonData.Language.json";

        public static void Load()
        {
            stringData.Clear();
            JObject parsed = JsonParser.GetJObjectFromAssembly(dataPath);

            uint lastLang = (uint)SupportedLangs.Irish;

            for (int i = 0; i < parsed.Count; i++)
            {
                JProperty prop = parsed.ChildrenTokens[i].TryCast<JProperty>();

                if (prop == null || !prop.HasValues) { continue; }

                string stringName = prop.Name;
                JObject val = prop.Value.TryCast<JObject>();

                var strings = new Dictionary<uint, string>();

                for (uint j = 0; j <= lastLang; j++)
                {
                    if (val.TryGetValue(j.ToString(), out JToken token))
                    {
                        string text = token.TryCast<JValue>().Value.ToString();
                        if (text.Length > 0)
                        {
                            strings.Add(j, text);
                        }
                    }
                }

                stringData.Add(stringName, strings);
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
