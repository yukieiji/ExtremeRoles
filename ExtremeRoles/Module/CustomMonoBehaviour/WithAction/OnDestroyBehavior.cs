using System;

using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.WithAction;

[Il2CppRegister]
public sealed class OnDestroyBehavior : MonoBehaviour
{
	private Action? destroyAction;

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
