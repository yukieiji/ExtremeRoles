using ExtremeRoles.Module.Event;

namespace ExtremeRoles.Module.Interface
{
    public interface IEventManager
    {
        void Register(ISubscriber subscriber, ModEvent eventType);
        void Invoke(ModEvent eventType);
    }
}
