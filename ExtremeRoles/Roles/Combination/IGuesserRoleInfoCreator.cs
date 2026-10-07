using System.Collections.Generic;
using ExtremeRoles.Module.CustomMonoBehaviour;

namespace ExtremeRoles.Roles.Combination;

public interface IGuesserRoleInfoCreator
{
	IReadOnlyList<GuessBehaviour.RoleInfo> Create(bool includeNoneRole, Guesser.DefaultGuessRole defaultRole);
}
