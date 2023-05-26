using System;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class FilterItemProperty : MonoBehaviour
{
#pragma warning disable CS8618
    public TextMeshProUGUI Text { get; private set; }
    public Button RemoveButton { get; private set; }

    public FilterItemProperty(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618
    public void Awake()
    {
        Transform trans = base.transform;

        this.Text = trans.Find("RoleName").gameObject.GetComponent<TextMeshProUGUI>();
        this.RemoveButton = trans.Find("CloseButton").gameObject.GetComponent<Button>();
    }
}
