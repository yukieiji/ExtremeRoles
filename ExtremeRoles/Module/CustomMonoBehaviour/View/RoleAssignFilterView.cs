using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Module.RoleAssign.Model;
using ExtremeRoles.Module.RoleAssign.Update;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.View;

[Il2CppRegister]
public sealed class RoleAssignFilterView : MonoBehaviour
{
    public RoleAssignFilterModel Model
    {
        private get => this.model;
        set
        {
            this.initialize(value);
            this.model = value;
        }
    }

#pragma warning disable CS8618
    private ButtonWrapper addFilterButton;
    private RoleFilterSetProperty filterSetPrefab;

    private VerticalLayoutGroup layout;

    private AddRoleMenuView addRoleMenu;

    private RoleAssignFilterModel model;

    public RoleAssignFilterView(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618
    public void Awake()
    {
        Transform trans = base.transform;

        this.addFilterButton = trans.Find(
            "Body/AddFilterButton").gameObject.GetComponent<ButtonWrapper>();
        this.layout = trans.Find(
            "Body/Scroll/Viewport/Content").gameObject.GetComponent<VerticalLayoutGroup>();
        this.filterSetPrefab = trans.Find(
            "Body/FillterSet").gameObject.GetComponent<RoleFilterSetProperty>();
        
        this.addRoleMenu = trans.Find(
            "Body/AddRoleMenu").gameObject.GetComponent<AddRoleMenuView>();
        this.addRoleMenu.Awake();

        var closeButton = trans.Find(
            "Body/CloseButton").gameObject.GetComponent<Button>();
        closeButton.onClick.AddListener(
            (UnityAction)(() => base.gameObject.SetActive(false)));

        // Create Actions
        this.addFilterButton.Awake();
        this.addFilterButton.SetButtonClickAction((UnityAction)addNewFilterSet);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            base.gameObject.SetActive(false);
        }
    }

    public void OnEnable()
    {
        if (this.Model == null) { return; }
        var menu = this.Model.AddRoleMenu;
        
        menu.Id.Clear();
        menu.NormalRole.Clear();
        menu.CombRole.Clear();
        menu.GhostRole.Clear();

        var roleSelector = ExtremeGameModeManager.Instance.RoleSelector;
        int id = 0;
        foreach (var roleId in roleSelector.UseNormalRoleId)
        {
            menu.Id.Add(id);
            menu.NormalRole.Add(id, roleId);
            id++;
        }
        foreach (var roleId in roleSelector.UseCombRoleType)
        {
            menu.Id.Add(id);
            menu.CombRole.Add(id, roleId);
            id++;
        }
        foreach (var roleId in roleSelector.UseGhostRoleId)
        {
            menu.Id.Add(id);
            menu.GhostRole.Add(id, roleId);
            id++;
        }
        this.addRoleMenu.gameObject.SetActive(false);
    }

    [HideFromIl2Cpp]
    private void addNewFilterSet()
    {
        if (this.Model == null) { return; }

        int id = this.Model.FilterId;

        // Update model
        RoleAssignFilterModelUpdater.AddFilter(this.Model);
        this.createFilterSet(id);
    }

    private RoleFilterSetProperty createFilterSet(int id)
    {
        var filterSet = Instantiate(this.filterSetPrefab, this.layout.transform);
        filterSet.gameObject.SetActive(true);

        filterSet.DeleteThisButton.SetButtonClickAction(
            (UnityAction)(() =>
            {
                RoleAssignFilterModelUpdater.RemoveFilter(this.Model, id);
                Destroy(filterSet.gameObject);
            }));
        filterSet.DeleteAllRoleButton.SetButtonClickAction(
            (UnityAction)(() =>
            {
                RoleAssignFilterModelUpdater.ResetFilter(this.Model, id);
                foreach (var child in filterSet.Layout.rectChildren)
                {
                    Destroy(child.gameObject);
                }
            }));
        filterSet.AddRoleButton.SetButtonClickAction(
            (UnityAction)(() =>
            {
                var menuModel = this.Model.AddRoleMenu;
                menuModel.Property = filterSet;
                menuModel.Filter = this.Model.FilterSet[id];

                this.addRoleMenu.gameObject.SetActive(true);
                this.addRoleMenu.UpdateView(this.Model.AddRoleMenu);
            }));
        return filterSet;
    }

    [HideFromIl2Cpp]
    private void initialize(RoleAssignFilterModel model)
    {
        foreach (var child in this.layout.rectChildren)
        {
            Destroy(child.gameObject);
        }


        foreach (var (id, filter) in model.FilterSet)
        {
            var filterProp = this.createFilterSet(id);
            var parent = filterProp.Layout.transform;

            foreach (var (filterId, roleId) in filter.FilterNormalId)
            {
                string roleName = ExtremeRoleManager.NormalRole[
                    (int)roleId].GetColoredRoleName(true);
                createFilterItem(parent, roleName, filterId);
            }
            foreach (var (filterId, roleId) in filter.FilterCombinationId)
            {
                string combRoleName = ExtremeRoleManager.CombRole[
                    (byte)roleId].GetOptionName();
                createFilterItem(parent, combRoleName, id);
            }
            foreach (var (filterId, roleId) in filter.FilterGhostRole)
            {
                string ghostRoleName = ExtremeGhostRoleManager.AllGhostRole[
                    roleId].GetColoredRoleName();
                createFilterItem(parent, ghostRoleName, filterId);
            }
        }
    }

    [HideFromIl2Cpp]
    private void createFilterItem(Transform parent, string name, int id)
    {
        FilterItemProperty item = Instantiate(
            this.addRoleMenu.FilterItemPrefab, parent);
        item.gameObject.SetActive(true);
        item.Text.text = name;
        item.RemoveButton.onClick.AddListener(
            (UnityAction)(() =>
            {
                AddRoleMenuModelUpdater.RemoveFilterRole(this.Model.AddRoleMenu, id);
                Destroy(item.gameObject);
            }));
    }
}
