using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.Model;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class RoleAssignFilterView : MonoBehaviour
{
    public static RoleAssignFilterModel? Model { get; private set; }

#pragma warning disable CS8618
    private ButtonWrapper addFilterButton;
    private RoleFilterSetBehaviour filterSetPrefab;

    private VerticalLayoutGroup layout;

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
            "Body/Scroll/Viewport/Content/FillterSet").gameObject.GetComponent<RoleFilterSetBehaviour>();

        // Create Actions
        this.addFilterButton.Awake();
        this.addFilterButton.SetButtonClickAction(
            (UnityAction)(() =>
            {
                var filter = Instantiate(this.filterSetPrefab, this.layout.transform);
                filter.gameObject.SetActive(true);
            }));



        if (!Model.HasValue)
        {
            Model = new RoleAssignFilterModel()
            {
                CurCount = 0,
                FilterSet = new()
            };
        }

        /*
        var model = Model.Value;
        var addFilterFunction = () =>
        {
            RoleAssignFilterModelUpdater.AddFilter(model);
            createFilter(model.CurCount);
        };
        */
    }

    private void createFilter(int id)
    {
        if (!Model.HasValue) { return; }

        var model = Model.Value;

        var addRoleFunction = () =>
        {
            RoleAssignFilterModelUpdater.AddFilter(model);
            updateFilter(model.FilterSet[id]);
        };
        var deleteRoleFunction = () =>
        {
            RoleAssignFilterModelUpdater.AddFilter(model);
            updateFilter(model.FilterSet[id]);
        };
        var removeAllRoleFunction = () =>
        {
            RoleAssignFilterModelUpdater.ResetFilter(model, id);
            updateFilter(model.FilterSet[id]);
        };
        var removeThisFunction = () =>
        {
            RoleAssignFilterModelUpdater.RemoveFilter(model, id);
            // updateAllView
        };
    }
    private void updateFilter(RoleFilterSetModel model)
    {

    }
}
