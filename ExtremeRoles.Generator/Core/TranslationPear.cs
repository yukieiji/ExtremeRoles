using System;
using System.Collections.Generic;
using System.Text;

namespace ExtremeRoles.Generator.Core;

public static class TranslationPear
{
	public enum Lang
	{
		English,
		Latam,
		Brazilian,
		Portuguese,
		Korean,
		Russian,
		Dutch,
		Filipino,
		French,
		German,
		Italian,
		Japanese,
		Spanish,
		SChinese,
		TChinese,
		Irish
	}

	public static Dictionary<string, Lang> CodeLangPear => new Dictionary<string, Lang>
	{
		{"en-US", Lang.English},
		{"sr-Latn", Lang.Latam},
		{"pt-BR", Lang.Brazilian},
		{"pt-PT", Lang.Portuguese},
		{"ko-KO", Lang.Korean},
		{"ru-RU", Lang.Russian},
		{"nl-NL", Lang.Dutch },
		{"en-PH", Lang.Filipino },
		{"fr-FR", Lang.French },
		{"de-DE", Lang.German },
		{"it-It", Lang.Italian },
		{"ja-JP", Lang.Japanese },
		{"es-ES", Lang.Spanish },
		{"zh-Hans", Lang.SChinese },
		{"zh-Hant", Lang.TChinese },
		{"ga-IE", Lang.Irish }
	};
}
