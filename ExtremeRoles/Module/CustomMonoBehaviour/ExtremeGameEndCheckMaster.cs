using System;

using UnityEngine;
using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.Module.GameEnd;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class ExtremeGameEndCheckBehavior(IntPtr ptr) : MonoBehaviour(ptr)
{
	public ExtremeGameEndChecker? Master { get; private set; }

	public void Awake()
	{
		this.Master = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<ExtremeGameEndChecker>();
	}

	public void CheckGameEnd()
	{
		this.Master?.Check();
	}
}
