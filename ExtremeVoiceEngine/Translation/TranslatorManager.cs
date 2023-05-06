using System.Collections.Generic;

using Il2CppCollection = Il2CppSystem.Collections.Generic;

using ExtremeRoles.Performance;

namespace ExtremeVoiceEngine.Translation;

public static class TranslatorManager
{
    private static Translator translator = new Translator();

    public static void AddAditionalTransData(
        Dictionary<string, string> newData)
    {
        if (!TranslationController.InstanceExists)
        {
            ExtremeVoiceEnginePlugin.Logger.LogError($"Can't Add new data");
        }
        AddData(
            FastDestroyableSingleton<TranslationController>.Instance.currentLanguage.AllStrings,
            newData);
    }

    internal static void AddTranslationData(
        SupportedLangs languageId,
        Il2CppCollection.Dictionary<string, string> allData)
    {
        SupportedLangs useLang =
            translator.IsSupport(languageId) ? languageId : translator.DefaultLang;
        AddData(
            allData,
            translator.GetTranslation(useLang));
    }

    private static void AddData(
        Il2CppCollection.Dictionary<string, string> allData,
        Dictionary<string, string> newData)
    {
        foreach (var (key, data) in newData)
        {
            if (allData.ContainsKey(key))
            {
                ExtremeVoiceEnginePlugin.Logger.LogError(
                    $"Detect:Translation Data conflict!!  Key:{key} Data:{data}");
            }
            else
            {
                allData.Add(key, data);
            }
        }
    }
}
