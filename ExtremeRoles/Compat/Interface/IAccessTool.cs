using System;
using System.Reflection;

#nullable enable

namespace ExtremeRoles.Compat.Interface;

public interface IAccessTool
{
	public Type[] GetTypesFromAssembly(Assembly assembly);

	public MethodInfo GetMethod(Type fromType, string methodName, Type[]? param = null);
	public FieldInfo GetField(Type fromType, string fieldName);
	public PropertyInfo GetProperty(Type fromType, string propertyName);
}
