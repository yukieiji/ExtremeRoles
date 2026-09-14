using ExtremeRoles.Compat.Interface;
using HarmonyLib;
using Sentry.Unity.NativeUtils;
using System;
using System.Linq;
using System.Reflection;


#nullable enable

namespace ExtremeRoles.Compat;

public class AccessToolWrapper : IAccessTool
{
	public Type[] GetTypesFromAssembly(Assembly assembly)
		=> AccessTools.GetTypesFromAssembly(assembly);

	public MethodInfo GetMethod(Type fromType, string methodName, Type[]? param = null)
		=> AccessTools.Method(fromType, methodName, param);

	public FieldInfo GetField(Type fromType, string fieldName)
		=> AccessTools.Field(fromType, fieldName);

	public PropertyInfo GetProperty(Type fromType, string propertyName)
		=> AccessTools.Property(fromType, propertyName);
}
