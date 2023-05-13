using System;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.Model;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using UnityEngine.UI;
using TMPro;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class AddRoleMenuProperty : MonoBehaviour
{
#pragma warning disable CS8618
    public ButtonWrapper ButtonPrefab { get; private set; }
    public TextMeshProUGUI Title { get; private set; }
    public GridLayoutGroup Layout { get; private set; }

    public AddRoleMenuProperty(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618
    public void Awake()
    {
        Transform trans = base.transform;

        this.Title = trans.Find("Title").GetComponent<TextMeshProUGUI>();
        this.ButtonPrefab = trans.Find("Button").gameObject.GetComponent<ButtonWrapper>();
        this.Layout = trans.Find("Scroll/Viewport/Content").gameObject.GetComponent<GridLayoutGroup>();

        var closeButton = trans.Find("CloseButton").gameObject.AddComponent<CloseButtonBehaviour>();
        closeButton.SetHideObject(base.gameObject);
    }
}
