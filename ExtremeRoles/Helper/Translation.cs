using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

using AmongUs.Data;

namespace ExtremeRoles.Helper
{
    public static class Translation
    {
        private static Dictionary<string, Dictionary<SupportedLangs, string>> stringData = 
            new Dictionary<string, Dictionary<SupportedLangs, string>>();

        private const SupportedLangs defaultLanguage = SupportedLangs.Japanese;
        private const string dataPath = "ExtremeRoles.Resources.JsonData.Language.json";

        public static void Load()
        {
            stringData.Clear();
            JObject parsed = JsonParser.GetJObjectFromAssembly(dataPath);

            for (int i = 0; i < parsed.Count; i++)
            {
                JProperty prop = parsed.ChildrenTokens[i].TryCast<JProperty>();

                if (prop == null || !prop.HasValues) { continue; }

                string stringName = prop.Name;
                JObject val = prop.Value.TryCast<JObject>();

                var strings = new Dictionary<SupportedLangs, string>();

                foreach (SupportedLangs langs in Enum.GetValues(typeof(SupportedLangs)))
                {
                    if (val.TryGetValue(((uint)langs).ToString(), out JToken token))
                    {
                        string text = token.TryCast<JValue>().Value.ToString();
                        if (text.Length > 0)
                        {
                            strings.Add(langs, text);
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

            if (data.TryGetValue(
                    DataManager.Settings.Language.CurrentLanguage,
                    out string transData))
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
