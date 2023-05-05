using System;
using System.Runtime.CompilerServices;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using Il2CppObject = Il2CppSystem.Object;

using ExtremeRoles.Extension;

namespace ExtremeVoiceEngine.Extension;

public static class StringIdExtension
{
    public static string GetString(this TranslationController cont, string id)
        => cont.GetString(id, string.Empty, Array.Empty<Il2CppObject>());

#pragma warning disable CS8619
    public static unsafe string GetString(
        this TranslationController cont, string id, params Il2CppObject[] parts)
    {
        return cont.GetString(
            id, defaultStr: string.Empty,
            parts: parts);
    }
#pragma warning restore CS8619
}
