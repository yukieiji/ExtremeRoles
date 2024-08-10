using System;
using System.Collections.Generic;

namespace ExtremeRoles.Translation;

internal sealed class Translator : ITranslator
{
	public SupportedLangs DefaultLang => SupportedLangs.Japanese;

	public IReadOnlyDictionary<string, string> Get(SupportedLangs languageId)
		=> (languageId) switch
		{ 
			SupportedLangs.English => TranslationRawData.English,
			SupportedLangs.Latam => TranslationRawData.Latam,
			SupportedLangs.Brazilian => TranslationRawData.Brazilian,
			SupportedLangs.Portuguese => TranslationRawData.Portuguese,
			SupportedLangs.Korean => TranslationRawData.Korean,
			SupportedLangs.Russian => TranslationRawData.Russian,
			SupportedLangs.Dutch => TranslationRawData.Dutch,
			SupportedLangs.Filipino => TranslationRawData.Filipino,
			SupportedLangs.French => TranslationRawData.French,
			SupportedLangs.German => TranslationRawData.German,
			SupportedLangs.Italian => TranslationRawData.Italian,
			SupportedLangs.Japanese => TranslationRawData.Japanese,
			SupportedLangs.Spanish => TranslationRawData.Spanish,
			SupportedLangs.SChinese => TranslationRawData.SChinese,
			SupportedLangs.TChinese => TranslationRawData.TChinese,
			SupportedLangs.Irish => TranslationRawData.Irish,
			_ => throw new ArgumentException(),
		};

	public bool IsSupport(SupportedLangs languageId)
		=> true;
}
