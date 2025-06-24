using System.Collections.Generic;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.Event;

public class EventManager : IEventManager
{
	private readonly Dictionary<EventType, List<ISubscriber>> subscribers = [];

    public void Register(ISubscriber subscriber, EventType eventType)
    {
        if (!subscribers.TryGetValue(eventType, out var eventSubscribers))
        {
			eventSubscribers = [];
        }
		eventSubscribers.Add(subscriber);
		subscribers[eventType] = eventSubscribers;
    }

    public void Invoke(EventType eventType)
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
