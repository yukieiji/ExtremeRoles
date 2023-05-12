using System.Collections.Generic;

using ExtremeRoles.GhostRoles;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class RoleFilterSet
{
    private bool isBlock = false;

    private HashSet<int>  normalRoleFilter = new HashSet<int>();
    private HashSet<byte> combRoleFilter = new HashSet<byte>();
    private HashSet<ExtremeGhostRoleId> ghostRoleFilter = new HashSet<ExtremeGhostRoleId>();

    public RoleFilterSet()
    {
        this.normalRoleFilter.Clear();
        this.combRoleFilter.Clear();
        this.ghostRoleFilter.Clear();

        this.isBlock = false;
    }

    public void Update(int intedRoleId)
    {
        if (this.isBlock) { return; }
        this.isBlock = this.normalRoleFilter.Contains(intedRoleId);
    }
    public void Update(byte bytedRoleId)
    {
        if (this.isBlock) { return; }
        this.isBlock = this.combRoleFilter.Contains(bytedRoleId);
    }
    public void Update(ExtremeGhostRoleId roleId)
    {
        if (this.isBlock) { return; }
        this.isBlock = this.ghostRoleFilter.Contains(roleId);
    }

    public bool IsBlock(int intedRoleId)
        => this.normalRoleFilter.Contains(intedRoleId) ? this.isBlock : false;

    public bool IsBlock(byte bytedRoleId)
        => this.combRoleFilter.Contains(bytedRoleId) ? this.isBlock : false;

    public bool IsBlock(ExtremeGhostRoleId roleId)
        => this.ghostRoleFilter.Contains(roleId) ? this.isBlock : false;
}
