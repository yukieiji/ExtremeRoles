﻿using System;
using System.Reflection;
using HarmonyLib;

using Il2CppInterop.Runtime.Injection;

#nullable enable

namespace ExtremeRoles.Module;

[AttributeUsage(AttributeTargets.Class)]
public sealed class Il2CppRegisterAttribute : Attribute
{
	public Type[] Interfaces { get; private set; }

	public Il2CppRegisterAttribute()
	{
		this.Interfaces = Type.EmptyTypes;
	}

	public Il2CppRegisterAttribute(params Type[] interfaces)
	{
		this.Interfaces = interfaces;
	}

	public static void Registration(Assembly? dll)
	{
		var logger = ExtremeRolesPlugin.Logger;

		if (dll == null)
		{
			logger.LogInfo("This dll is NULL!!");
			return;
		}

		logger.LogInfo(
			"---------- Il2CppRegister: Start Registration ----------");

		foreach (Type type in dll.GetTypes())
		{
			Il2CppRegisterAttribute? attribute =
				CustomAttributeExtensions.GetCustomAttribute<Il2CppRegisterAttribute>(type);
			if (attribute != null)
			{
				RegistrationForTarget(type, attribute.Interfaces);
			}
		}

		logger.LogInfo(
			"---------- Il2CppRegister: Complete Registration ----------");

	}

	public static void RegistrationForTarget(
		Type targetType, Type[] interfaces)
	{
		Type? targetBase = targetType.BaseType;

		Il2CppRegisterAttribute? baseAttribute =
			(targetBase == null) ?
			null :
			CustomAttributeExtensions.GetCustomAttribute<Il2CppRegisterAttribute>(targetBase);

		if (baseAttribute != null)
		{
			RegistrationForTarget(targetBase!, baseAttribute.Interfaces);
		}

		ExtremeRolesPlugin.Logger.LogInfo(
			$"Il2CppRegister:  Register {targetType}");

		if (ClassInjector.IsTypeRegisteredInIl2Cpp(targetType)) { return; }

		try
		{
			ClassInjector.RegisterTypeInIl2Cpp(
				targetType, new RegisterTypeOptions
				{
					Interfaces = interfaces,
					LogSuccess = true
				}
			);
		}
		catch (Exception e)
		{

			string excStr = GeneralExtensions.FullDescription(targetType);
			ExtremeRolesPlugin.Logger.LogError(
				$"Registion Fail!!    Target:{excStr}   Il2CppError:{e}");
		}
	}
}
