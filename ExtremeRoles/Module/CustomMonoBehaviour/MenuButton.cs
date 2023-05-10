using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

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

    public void AddAction(Action action)
    {
        AddAction((UnityAction)action);
    }

    public void SetText(string text)
    {
        if (this.Text == null) { return; }

        StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) =>
        {
            this.Text.SetText(text);
        })));
    }
}
