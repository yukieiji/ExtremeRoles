using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

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
    
    public TextMeshProUGUI AssignText { get; private set; }
    public TextMeshProUGUI AssignNumText { get; private set; }
    public Button IncreseButton { get; private set; }
    public Button DecreseButton { get; private set; }

    public RoleFilterSetProperty(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618
    public void Awake()
    {
        Transform trans = base.transform;

        this.AddRoleButton = trans.Find(
            "Menu/Buttons/AddRoleButton").gameObject.GetComponent<ButtonWrapper>();
        this.DeleteAllRoleButton = trans.Find(
            "Menu/Buttons/RemoveAllButton").gameObject.GetComponent<ButtonWrapper>();
        this.DeleteThisButton = trans.Find(
            "Menu/Buttons/DeleteThisButton").gameObject.GetComponent<ButtonWrapper>();
        this.AddRoleButton.Awake();
        this.DeleteAllRoleButton.Awake();
        this.DeleteThisButton.Awake();

        this.AssignText = trans.Find(
            "Menu/Assign/Text").gameObject.GetComponent<TextMeshProUGUI>();
        this.AssignNumText = trans.Find(
            "Menu/Assign/Group/Num").gameObject.GetComponent<TextMeshProUGUI>();
        this.DecreseButton = trans.Find(
            "Menu/Assign/Group/Decrese").gameObject.GetComponent<Button>();
        this.IncreseButton = trans.Find(
            "Menu/Assign/Group/Increse").gameObject.GetComponent<Button>();

        this.Layout = trans.Find("FillterBody").gameObject.GetComponent<GridLayoutGroup>();
    }
}
