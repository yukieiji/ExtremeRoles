using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ExtremeRoles.Extension.UnityEvent;

public static class AddListenerExtension
{
    public static void AddListener(this Button.ButtonClickedEvent events, Delegate delegateFunc)
    {
		events.AddListener((Action)delegateFunc);
    }
}
