using System.Collections.Generic;
using System.Linq;

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

	public static IEnumerable<T> GetRandomItem<T>(this IReadOnlyList<T> self, int num)
	{
		if (self.Count >= num)
		{
			return self.OrderBy(x => RandomGenerator.Instance.Next()).Take(num);

		}

		return sizedRandom(self, num);
	}

	private static IEnumerable<T> sizedRandom<T>(IReadOnlyList<T> self, int num)
	{
		for (int i = 0; i < num; ++i)
		{
			yield return self.GetRandomItem();
		}
	}

	public static T GetRandomItem<T>(this T[] self)
	{
		int size = self.Length;
		int index = RandomGenerator.Instance.Next(size);
		return self[index];
	}
}
