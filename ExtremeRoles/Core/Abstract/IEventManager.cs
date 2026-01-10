using ExtremeRoles.Module.Event;

namespace ExtremeRoles.Core.Abstract
{
    public interface IEventManager
    {
        void Register(ISubscriber subscriber, ModEvent eventType);
        void Invoke(ModEvent eventType);
    }
}
