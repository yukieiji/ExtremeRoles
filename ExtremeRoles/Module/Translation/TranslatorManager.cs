using System.Collections.Generic;

using Il2CppCollection = Il2CppSystem.Collections.Generic;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.NewTranslation;

public static class TranslatorManager
{
    private static SortedList<int, ITranslator> translators = new SortedList<int, ITranslator>();

	public static void Register(ITranslator translator)
	{
		int priority = translator.Priority;
		while(!translators.TryAdd(priority, translator))
		{
			priority++;
		}
	}

    public static void AddAditionalTransData(
        Dictionary<string, string> newData)
    {
        if (!TranslationController.InstanceExists)
        {
            ExtremeRolesPlugin.Logger.LogError($"Can't Add new data");
        }
        AddData(
            FastDestroyableSingleton<TranslationController>.Instance.currentLanguage.AllStrings,
            newData);
    }

    internal static void AddTranslationData(
        SupportedLangs languageId,
        Il2CppCollection.Dictionary<string, string> allData)
    {

		foreach (var translator in translators.Values)
		{
			SupportedLangs useLang =
				translator.IsSupport(languageId) ? languageId : translator.DefaultLang;
			AddData(
				allData,
				translator.GetTranslation(useLang));
		}
    }

    private static void AddData(
        Il2CppCollection.Dictionary<string, string> allData,
        Dictionary<string, string> newData)
    {
        foreach (var (key, data) in newData)
        {
            if (allData.ContainsKey(key))
            {
				ExtremeRolesPlugin.Logger.LogError(
                    $"Detect:Translation Data conflict!!  Key:{key} Data:{data}");
            }
            else
            {
                allData.Add(key, data);
            }
        }
    }
}
