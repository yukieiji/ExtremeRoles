using UnityEngine.Events;

namespace ExtremeRoles.Extension.UnityEvent;

public static class UnityEventBasExtension
{
    public static void RemoveAllPersistentAndListeners(this UnityEventBase events)
    {
        events.RemoveAllListeners();
        events.m_PersistentCalls.Clear();
    }
}
