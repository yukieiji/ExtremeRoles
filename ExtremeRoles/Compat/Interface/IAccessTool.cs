using System;
using System.Reflection;

namespace ExtremeRoles.Compat.Interface;

public interface IAccessTool
{
	public Type GetClass(Assembly assembly, string name);
	public MethodInfo GetMethod(Assembly assembly, string className, string methodName, Type[]? param = null);
	public MethodInfo GetMethod(Type fromType, string methodName, Type[]? param = null);
	public FieldInfo GetField(Type fromType, string fieldName);
	public PropertyInfo GetProperty(Type fromType, string propertyName);
}
