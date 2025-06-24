using ExtremeRoles.Module.Event;

namespace ExtremeRoles.Module.Interface
{
    public interface IEventManager
    {
        void Register(ISubscriber subscriber, Event.ModEvent eventType);
        void Invoke(Event.ModEvent eventType);
    }
}
