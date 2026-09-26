using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;

namespace ExtremeRoles.Core.Abstract;

public interface IIl2CppObjectProvider
{
	public List<T> GetList<T>();
	public Il2CppStructArray<T> GetStructArray<T>(int size) where T : unmanaged;
	public Il2CppStructArray<T> GetStructArray<T>(T[] array) where T : unmanaged;
}
