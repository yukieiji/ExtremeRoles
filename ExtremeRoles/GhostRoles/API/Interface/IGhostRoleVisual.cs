using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.GhostRoles.API.Interface;

public interface IGhostRoleVisual
{
	public string ColoredRoleName { get; }

	public string ImportantText { get; }
}
