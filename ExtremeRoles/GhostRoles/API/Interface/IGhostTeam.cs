using ExtremeRoles.Roles.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.GhostRoles.API.Interface;

public interface IGhostTeam
{
	public ExtremeRoleType Id { get; }
	public bool IsCrewmate();

	public bool IsImpostor();

	public bool IsNeutral();
}
