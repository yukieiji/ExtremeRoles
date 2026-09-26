using ExtremeRoles.Core.Abstract;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;

namespace ExtremeRoles.Core;

public class DefaultIl2CppObjectProvider : IIl2CppObjectProvider
{
	public List<T> GetList<T>()
		=> new List<T>();

	public Il2CppStructArray<T> GetStructArray<T>(int size) where T : unmanaged
		=> new Il2CppStructArray<T>(size);

	public Il2CppStructArray<T> GetStructArray<T>(T[] array) where T : unmanaged
		=> new Il2CppStructArray<T>(array);
}
