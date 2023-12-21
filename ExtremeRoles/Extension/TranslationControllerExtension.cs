using System;
using System.Text.RegularExpressions;

using Il2CppObject = Il2CppSystem.Object;

#nullable enable

namespace ExtremeRoles.Extension.Translation;

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

    public static string GetString(
        this TranslationController cont, string id, params Il2CppObject[] parts)
		=> cont.GetString(cleanStringId(id), defaultStr: string.Empty, parts: parts);

	public static void AddString(string id, string value)
	{
		TranslationController.Instance.AddString(cleanStringId(id), value);
	}

	public static void AddString(
		this TranslationController cont, string id, string value)
	{
		cont.currentLanguage.AllStrings.Add(cleanStringId(id), value);
	}

	private static string cleanStringId(string id)
	{
		id = ignoreTransKeyRemover.Replace(id, string.Empty);
		return keyCleaner.Replace(id, string.Empty).Trim();
	}
}
