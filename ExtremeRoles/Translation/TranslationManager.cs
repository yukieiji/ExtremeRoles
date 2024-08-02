using System.Collections.Generic;

using Il2CppCollection = Il2CppSystem.Collections.Generic;

using ExtremeRoles.Performance;


namespace ExtremeRoles.Translation;

public static class TranslatorManager
{
	private static List<ITranslator> translator = new ();

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

	public static void Register<T>() where T : ITranslator, new()
	{
		Register(new T());
	}

	public static void Register(ITranslator trans)
	{
		translator.Add(trans);
	}

	internal static void AddTranslationData(
		SupportedLangs languageId,
		Il2CppCollection.Dictionary<string, string> allData)
	{
		foreach (var trans in translator)
		{
			SupportedLangs useLang =
				trans.IsSupport(languageId) ? 
					languageId : trans.DefaultLang;
			AddData(
				allData,
				trans.Get(useLang));
		}
	}

	private static void AddData(
		Il2CppCollection.Dictionary<string, string> allData,
		IReadOnlyDictionary<string, string> newData)
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
