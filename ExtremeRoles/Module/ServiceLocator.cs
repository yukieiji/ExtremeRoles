using System;
using System.Collections.Generic;

namespace ExtremeRoles.Module;

public sealed class ServiceLocator<TInterface>
{
    private Dictionary<Type, TInterface> service = new Dictionary<Type, TInterface>();

    public ServiceLocator()
    {
        this.service.Clear();
    }

    public IEnumerable<TInterface> GetAllService() => this.service.Values;

    public void Register<TInstance>(TInstance instance) where TInstance : TInterface, new()
    {
        this.service.Add(typeof(TInstance), instance);
    }

    public TTarget Resolve<TTarget>() where TTarget : class, TInterface, new()
    {
        if (this.service.TryGetValue(typeof(TTarget), out TInterface instance))
        {
            return instance as TTarget;
        }
        else
        {
            TTarget newInstance = new TTarget();
            Register(newInstance);
            return newInstance;
        }

    }
}
