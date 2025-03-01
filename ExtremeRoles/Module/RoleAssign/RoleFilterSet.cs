using System.Collections.Generic;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class RoleFilterSet(int assignNum)
{
	private int curAssignNum = 0;
	private bool isBlock = false;

	private readonly int assignNum = assignNum;
	private readonly HashSet<int> normalRoleFilter = new HashSet<int>(assignNum);
	private readonly HashSet<byte> combRoleFilter = new HashSet<byte>(assignNum);
	private readonly HashSet<ExtremeGhostRoleId> ghostRoleFilter = new HashSet<ExtremeGhostRoleId>(assignNum);

	public void Add(ExtremeRoleId roleId)
	{
		this.normalRoleFilter.Add((int)roleId);
	}

	public void Add(CombinationRoleType roleId)
	{
		this.combRoleFilter.Add((byte)roleId);
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
		if (this.isBlock || !this.combRoleFilter.Contains(bytedRoleId))
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
		this.isBlock = this.curAssignNum >= this.assignNum;
	}
}
