using System;

using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.WithAction;

[Il2CppRegister]
public sealed class OnCloseBehavior : MonoBehaviour
{
	private Action? closeAction;

	public void Add(Delegate @delegate)
	{
		if (closeAction is null)
		{
			closeAction = (Action)@delegate;
		}
		else
		{
			closeAction += (Action)@delegate;
		}
	}

	public void OnClose()
	{
		this.closeAction?.Invoke();
	}
}
