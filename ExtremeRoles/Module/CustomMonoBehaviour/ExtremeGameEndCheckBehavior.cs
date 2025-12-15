using System;

using UnityEngine;
using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.Module.GameEnd;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class ExtremeGameEndCheckBehavior(IntPtr ptr) : MonoBehaviour(ptr)
{
	private ExtremeGameEndChecker? master;

	public void Awake()
	{
		this.master = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<ExtremeGameEndChecker>();
	}

	public void CheckGameEnd()
	{
		this.master?.Check();
	}
}
