using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Il2CppInterop.Runtime.InteropTypes.Arrays;


#nullable enable

namespace ExtremeRoles.Helper;

public static class Unity
{
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
			Object.Destroy(beha);
		}
	}
}
