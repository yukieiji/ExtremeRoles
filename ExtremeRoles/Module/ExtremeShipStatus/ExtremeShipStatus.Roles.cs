using System.Collections.Generic;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.ExtremeShipStatus;

public sealed partial class ExtremeShipStatus
{
	public bool IsAssassinAssign => isAssignAssassin;
	public bool AssassinMeetingTrigger => this.assassinMeetingTrigger;

	private bool isAssignAssassin = false;
	private bool assassinMeetingTrigger = false;

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

	public void AssassinMeetingTriggerOff()
	{
		this.assassinMeetingTrigger = false;
	}

	private void resetGlobalAction()
	{
		this.isAssignAssassin = false;
		this.assassinMeetingTrigger = false;
		this.deadedAssassin.Clear();
	}
}
