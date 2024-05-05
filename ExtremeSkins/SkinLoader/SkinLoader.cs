
using System;
using System.Collections;
using System.Collections.Generic;

using ExtremeRoles.Module;

namespace ExtremeSkins.SkinLoader;

public abstract class SkinLoader
{
	public abstract IReadOnlyDictionary<string, T> Load<T>();
	public abstract IEnumerator Fetch();
}

public sealed class ExtremeSkinLoader : NullableSingleton<ExtremeSkinLoader>
{
	private readonly Dictionary<Type, SkinLoader> loader = new Dictionary<Type, SkinLoader>();

	public void AddLoader<T>(T loader) where T : SkinLoader
	{
		this.loader.Add(typeof(T), loader);
	}

	public IEnumerator Fetch()
	{
		foreach (var loader in this.loader.Values)
		{
			yield return loader.Fetch();
		}
	}
	public IReadOnlyDictionary<string, T> Load<T>()
	{
		return this.loader[typeof(T)].Load<T>();
	}
}
