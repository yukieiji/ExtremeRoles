using ExtremeRoles.Core.Abstract;
using Il2CppSystem.Collections.Generic;

namespace ExtremeRoles.Core;

public class DefaultIl2CppObjectProvider : IIl2CppObjectProvider
{
	public List<T> GetList<T>()
		=> new List<T>();
}
