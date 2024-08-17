using System;
using System.Text.RegularExpressions;

using Il2CppObject = Il2CppSystem.Object;

#nullable enable

namespace ExtremeRoles.Extension.Controller;

public static class TranslationControllerExtension
{
	private static readonly Regex keyCleaner = new Regex(
			@"(<.*?>)|(^-\s*)", RegexOptions.Compiled);
	private static readonly Regex ignoreTransKeyRemover = new Regex(
		@"\|.*?\|", RegexOptions.Compiled);

	public static string GetString(string id, params Il2CppObject[] parts)
        => TranslationController.Instance.GetString(id.clean(), defaultStr: id, parts: parts);

    public static string GetString(string id)
        => TranslationController.Instance.GetString(
            id.clean(), defaultStr: id, parts: Array.Empty<Il2CppObject>());

    public static string GetString(this TranslationController cont, string id)
        => cont.GetString(id.clean(), id, Array.Empty<Il2CppObject>());

    public static unsafe string GetString(
        this TranslationController cont, string id, params Il2CppObject[] parts)
        => cont.GetString(id.clean(), defaultStr: id, parts: parts);

	private static string clean(this string body)
	{
		string result = ignoreTransKeyRemover.Replace(body, string.Empty);
		result = keyCleaner.Replace(result, string.Empty).Trim();
		return result;
	}
}
