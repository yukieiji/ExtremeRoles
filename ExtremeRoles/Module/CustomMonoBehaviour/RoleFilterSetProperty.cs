using System;

using UnityEngine;

using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using UnityEngine.UI;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class RoleFilterSetProperty : MonoBehaviour
{
#pragma warning disable CS8618
    public ButtonWrapper AddRoleButton { get; private set; }
    public ButtonWrapper DeleteAllRoleButton { get; private set; }
    public ButtonWrapper DeleteThisButton { get; private set; }
    public GridLayoutGroup Layout { get; private set; }

    public RoleFilterSetProperty(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618
    public void Awake()
    {
        Transform trans = base.transform;

        this.AddRoleButton = trans.Find(
            "Buttons/AddRoleButton").gameObject.GetComponent<ButtonWrapper>();
        this.DeleteAllRoleButton = trans.Find(
            "Buttons/RemoveAllButton").gameObject.GetComponent<ButtonWrapper>();
        this.DeleteThisButton = trans.Find(
            "Buttons/DeleteThisButton").gameObject.GetComponent<ButtonWrapper>();
        this.Layout = trans.Find("FillterBody").gameObject.GetComponent<GridLayoutGroup>();
    }
}
