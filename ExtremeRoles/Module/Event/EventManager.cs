using System.Collections.Generic;
using ExtremeRoles.Module.Interface;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace ExtremeRoles.Module.Event;

public sealed class EventManager : IEventManager
{
	public static IEventManager Instance
	{
		get
		{
			if (instance is null)
			{
				instance = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IEventManager>();
			}
			return instance;
		}
	}

	private static IEventManager? instance;

	private readonly Dictionary<ModEvent, List<ISubscriber>> subscribers = [];

    public void Register(ISubscriber subscriber, ModEvent eventType)
    {
        if (!subscribers.TryGetValue(eventType, out var eventSubscribers))
        {
			eventSubscribers = [];
			subscribers[eventType] = eventSubscribers;
		}
		eventSubscribers.Add(subscriber);
    }

    public void Invoke(ModEvent eventType)
    {
        if (!subscribers.TryGetValue(eventType, out var eventSubscribers))
        {
			return;
        }

		eventSubscribers.RemoveAll(subscriber => !subscriber.Invoke());
	}
}
