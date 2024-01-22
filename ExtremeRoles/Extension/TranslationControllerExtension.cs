using System;

using Il2CppObject = Il2CppSystem.Object;

namespace ExtremeRoles.Extension.Controller;

public static class TranslationControllerExtension
{
    public static string GetString(string id, params Il2CppObject[] parts)
        => TranslationController.Instance.GetString(id, defaultStr: string.Empty, parts: parts);

    public static string GetString(string id)
        => TranslationController.Instance.GetString(
            id, defaultStr: string.Empty, parts: Array.Empty<Il2CppObject>());

    public static string GetString(this TranslationController cont, string id)
        => cont.GetString(id, string.Empty, Array.Empty<Il2CppObject>());

#pragma warning disable CS8619
    public static unsafe string GetString(
        this TranslationController cont, string id, params Il2CppObject[] parts)
        => cont.GetString(id, defaultStr: string.Empty, parts: parts);
#pragma warning restore CS8619
}
