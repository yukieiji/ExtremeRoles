using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Roles.API.Interface.Status;

public interface ISubTeam
{
	public NeutralSeparateTeam Main { get; }
	public NeutralSeparateTeam Sub { get; }
	public bool IsSub { get; }
}
