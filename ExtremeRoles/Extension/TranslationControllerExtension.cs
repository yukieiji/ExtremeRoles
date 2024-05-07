using System;

using Il2CppObject = Il2CppSystem.Object;

namespace ExtremeRoles.Extension.Controller;

public static class TranslationControllerExtension
{
    public static string GetString(string id, params Il2CppObject[] parts)
	{
		string result = TranslationController.Instance.GetString(id, defaultStr: string.Empty, parts: parts);
		return string.IsNullOrEmpty(result) ? id : result;
	}

    public static string GetString(string id)
	{
		string result = TranslationController.Instance.GetString(
		   id, defaultStr: string.Empty, parts: Array.Empty<Il2CppObject>());
		return string.IsNullOrEmpty(result) ? id : result;
	}

    public static string GetString(this TranslationController cont, string id)
	{
		string result = cont.GetString(id, string.Empty, Array.Empty<Il2CppObject>());
		return string.IsNullOrEmpty(result) ? id : result;
	}

#pragma warning disable CS8619
	public static unsafe string GetString(
		this TranslationController cont, string id, params Il2CppObject[] parts)
	{
		string result = cont.GetString(id, defaultStr: string.Empty, parts: parts);
		return string.IsNullOrEmpty(result) ? id : result;
	}
#pragma warning restore CS8619
}
