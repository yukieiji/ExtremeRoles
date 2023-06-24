using System.Collections.Generic;

#nullable enable

namespace ExtremeRoles.Module.NewTranslation;

public interface ITranslator
{
	public int Priority { get; }

    public SupportedLangs DefaultLang { get; }

    public bool IsSupport(SupportedLangs languageId)
        => languageId == DefaultLang;

	public Dictionary<string, string> GetTranslation(SupportedLangs languageId);
}
