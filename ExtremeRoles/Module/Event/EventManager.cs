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
        }
		eventSubscribers.Add(subscriber);
		subscribers[eventType] = eventSubscribers;
    }

    public void Invoke(ModEvent eventType)
    {
        if (!subscribers.TryGetValue(eventType, out var eventSubscribers))
        {
			return;
        }

		var subscribersToRemove = new List<ISubscriber>();
		foreach (var subscriber in eventSubscribers)
		{
			if (!subscriber.Invoke())
			{
				subscribersToRemove.Add(subscriber);
			}
		}

		foreach (var subscriber in subscribersToRemove)
		{
			eventSubscribers.Remove(subscriber);
		}
	}
}
