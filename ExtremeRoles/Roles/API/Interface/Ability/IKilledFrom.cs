using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Roles.API.Interface.Ability;

public interface IKilledFrom
{
	public bool TryKilledFrom(
		PlayerControl rolePlayer,
		PlayerControl fromPlayer);
}
