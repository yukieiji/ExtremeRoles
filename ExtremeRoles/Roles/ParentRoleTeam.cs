using ExtremeRoles.GameMode;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Team;

namespace ExtremeRoles.Roles;

public class ParentNeutralRoleTeam(RoleCore core, ExtremeRoleId childId) : ITeam
{
	private readonly DefaultTeam _default = new DefaultTeam(core);
	private readonly ExtremeRoleId _childId = childId;

	public int GameControlId => _default.GameControlId;

	public bool? IsSame(SingleRoleBase targetRole)
	{
		var id = targetRole.Core.Id;
		if (id == this._default.Core.Id || id == _childId)
		{
			if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
			{
				return true;
			}
			else
			{
				return _default.GameControlId == targetRole.GameControlId;
			}
		}
		else
		{
			return _default.IsSame(targetRole);
		}
	}

	public void SetControlId(int id)
		=> this._default.SetControlId(id);
}
