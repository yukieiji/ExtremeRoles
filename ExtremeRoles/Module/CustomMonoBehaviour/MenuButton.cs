using System;

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using Il2CppInterop.Runtime.Attributes;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class MenuButton : MonoBehaviour
{
    public MenuButton(IntPtr ptr) : base(ptr) { }

    public PassiveButton? Button { get; private set; }
    public TMP_Text? Text { get; private set; }

    public void Awake()
    {
        this.Button = base.gameObject.GetComponent<PassiveButton>();
        this.Button.OnClick = new Button.ButtonClickedEvent();

        this.Text = base.transform.GetChild(0).GetComponent<TMP_Text>();
    }

    public void AddAction(UnityAction action)
    {
        if (this.Button == null) { return; }
        this.Button.OnClick.AddListener(action);
    }

    [HideFromIl2Cpp]
    public void AddAction(Action action)
    {
        AddAction((UnityAction)action);
    }

    public void SetText(string text)
    {
        if (this.Text == null) { return; }
        this.Text.SetText(text);
    }
}
