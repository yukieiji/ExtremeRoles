using System.Collections.Generic;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.Event
{
    public class EventManager : IEventManager
    {
        private readonly Dictionary<EventType, List<ISubscriber>> _subscribers =
            new Dictionary<EventType, List<ISubscriber>>();

        public void Register(ISubscriber subscriber, EventType eventType)
        {
            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType] = new List<ISubscriber>();
            }
            _subscribers[eventType].Add(subscriber);
        }

        public void Invoke(EventType eventType)
        {
            if (_subscribers.TryGetValue(eventType, out var eventSubscribers))
            {
                List<ISubscriber> subscribersToRemove = new List<ISubscriber>();
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
    }
}
