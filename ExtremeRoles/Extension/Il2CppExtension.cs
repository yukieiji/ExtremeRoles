using System;
using HarmonyLib;

using UnityEngine;

using Il2CppInterop.Runtime.InteropTypes;

using Il2CppType = Il2CppSystem.Type;
using System.Diagnostics.CodeAnalysis;


#nullable enable

namespace ExtremeRoles.Extension.Il2Cpp;

public static class Il2CppExtension
{
	public static object? TryCast(this Il2CppObjectBase self, Type type)
	{
		return AccessTools.Method(
			self.GetType(),
			nameof(Il2CppObjectBase.TryCast))
			.MakeGenericMethod(type)
			.Invoke(self, Array.Empty<object>());
	}
	public static bool IsTryCast<T>(this Il2CppObjectBase? self, [NotNullWhen(true)] out T? obj) where T : Il2CppObjectBase
	{
		if (self == null)
		{
			obj = null;
			return false;
		}
		obj = self.TryCast<T>();
		return obj != null;
	}

	public static Component TryAddComponent(this GameObject obj, Il2CppType type)
	{
		return obj.TryGetComponent(type, out var comp) ? comp : obj.AddComponent(type);
	}

	public static T TryAddComponent<T>(this GameObject obj) where T : Component
	{
		return obj.TryGetComponent<T>(out var comp) ? comp : obj.AddComponent<T>();
	}
}
