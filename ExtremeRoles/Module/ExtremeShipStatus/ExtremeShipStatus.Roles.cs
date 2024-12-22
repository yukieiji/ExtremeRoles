using System.Collections.Generic;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.ExtremeShipStatus;

public sealed partial class ExtremeShipStatus
{
	public bool IsAssassinAssign => isAssignAssassin;

	private bool isAssignAssassin = false;

	private Queue<byte> deadedAssassin = new Queue<byte>();

	public void AddGlobalActionRole(SingleRoleBase role)
	{
		switch (role.Id)
		{
			case ExtremeRoleId.Assassin:
				this.isAssignAssassin = true;
				break;
			default:
				break;
		}
	}

	private void resetGlobalAction()
	{
		this.isAssignAssassin = false;
		this.deadedAssassin.Clear();
	}
}
