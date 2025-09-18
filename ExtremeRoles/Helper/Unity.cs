using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;

using UnityObject = UnityEngine.Object;


#nullable enable

namespace ExtremeRoles.Helper;

public static class Unity
{

	public static Nullable<T> CreateNullAble<T>(T value) where T : new()
	{
		var il2CppValue = new Nullable<T>();
		il2CppValue.hasValue = true;
		il2CppValue.value = value;
		return il2CppValue;
	}

	public static Nullable<T> CreateNullAble<T>() where T : new()
	{
		var il2CppValue = new Nullable<T>();
		il2CppValue.hasValue = false;
#pragma warning disable CS8601 // Null 参照代入の可能性があります。
		il2CppValue.value = default;
#pragma warning restore CS8601 // Null 参照代入の可能性があります。

		return il2CppValue;
	}

	public static void FindAndDisableComponent<T>(
		Il2CppArrayBase<T> array, IReadOnlySet<string> disableComponent) where T : Component
	{
		foreach (string name in disableComponent)
		{
			FindAndDisableComponent(array, name);
		}
	}

	public static void FindAndDisableComponent<T>(
		Il2CppArrayBase<T> array, string name) where T : Component
	{
		T? target = array.FirstOrDefault(x => x.gameObject.name == name);
		if (target != null)
		{
			SetColliderActive(target.gameObject, false);
		}
	}

	public static void SetColliderActive(GameObject obj, bool active)
	{
		setColliderEnable<Collider2D>(obj, active);
		setColliderEnable<PolygonCollider2D>(obj, active);
		setColliderEnable<BoxCollider2D>(obj, active);
		setColliderEnable<CircleCollider2D>(obj, active);
	}

	private static void setColliderEnable<T>(GameObject obj, bool active) where T : Collider2D
	{
		if (obj.TryGetComponent<T>(out var comp))
		{
			comp.enabled = active;
		}
	}

	public static void DestroyComponent<T>(GameObject obj) where T : Behaviour
	{
		if (obj.TryGetComponent<T>(out var beha))
		{
			UnityObject.Destroy(beha);
		}
	}
}
