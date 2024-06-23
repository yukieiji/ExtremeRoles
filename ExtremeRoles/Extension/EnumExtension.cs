using System;
using System.Runtime.CompilerServices;

namespace ExtremeRoles.Extension;

public static class EnumExtension
{
	public static int FastInt<T>(this T value) where T : Enum
	{
		int result = Unsafe.As<T, int>(ref value);
		return result;
	}
}
