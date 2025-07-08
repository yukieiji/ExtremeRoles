using System;

using Il2CppInterop.Runtime.Attributes;

using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.WithAction;

[Il2CppRegister]
public sealed class OnDestroyBehavior : MonoBehaviour
{
	private Action? destroyAction;

	public OnDestroyBehavior(IntPtr ptr) : base(ptr)
	{
	}

	[HideFromIl2Cpp]
	public void Add(Action @delegate)
	{
		if (destroyAction is null)
		{
			destroyAction = @delegate;
		}
		else
		{
			destroyAction += @delegate;
		}
	}

	public void OnDestroy()
	{
		this.destroyAction?.Invoke();
	}
}
