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
    private FilterItemProperty filterItemPrefab;

    private VerticalLayoutGroup layout;

    private AddRoleMenuView addRoleMenu;

    public RoleAssignFilterView(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618
    public void Awake()
    {
        Transform trans = transform;

        addFilterButton = trans.Find(
            "Body/AddFilterButton").gameObject.GetComponent<ButtonWrapper>();
        layout = trans.Find(
            "Body/Scroll/Viewport/Content").gameObject.GetComponent<VerticalLayoutGroup>();
        filterSetPrefab = trans.Find(
            "Body/FillterSet").gameObject.GetComponent<RoleFilterSetProperty>();
        filterItemPrefab = trans.Find(
            "Body/FilterItem").gameObject.GetComponent<FilterItemProperty>();
        addRoleMenu = trans.Find(
            "Body/AddRoleMenu").gameObject.GetComponent<AddRoleMenuView>();
        var closeButton = trans.Find("CloseButton").gameObject.GetComponent<
            Button>();

        if (Model == null)
        {
            Model = new RoleAssignFilterModel()
            {
                FilterId = 0,
                FilterSet = new()
            };
        }

        // Create Actions
        addFilterButton.Awake();
        addFilterButton.SetButtonClickAction((UnityAction)AddNewFilterSet);
    }

    public void OnEnable()
    {
        var roleSelector = ExtremeGameModeManager.Instance.RoleSelector;
        foreach (var child in addRoleMenu.Layout.rectChildren)
        {
            Destroy(child.gameObject);
        }

        foreach (var roleId in roleSelector.UseNormalRoleId)
        {

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
    }
}
