using System;

using UnityEngine;

using ExtremeRoles.Core;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class AutoDisabler : MonoBehaviour
{
	public AutoDisabler(IntPtr ptr) : base(ptr)
	{
	}

	public void OnEnable()
	{
		this.gameObject.SetActive(false);
	}
}
