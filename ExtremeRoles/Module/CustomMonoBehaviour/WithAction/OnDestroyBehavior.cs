using System;

using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.WithAction;

[Il2CppRegister]
public sealed class OnDestroyBehavior : MonoBehaviour
{
	private Action? destroyAction;

	public void Add(Delegate @delegate)
	{
		if (destroyAction is null)
		{
			destroyAction = (Action)@delegate;
		}
		else
		{
			destroyAction += (Action)@delegate;
		}
	}

	public void OnDestroy()
	{
		this.destroyAction?.Invoke();
	}
}
