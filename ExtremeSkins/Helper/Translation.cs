using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

using AmongUs.Data;


namespace ExtremeSkins.Helper
{
    public static class Translation
    {
        private static Dictionary<string, Dictionary<SupportedLangs, string>> stringData = 
            new Dictionary<string, Dictionary<SupportedLangs, string>>();

        private const SupportedLangs defaultLanguage = SupportedLangs.Japanese;
        private const string dataPath = "ExtremeSkins.Resources.LangData.stringData.json";

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

            if (data.TryGetValue(
                    DataManager.Settings.Language.CurrentLanguage,
                    out string transStr))
            {
                return key.Replace(keyClean, transStr);
            }
            else if (data.TryGetValue(defaultLanguage, out string defaultStr))
            {
                return key.Replace(keyClean, defaultStr);
            }

            return key;
        }

        public static void AddKeyTransdata(
            string key, Dictionary<SupportedLangs, string> newData)
        {
            stringData[key] = newData;
        }

        private static void addJsonToTransData(JObject parsed)
        {
            for (int i = 0; i < parsed.Count; i++)
            {
                JProperty prop = parsed.ChildrenTokens[i].TryCast<JProperty>();
                if (prop == null || !prop.HasValues) { continue; }

                string stringName = prop.Name;
                var val = prop.Value.TryCast<JObject>();

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
                stringData[stringName] = strings;
            }
        }

    }
}
