using System.Collections.Generic;

namespace ExtremeRoles.Translation;

public interface ITranslator
{
	public SupportedLangs DefaultLang { get; }

	public bool IsSupport(SupportedLangs languageId);

	public IReadOnlyDictionary<string, string> Get(SupportedLangs languageId);
}
