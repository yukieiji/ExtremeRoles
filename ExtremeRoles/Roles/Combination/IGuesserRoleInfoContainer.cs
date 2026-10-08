using System.Collections.Generic;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Combination;

public interface IGuesserRoleInfoContainer
{
	IReadOnlyList<GuessBehaviour.RoleInfo> Result { get; }
	IReadOnlyDictionary<ExtremeRoleType, List<ExtremeRoleId>> SeparatedRoleId { get; }

	void Add(ExtremeRoleId id, ExtremeRoleType team, ExtremeRoleId another = ExtremeRoleId.Null);
	void AddSeparatedRoleId(ExtremeRoleType team, ExtremeRoleId id);
	void ListAdd(ExtremeRoleId baseId, ExtremeRoleType team, List<ExtremeRoleId> list);
	void ListAddTargetTeam(ExtremeRoleId baseId, ExtremeRoleType team, ExtremeRoleType targetType);
}
