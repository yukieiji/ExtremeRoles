﻿using BepInEx.Logging;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using ExtremeRoles.Module;
using ExtremeRoles.Test.Helper;
using ExtremeRoles.Test.Lobby;
using Il2CppInterop.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ExtremeRoles.Test;

[Il2CppRegister]
public class ExtremeRolesTestPluginBehaviour : MonoBehaviour
{
	public static bool Enable => instance != null;

	public static ExtremeRolesTestPluginBehaviour Instance
	{
		get
		{
			if (instance == null)
			{
				instance = ExtremeRolesTestPlugin.Instance.AddComponent<ExtremeRolesTestPluginBehaviour>();
			}
			return instance;
		}
	}
	private static ExtremeRolesTestPluginBehaviour? instance;

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	public ExtremeRolesTestPluginBehaviour(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

	public static void Register(
		IServiceCollection services,
		Assembly dll)
	{
		foreach (var type in dll.GetTypes())
		{
			if (!typeof(ITestStep).IsAssignableFrom(type) ||
				type.IsInterface || type.IsAbstract)
			{
				continue;
			}
			services.AddTransient(typeof(ITestStep), type);
		}
	}

	public IEnumerator coStart(IEnumerable<ITestStep> testStep)
	{
		var waitor = new WaitForSeconds(1.0f);
		foreach (var step in testStep)
		{
			yield return step.Run();
			yield return GameUtility.WaitForStabilize();
		}
		EndTest();
	}

	public void EndTest()
	{
		ExtremeRolesTestPlugin.Instance.Log.LogInfo("------- END TEST ------");
	}

	public static void Start()
	{
		var provider = ExtremeRolesTestPlugin.Instance.Provider;
		Instance.StartCoroutine(
			Instance.coStart(
				provider.GetServices<ITestStep>()).WrapToIl2Cpp());
	}
}
