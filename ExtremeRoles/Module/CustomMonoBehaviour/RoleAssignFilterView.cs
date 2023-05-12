using System;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.Model;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

public sealed class RoleAssignFilterView : MonoBehaviour
{
    public static RoleAssignFilterModel? Model { get; private set; }

    public RoleAssignFilterView(IntPtr ptr) : base(ptr) { }

    public void Awake()
    {
        if (!Model.HasValue)
        {
            Model = new RoleAssignFilterModel()
            {
                CurCount = 0,
                FilterSet = new()
            };
        }

        var model = Model.Value;
        var addFilterFunction = () =>
        {
            RoleAssignFilterModelUpdater.AddFilter(model);
            createFilter(model.CurCount);
        };
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
