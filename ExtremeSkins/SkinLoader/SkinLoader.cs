
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

using ExtremeRoles.Module;

namespace ExtremeSkins.SkinLoader;

public interface ISkinLoader
{
	public IReadOnlyDictionary<string, T> Load<T>() where T : class;
	public IEnumerator Fetch();
}

public sealed class ExtremeSkinLoader : NullableSingleton<ExtremeSkinLoader>
{
	private readonly Dictionary<Type, ISkinLoader> loader = new Dictionary<Type, ISkinLoader>();

	public void AddLoader<C, T>() where T : ISkinLoader, new()
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
