using ExtremeRoles.Module.Event;

namespace ExtremeRoles.Module.Interface
{
    public interface IEventManager
    {
        void Register(ISubscriber subscriber, EventType eventType);
        void Invoke(EventType eventType);
    }
}
