using System.Collections.Generic;

#nullable enable

namespace ExtremeRoles.Extension.Linq;

public static class LinqExtend
{
	public static T GetRandomItem<T>(this IReadOnlyList<T> self)
	{
		int size = self.Count;
		int index = RandomGenerator.Instance.Next(size);
		return self[index];
	}
	public static T GetRandomItem<T>(this T[] self)
	{
		int size = self.Length;
		int index = RandomGenerator.Instance.Next(size);
		return self[index];
	}
}
