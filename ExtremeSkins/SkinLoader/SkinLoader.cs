
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

using ExtremeRoles.Module;

namespace ExtremeSkins.SkinLoader;

public abstract class SkinLoader
{
	public abstract IReadOnlyDictionary<string, T> Load<T>() where T : class;
	public abstract IEnumerator Fetch();
}

public sealed class ExtremeSkinLoader : NullableSingleton<ExtremeSkinLoader>
{
	private readonly Dictionary<Type, SkinLoader> loader = new Dictionary<Type, SkinLoader>();

	public void AddLoader<C, T>() where T : SkinLoader, new()
	{
		this.loader.Add(typeof(C), new T());
	}

	public IEnumerator Fetch()
	{
		foreach (var loader in this.loader.Values)
		{
			yield return loader.Fetch();
		}
	}

	public IReadOnlyDictionary<string, T> Load<T>() where T : class
	{
		return this.loader[typeof(T)].Load<T>();
	}
}
