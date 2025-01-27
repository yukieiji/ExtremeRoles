using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Roles.API.Interface;

public interface IKilledFrom
{
	public bool TryPlayerKilledFrom(
		PlayerControl rolePlayer,
		PlayerControl fromPlayer);
}
