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
using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.View;

[Il2CppRegister]
public sealed class RoleAssignFilterView : MonoBehaviour
{
    [HideFromIl2Cpp]
    public RoleAssignFilterModel Model
    {
        private get => this.model;
        set
        {
            this.initialize(value);
            this.model = value;
        }
    }
    [HideFromIl2Cpp]
    public GameObject HideObject { private get; set; }

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
        this.addFilterButton.ResetButtonAction();
        this.addFilterButton.SetButtonClickAction((UnityAction)addNewFilterSet);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            base.gameObject.SetActive(false);
        }
    }

    public void Start()
    {
        if (this.Model == null) { return; }

        this.Model.Id.Clear();
        this.Model.NormalRole.Clear();
        this.Model.CombRole.Clear();
        this.Model.GhostRole.Clear();

        var roleSelector = ExtremeGameModeManager.Instance.RoleSelector;
        int id = 0;
        foreach (var roleId in roleSelector.UseNormalRoleId)
        {
            this.Model.Id.Add(id);
            this.Model.NormalRole.Add(id, roleId);
            id++;
        }
        foreach (var roleId in roleSelector.UseCombRoleType)
        {
            this.Model.Id.Add(id);
            this.Model.CombRole.Add(id, roleId);
            id++;
        }
        foreach (var roleId in roleSelector.UseGhostRoleId)
        {
            this.Model.Id.Add(id);
            this.Model.GhostRole.Add(id, roleId);
            id++;
        }
    }

    public void OnEnable()
    {
        FastDestroyableSingleton<HudManager>.Instance.gameObject.SetActive(false);
        if (this.HideObject != null)
        {
            this.HideObject.SetActive(false);
        }
        this.addRoleMenu.gameObject.SetActive(false);
    }

    public void OnDisable()
    {
        if (this.HideObject != null)
        {
            this.HideObject.SetActive(true);
        }
        FastDestroyableSingleton<HudManager>.Instance.gameObject.SetActive(true);
    }

    [HideFromIl2Cpp]
    private void addNewFilterSet()
    {
        if (this.Model == null) { return; }

        Guid id = Guid.NewGuid();

        // Update model
        RoleAssignFilterModelUpdater.AddFilter(this.Model, id);
        this.createFilterSet(id);
    }

    [HideFromIl2Cpp]
    private RoleFilterSetProperty createFilterSet(Guid id)
    {
        var filterSet = Instantiate(this.filterSetPrefab, this.layout.transform);
        filterSet.Awake();
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
                this.addRoleMenu.gameObject.SetActive(true);
                this.addRoleMenu.UpdateView(
                    this.Model, id, filterSet.Layout.transform);
            }));
        filterSet.IncreseButton.onClick.AddListener(
            (UnityAction)(() =>
            {
                RoleAssignFilterModelUpdater.IncreseFilterAssignNum(this.Model, id);
                filterSet.AssignNumText.text = $"{this.Model.FilterSet[id].AssignNum}";
            }));
        filterSet.DecreseButton.onClick.AddListener(
            (UnityAction)(() =>
            {
                RoleAssignFilterModelUpdater.DecreseFilterAssignNum(this.Model, id);
                filterSet.AssignNumText.text = $"{this.Model.FilterSet[id].AssignNum}";
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

        foreach (var (filterId, filter) in model.FilterSet)
        {
            var filterProp = this.createFilterSet(filterId);
            var parent = filterProp.Layout.transform;
            filterProp.AssignNumText.text = $"{filter.AssignNum}";

            foreach (var (id, roleId) in filter.FilterNormalId)
            {
                string roleName = ExtremeRoleManager.NormalRole[
                    (int)roleId].GetColoredRoleName(true);
                createFilterItem(parent, roleName, filterId, id);
            }
            foreach (var (id, roleId) in filter.FilterCombinationId)
            {
                string combRoleName = ExtremeRoleManager.CombRole[
                    (byte)roleId].GetOptionName();
                createFilterItem(parent, combRoleName, filterId, id);
            }
            foreach (var (id, roleId) in filter.FilterGhostRole)
            {
                string ghostRoleName = ExtremeGhostRoleManager.AllGhostRole[
                    roleId].GetColoredRoleName();
                createFilterItem(parent, ghostRoleName, filterId, id);
            }
        }
    }

    [HideFromIl2Cpp]
    private void createFilterItem(
        Transform parent, string name, Guid filterId, int id)
    {
        FilterItemProperty item = Instantiate(
            this.addRoleMenu.FilterItemPrefab, parent);
        item.Awake();
        item.gameObject.SetActive(true);
        item.Text.text = name;
        item.RemoveButton.onClick.AddListener(
            (UnityAction)(() =>
            {
                RoleAssignFilterModelUpdater.RemoveFilterRole(this.Model, filterId, id);
                Destroy(item.gameObject);
            }));
    }
}
