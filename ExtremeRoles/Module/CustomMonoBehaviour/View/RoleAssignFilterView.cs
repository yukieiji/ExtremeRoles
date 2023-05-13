using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Module.RoleAssign.Model;
using ExtremeRoles.Module.RoleAssign.Update;
using ExtremeRoles.GameMode;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.View;

[Il2CppRegister]
public sealed class RoleAssignFilterView : MonoBehaviour
{
    public static RoleAssignFilterModel? Model { get; private set; }

#pragma warning disable CS8618
    private ButtonWrapper addFilterButton;
    private RoleFilterSetProperty filterSetPrefab;

    private VerticalLayoutGroup layout;

    private AddRoleMenuView addRoleMenu;

    public RoleAssignFilterView(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618
    public void Awake()
    {
        Transform trans = transform;

        this.addFilterButton = trans.Find(
            "Body/AddFilterButton").gameObject.GetComponent<ButtonWrapper>();
        this.layout = trans.Find(
            "Body/Scroll/Viewport/Content").gameObject.GetComponent<VerticalLayoutGroup>();
        this.filterSetPrefab = trans.Find(
            "Body/FillterSet").gameObject.GetComponent<RoleFilterSetProperty>();
        
        this.addRoleMenu = trans.Find(
            "Body/AddRoleMenu").gameObject.GetComponent<AddRoleMenuView>();
        var closeButton = trans.Find("CloseButton").gameObject.GetComponent<
            Button>();

        if (Model == null)
        {
            Model = new RoleAssignFilterModel()
            {
                FilterId = 0,
                AddRoleMenu = new(),
                FilterSet = new()
            };
        }

        // Create Actions
        this.addFilterButton.Awake();
        this.addFilterButton.SetButtonClickAction((UnityAction)AddNewFilterSet);
    }

    public void OnEnable()
    {
        if (Model == null) { return; }
        var menu = Model.AddRoleMenu;
        
        menu.NormalRole.Clear();
        menu.CombRole.Clear();
        menu.GhostRole.Clear();

        var roleSelector = ExtremeGameModeManager.Instance.RoleSelector;
        int id = 0;
        foreach (var roleId in roleSelector.UseNormalRoleId)
        {
            menu.NormalRole.Add(id, roleId);
            id++;
        }
        foreach (var roleId in roleSelector.UseCombRoleType)
        {
            menu.CombRole.Add(id, roleId);
            id++;
        }
        foreach (var roleId in roleSelector.UseGhostRoleId)
        {
            menu.GhostRole.Add(id, roleId);
            id++;
        }
    }

    private void AddNewFilterSet()
    {
        if (Model == null) { return; }

        int id = Model.FilterId;

        // Update model
        RoleAssignFilterModelUpdater.AddFilter(Model);

        var filterSet = Instantiate(filterSetPrefab, layout.transform);
        filterSet.gameObject.SetActive(true);

        filterSet.DeleteThisButton.SetButtonClickAction(
            (UnityAction)(() =>
            {
                RoleAssignFilterModelUpdater.RemoveFilter(Model, id);
                Destroy(filterSet.gameObject);
            }));
        filterSet.DeleteAllRoleButton.SetButtonClickAction(
            (UnityAction)(() =>
            {
                RoleAssignFilterModelUpdater.ResetFilter(Model, id);
                foreach (var child in filterSet.Layout.rectChildren)
                {
                    Destroy(child.gameObject);
                }
            }));
        filterSet.AddRoleButton.SetButtonClickAction(
            (UnityAction)(() =>
            {
                this.addRoleMenu.gameObject.SetActive(true);
                this.addRoleMenu.UpdateView(Model.AddRoleMenu);
            }));
    }
}
