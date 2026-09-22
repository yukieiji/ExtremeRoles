using ExtremeRoles.GameMode;
using ExtremeRoles.Roles.API.Interface.Team;

namespace ExtremeRoles.Roles.API;

public class DefaultNeutralTeam(RoleCore core) : ITeam
{
	private readonly DefaultTeam _default = new DefaultTeam(core);

	public int GameControlId => _default.GameControlId;
	public bool IsCrewmate => _default.IsCrewmate;
	public bool IsImpostor => _default.IsImpostor;
	public bool IsLiberal => _default.IsLiberal;
	public bool IsNeutral => _default.IsNeutral;
	public bool IsVanillaRole => _default.IsVanillaRole;

	public bool? IsSame(SingleRoleBase targetRole)
	{
		var targetCore = targetRole.Core;

		if (_default.Core.Id == targetCore.Id)
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

	public void SetControlId(int id) => _default.SetControlId(id);
}
