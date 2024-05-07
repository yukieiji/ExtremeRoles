using System.Collections.Generic;

using Il2CppCollection = Il2CppSystem.Collections.Generic;

using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Module;

public interface ITranslator
{
	public SupportedLangs DefaultLang { get; }
	public bool IsSupport(SupportedLangs lang);
	public IReadOnlyDictionary<string, string> Get(SupportedLangs lang);
}

public sealed class ExRTranslator : ITranslator
{
	public SupportedLangs DefaultLang => SupportedLangs.Japanese;
	public bool IsSupport(SupportedLangs lang) => true;

	private const string dataPath = "ExtremeRoles.Resources.JsonData.Language.{0}.json";

	public IReadOnlyDictionary<string, string> Get(SupportedLangs lang)
	{
		var result = Helper.JsonParser.LoadJsonStructFromAssembly<Dictionary<string, string>>(
			string.Format(dataPath, lang.ToString()));
		if (result is null)
		{
			throw new System.ArgumentNullException();
		}
		return result;
	}
}

public static class TranslatorManager
{
	private static readonly List<ITranslator> allTranslator = new List<ITranslator>();

	public static void AddTranslator(ITranslator translator)
	{
		allTranslator.Add(translator);
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
		foreach (var translator in allTranslator)
		{
			SupportedLangs useLang =
				translator.IsSupport(languageId) ?
				languageId : translator.DefaultLang;
			AddData(
				allData,
				translator.Get(useLang));
		}
	}

	private static void AddData(
		Il2CppCollection.Dictionary<string, string> allData,
		IReadOnlyDictionary<string, string> newData)
	{
		foreach (var (key, data) in newData)
		{
			if (!allData.TryAdd(key, data))
			{
				ExtremeRolesPlugin.Logger.LogError(
					$"Detect:Translation Data conflict!!  Key:{key} Data:{data}");
			}
		}
	}
}
