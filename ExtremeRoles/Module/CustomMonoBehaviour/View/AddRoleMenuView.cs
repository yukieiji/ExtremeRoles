using System;
using System.Collections.Generic;

using TMPro;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;

using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Module.RoleAssign.Model;
using ExtremeRoles.Module.RoleAssign.Update;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.View;

[Il2CppRegister]
public sealed class AddRoleMenuView : MonoBehaviour
{
#pragma warning disable CS8618
    public TextMeshProUGUI Title { get; private set; }

    private ButtonWrapper buttonPrefab;
    private GridLayoutGroup layout;
    private Dictionary<int, ButtonWrapper> allButton;

    public AddRoleMenuView(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618
    public void Awake()
    {
        Transform trans = transform;

        this.Title = trans.Find("Title").GetComponent<TextMeshProUGUI>();

        this.buttonPrefab = trans.Find("Button").gameObject.GetComponent<ButtonWrapper>();
        this.layout = trans.Find("Scroll/Viewport/Content").gameObject.GetComponent<GridLayoutGroup>();

        var closeButton = trans.Find("CloseButton").gameObject.AddComponent<CloseButtonBehaviour>();
        closeButton.SetHideObject(gameObject);
    }

    public void UpdateView(AddRoleMenuModel model)
    {
        if (this.layout.rectChildren.Count == 0)
        {
            this.allButton = new Dictionary<int, ButtonWrapper>();
            // メニューを作る
            foreach (int id in model.Id)
            {
                this.allButton.Add(id, Instantiate(this.buttonPrefab, this.layout.transform));
            }
        }

        foreach (var (id, button) in allButton)
        {
            button.ResetButtonAction();
            button.SetButtonClickAction(createButton(button, model, id));
        }
    }

    private static UnityAction createButton(
        ButtonWrapper button, AddRoleMenuModel model, int id)
    {
        if (model.NormalRole.TryGetValue(id, out var normalRoleId))
        {
            button.SetButtonText(ExtremeRoleManager.NormalRole[
                (int)normalRoleId].GetColoredRoleName(true));
            return (UnityAction)(() =>
            {
                AddRoleMenuModelUpdater.AddRoleData(model, normalRoleId);
            });
        }
        else if (model.CombRole.TryGetValue(id, out var combRoleId))
        {
            button.SetButtonText(ExtremeRoleManager.CombRole[
                (byte)combRoleId].GetOptionName());
            return (UnityAction)(() =>
            {
                AddRoleMenuModelUpdater.AddRoleData(model, combRoleId);
            });
        }
        else if (model.GhostRole.TryGetValue(id, out var ghostRoleId))
        {
            button.SetButtonText(ExtremeGhostRoleManager.AllGhostRole[
                ghostRoleId].GetColoredRoleName());
            return (UnityAction)(() =>
            {
                AddRoleMenuModelUpdater.AddRoleData(model, ghostRoleId);
            });
        }
        else
        {
            return (UnityAction)(() => { });
        }
    }
}
