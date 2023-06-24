using System;
using System.Text.RegularExpressions;

using Il2CppObject = Il2CppSystem.Object;

namespace ExtremeRoles.Extension;

public static class TranslationControllerExtension
{
	private static readonly Regex keyCleaner = new Regex(
			@"(<.*?>)|(^-\s*)", RegexOptions.Compiled);
	private static readonly Regex ignoreTransKeyRemover = new Regex(
		@"\|.*?\|", RegexOptions.Compiled);

	public static string GetString(string id, params Il2CppObject[] parts)
        => TranslationController.Instance.GetString(
			cleanStringId(id), defaultStr: string.Empty, parts: parts);

    public static string GetString(string id)
        => TranslationController.Instance.GetString(
			cleanStringId(id), defaultStr: string.Empty, parts: Array.Empty<Il2CppObject>());

    public static string GetString(this TranslationController cont, string id)
		=> cont.GetString(cleanStringId(id), string.Empty, Array.Empty<Il2CppObject>());

#pragma warning disable CS8619
    public static unsafe string GetString(
        this TranslationController cont, string id, params Il2CppObject[] parts)
		=> cont.GetString(cleanStringId(id), defaultStr: string.Empty, parts: parts);
#pragma warning restore CS8619

	private static string cleanStringId(string id)
	{
		id = ignoreTransKeyRemover.Replace(id, string.Empty);
		return keyCleaner.Replace(id, string.Empty).Trim();
	}
}
