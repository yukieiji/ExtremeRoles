using Il2CppSystem.Collections.Generic;

namespace ExtremeRoles.Core.Abstract;

public interface IIl2CppObjectProvider
{
	public List<T> GetList<T>();
}
