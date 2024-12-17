using ExtremeRoles.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

public interface IOnemanMeeting
{
	public byte ExiledTarget { set; }

	public bool TryGetGameEndReason(out RoleGameOverReason reason);
	public bool TryStartMeeting(byte target);
}
