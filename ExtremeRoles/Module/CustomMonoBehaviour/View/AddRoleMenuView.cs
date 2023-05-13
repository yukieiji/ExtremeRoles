using System;
using System.Collections.Generic;

using TMPro;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using Il2CppInterop.Runtime.Attributes;

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
    private FilterItemProperty filterItemPrefab;
    private GridLayoutGroup layout;

    private Dictionary<int, ButtonWrapper> allButton;

    public AddRoleMenuView(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618
    public void Awake()
    {
        Transform trans = transform;

        this.Title = trans.Find("Title").GetComponent<TextMeshProUGUI>();

        this.filterItemPrefab = trans.Find(
            "FilterItem").gameObject.GetComponent<FilterItemProperty>();
        this.buttonPrefab = trans.Find("Button").gameObject.GetComponent<ButtonWrapper>();
        this.layout = trans.Find("Scroll/Viewport/Content").gameObject.GetComponent<GridLayoutGroup>();

        var closeButton = trans.Find("CloseButton").gameObject.GetComponent<Button>();
        closeButton.onClick.AddListener(
            (UnityAction)(() => base.gameObject.SetActive(false)));
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            base.gameObject.SetActive(false);
        }
    }

    [HideFromIl2Cpp]
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
            button.gameObject.SetActive(true);
            button.ResetButtonAction();
            button.SetButtonClickAction(createButton(button, model, id));
        }
    }

    [HideFromIl2Cpp]
    private UnityAction createButton(
        ButtonWrapper button, AddRoleMenuModel model, int id)
    {
        if (model.NormalRole.TryGetValue(id, out var normalRoleId))
        {
            string roleName = ExtremeRoleManager.NormalRole[
                (int)normalRoleId].GetColoredRoleName(true);
            button.SetButtonText(roleName);
            return (UnityAction)(() =>
            {
                AddRoleMenuModelUpdater.AddRoleData(model, id, normalRoleId);
                
                base.gameObject.SetActive(false);
                createFilterItem(model, roleName, id);
            });
        }
        else if (model.CombRole.TryGetValue(id, out var combRoleId))
        {
            string combRoleName = ExtremeRoleManager.CombRole[
                (byte)combRoleId].GetOptionName();
            button.SetButtonText(combRoleName);
            return (UnityAction)(() =>
            {
                AddRoleMenuModelUpdater.AddRoleData(model, id, combRoleId);
                
                base.gameObject.SetActive(false);
                createFilterItem(model, combRoleName, id);
            });
        }
        else if (model.GhostRole.TryGetValue(id, out var ghostRoleId))
        {
            string ghostRoleName = ExtremeGhostRoleManager.AllGhostRole[
                ghostRoleId].GetColoredRoleName();
            button.SetButtonText(ghostRoleName);
            return (UnityAction)(() =>
            {
                AddRoleMenuModelUpdater.AddRoleData(model, id, ghostRoleId);
                
                base.gameObject.SetActive(false);
                createFilterItem(model, ghostRoleName, id);
            });
        }
        else
        {
            return (UnityAction)(() => { });
        }
    }

    [HideFromIl2Cpp]
    private void createFilterItem(AddRoleMenuModel model, string name, int id)
    {
        FilterItemProperty item = Instantiate(
            this.filterItemPrefab,
            model.Property.Layout.transform);
        item.gameObject.SetActive(true);
        item.Text.text = name;
        item.RemoveButton.onClick.AddListener(
            (UnityAction)(() =>
            {
                AddRoleMenuModelUpdater.RemoveFilterRole(model, id);
                Destroy(item.gameObject);
            }));
    }
}
