using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

namespace ExtremeRoles.Module;

#nullable enable

public sealed class ServiceLocator<TInterface>
{
	private ConcurrentDictionary<Type, TInterface> service = new ConcurrentDictionary<Type, TInterface>();

	public ServiceLocator()
	{
		this.service.Clear();
	}

	public IEnumerable<TInterface> GetAllService() => this.service.Values;

	public void Register<TInstance>() where TInstance : TInterface, new()
	{
		Register(new TInstance());
	}

	public void Register<TInstance>(TInstance instance) where TInstance : TInterface, new()
	{
		if (!this.service.TryAdd(typeof(TInstance), instance))
		{
			ExtremeRolesPlugin.Logger.LogError("This instance already added!!");
		}
	}

	public TTarget Resolve<TTarget>() where TTarget : class, TInterface, new()
	{
		if (this.service.TryGetValue(typeof(TTarget), out TInterface? instance) &&
			instance is TTarget castedInstance)
		{
			return castedInstance;
		}
		else
		{
			TTarget newInstance = new TTarget();
			Register(newInstance);
			return newInstance;
		}

	}
}
