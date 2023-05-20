using System.Collections.Generic;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class RoleFilterSet
{
    public int AssignNum { private get; set; } = 1;

    private int curAssignNum = 0;
    private bool isBlock = false;

    private HashSet<int>  normalRoleFilter = new HashSet<int>();
    private HashSet<byte> combRoleFilter = new HashSet<byte>();
    private HashSet<ExtremeGhostRoleId> ghostRoleFilter = new HashSet<ExtremeGhostRoleId>();

    public RoleFilterSet()
    {
        this.normalRoleFilter.Clear();
        this.combRoleFilter.Clear();
        this.ghostRoleFilter.Clear();

        this.curAssignNum = 0;
        
        this.isBlock = false;
    }

    public void Add(ExtremeRoleId roleId)
    {
        this.normalRoleFilter.Add((int)roleId);
    }

    public void Add(CombinationRoleType roleId)
    {
        this.combRoleFilter.Contains((byte)roleId);
    }
    
    public void Add(ExtremeGhostRoleId roleId)
    {
        this.ghostRoleFilter.Add(roleId);
    }

    public bool IsBlock(int intedRoleId)
        => this.normalRoleFilter.Contains(intedRoleId) ? this.isBlock : false;

    public bool IsBlock(byte bytedRoleId)
        => this.combRoleFilter.Contains(bytedRoleId) ? this.isBlock : false;

    public bool IsBlock(ExtremeGhostRoleId roleId)
        => this.ghostRoleFilter.Contains(roleId) ? this.isBlock : false;

    public void Update(int intedRoleId)
    {
        if (this.isBlock || !this.normalRoleFilter.Contains(intedRoleId))
        { 
            return; 
        }
        this.updateState();
    }

    public void Update(byte bytedRoleId)
    {
        if (this.isBlock || !this.normalRoleFilter.Contains(bytedRoleId))
        {
            return;
        }
        this.updateState();
    }

    public void Update(ExtremeGhostRoleId roleId)
    {
        if (this.isBlock || this.ghostRoleFilter.Contains(roleId))
        { 
            return; 
        }
        this.updateState();
    }

    private void updateState()
    {
        ++this.curAssignNum;
        this.isBlock = this.curAssignNum >= this.AssignNum;
    }
}
