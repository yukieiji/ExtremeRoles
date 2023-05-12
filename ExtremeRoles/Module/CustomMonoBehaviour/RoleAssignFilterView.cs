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
            "Body/FillterSet").gameObject.GetComponent<RoleFilterSetBehaviour>();

        if (!Model.HasValue)
        {
            Model = new RoleAssignFilterModel()
            {
                FilterId = 0,
                FilterSet = new()
            };
        }

        var model = Model.Value;

        // Create Actions
        this.addFilterButton.Awake();
        this.addFilterButton.SetButtonClickAction((UnityAction)AddNewFilterSet);
    }

    private void AddNewFilterSet()
    {
        if (!Model.HasValue) { return; }

        var model = Model.Value;

        int id = model.FilterId;

        // Update model
        RoleAssignFilterModelUpdater.AddFilter(model);

        var filterSet = Instantiate(this.filterSetPrefab, this.layout.transform);
        filterSet.gameObject.SetActive(true);

        filterSet.DeleteThisButton.SetButtonClickAction(
            (UnityAction)(() =>
            {
                RoleAssignFilterModelUpdater.RemoveFilter(model, id);
                Destroy(filterSet.gameObject);
            }));
        filterSet.DeleteAllRoleButton.SetButtonClickAction(
            (UnityAction)(() =>
            {
                RoleAssignFilterModelUpdater.ResetFilter(model, id);
                foreach (var child in filterSet.Layout.rectChildren)
                {
                    Destroy(child.gameObject);
                }
            }));
    }
}
