using System.Collections.Generic;
using System.Collections.Immutable;

namespace ExtremeSkins.Module;

public sealed class CosmicStorage<T>
{
	public static bool IsEmpty => instance is null || instance.data.IsEmpty;
	public static IEnumerable<T> GetAll()
	{
		if (instance is null) { yield break; }
		foreach (var item in instance.data.Values)
		{
			yield return item;
		}
	}

	public static bool TryAdd(string key, T data)
	{
		if (instance is null || instance.data.ContainsKey(key))
		{
			return false;
		}
		var newData = instance.data.SetItem(key, data);
		new CosmicStorage<T>(newData);
		return true;
	}

	public static bool TryGet(string key, out T? data)
	{
		if (instance is null)
		{
			data = default;
			return false;
		}
		return instance.data.TryGetValue(key, out data);
	}

	private static CosmicStorage<T>? instance;
	private readonly ImmutableSortedDictionary<string, T> data;

	public CosmicStorage(in IReadOnlyDictionary<string, T> dict)
	{
		instance = this;
		this.data = dict.ToImmutableSortedDictionary();
	}
}
