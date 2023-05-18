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
    public FilterItemProperty FilterItemPrefab { get; private set; }

    private ButtonWrapper buttonPrefab;
    private GridLayoutGroup layout;

    private Dictionary<int, ButtonWrapper> allButton;

    public AddRoleMenuView(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618
    public void Awake()
    {
        Transform trans = transform;

        this.Title = trans.Find("Title").GetComponent<TextMeshProUGUI>();

        this.FilterItemPrefab = trans.Find(
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
    public void UpdateView(
        RoleAssignFilterModel model, Guid filterId,
        Transform targetFilterTransform)
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

        foreach (var (id, button) in this.allButton)
        {
            button.gameObject.SetActive(true);
            button.ResetButtonAction();
            button.SetButtonClickAction(
                (UnityAction)createButton(
                    button, model, filterId,
                    id, targetFilterTransform));
        }
    }

    [HideFromIl2Cpp]
    private Action createButton(
        ButtonWrapper button, RoleAssignFilterModel model,
        Guid filterId, int id, Transform targetFilterTransform)
    {
        if (model.NormalRole.TryGetValue(id, out var normalRoleId))
        {
            string roleName = ExtremeRoleManager.NormalRole[
                (int)normalRoleId].GetColoredRoleName(true);
            button.SetButtonText(roleName);
            return () =>
            {
                base.gameObject.SetActive(false);
                if (RoleAssignFilterModelUpdater.AddRoleData(model, filterId, id, normalRoleId))
                {
                    createFilterItem(model, roleName, filterId, id, targetFilterTransform);
                }
            };
        }
        else if (model.CombRole.TryGetValue(id, out var combRoleId))
        {
            string combRoleName = ExtremeRoleManager.CombRole[
                (byte)combRoleId].GetOptionName();
            button.SetButtonText(combRoleName);
            return () =>
            {
                base.gameObject.SetActive(false);
                if (RoleAssignFilterModelUpdater.AddRoleData(model, filterId, id, combRoleId))
                {
                    createFilterItem(model, combRoleName, filterId, id, targetFilterTransform);
                }
            };
        }
        else if (model.GhostRole.TryGetValue(id, out var ghostRoleId))
        {
            string ghostRoleName = ExtremeGhostRoleManager.AllGhostRole[
                ghostRoleId].GetColoredRoleName();
            button.SetButtonText(ghostRoleName);
            return () =>
            {
                base.gameObject.SetActive(false); 
                if (RoleAssignFilterModelUpdater.AddRoleData(model, filterId, id, ghostRoleId))
                {
                    createFilterItem(model, ghostRoleName, filterId, id, targetFilterTransform);
                }
            };
        }
        else
        {
            return () => { };
        }
    }

    [HideFromIl2Cpp]
    private void createFilterItem(
        RoleAssignFilterModel model, string name,
        Guid targetFilter, int id, Transform parent)
    {
        FilterItemProperty item = Instantiate(
            this.FilterItemPrefab, parent);
        item.gameObject.SetActive(true);
        item.Text.text = name;
        item.RemoveButton.onClick.AddListener(
            (UnityAction)(() =>
            {
                RoleAssignFilterModelUpdater.RemoveFilterRole(model, targetFilter, id);
                Destroy(item.gameObject);
            }));
    }
}
