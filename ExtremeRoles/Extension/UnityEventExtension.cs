using System;

using UnityEngine.UI;
using UnityEngine.Events;

namespace ExtremeRoles.Extension.UnityEvents;

public static class UnityEventBaseExtension
{
    public static void RemoveAllPersistentAndListeners(this UnityEventBase events)
    {
        events.RemoveAllListeners();
        events.m_PersistentCalls.Clear();
    }
}

public static class AddListenerExtention
{
	public static void AddListener(this Button.ButtonClickedEvent events, Delegate delegateFunc)
	{
		events.AddListener((Action)delegateFunc);
	}

	public static void AddListener(this UnityEvent events, Delegate delegateFunc)
	{
		events.AddListener((Action)delegateFunc);
	}
}
