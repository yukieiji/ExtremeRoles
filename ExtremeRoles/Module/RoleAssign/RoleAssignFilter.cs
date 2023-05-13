using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.RoleAssign.Update;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class RoleAssignFilter : NullableSingleton<RoleAssignFilter>
{
    private List<RoleFilterSet> filter = new List<RoleFilterSet>();
    public RoleAssignFilter()
    {
        this.filter.Clear();

        var model = CustomMonoBehaviour.View.RoleAssignFilterView.Model;
        if (model == null) { return; }
        RoleAssignFilterModelUpdater.ConvertModelToAssignFilter(model, this);
    }
    
    public void AddRoleFilterSet(RoleFilterSet filter)
    {
        this.filter.Add(filter);
    }

    public void Update(int intedRoleId)
    {
        foreach (var fil in this.filter)
        {
            fil.Update(intedRoleId);
        }
    }
    public void Update(byte bytedCombRoleId)
    {
        foreach (var fil in this.filter)
        {
            fil.Update(bytedCombRoleId);
        }
    }
    public void Update(ExtremeGhostRoleId roleId)
    {
        foreach (var fil in this.filter)
        {
            fil.Update(roleId);
        }
    }
    public bool IsBlock(int intedRoleId) => this.filter.Any(x => x.IsBlock(intedRoleId));
    public bool IsBlock(byte bytedCombRoleId) => this.filter.Any(x => x.IsBlock(bytedCombRoleId));
    public bool IsBlock(ExtremeGhostRoleId roleId) => this.filter.Any(x => x.IsBlock(roleId));
}
