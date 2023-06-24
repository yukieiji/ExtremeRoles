using System.Collections.Generic;

#nullable enable

namespace ExtremeRoles.Module.NewTranslation;

public abstract class Translator
{
	public abstract int Priority { get; }

    public abstract SupportedLangs DefaultLang { get; }

    public bool IsSupport(SupportedLangs languageId)
        => languageId == DefaultLang;

	public abstract Dictionary<string, string> GetTranslation(SupportedLangs languageId);
}
