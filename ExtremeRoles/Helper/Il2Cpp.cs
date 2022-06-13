using System;
using HarmonyLib;
using UnhollowerBaseLib;

namespace ExtremeRoles.Helper
{
    public static class Il2Cpp
    {
        public static object TryCast(this Il2CppObjectBase self, Type type)
        {
            return AccessTools.Method(
                self.GetType(),
                nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type).Invoke(self, Array.Empty<object>());
        }
    }
}
