using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using Il2CppObject = Il2CppSystem.Object;

namespace ExtremeVoiceEngine.Extension;

public static class StringIdExtension
{
    public static string GetString(this TranslationController cont, string id)
        => cont.GetString(id, string.Empty, Array.Empty<Il2CppObject>());

#pragma warning disable CS8619
    public static string GetString(
        this TranslationController cont, string id, params object[] parts)
        => cont.GetString(
            id, defaultStr: string.Empty,
            parts: (Il2CppReferenceArray<Il2CppObject>)parts);
#pragma warning restore CS8619
}
