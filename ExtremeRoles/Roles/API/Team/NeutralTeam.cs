using ExtremeRoles.GameMode;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.API.Team;

public sealed class NeutralTeam : ITeam
{
    public int GameControlId { get; private set; }
    public ExtremeRoleType Type => ExtremeRoleType.Neutral;

    public bool Is(ExtremeRoleType teamType)
    {
        return Type == teamType;
    }

    public bool IsSameTeam(SingleRoleBase self, SingleRoleBase targetRole)
    {
        var targetCore = targetRole.Core;

		if (self.Core.Id == targetCore.Id)
        {
            if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
            {
                return true;
            }
            else
            {
                return self.Team.GameControlId == targetRole.Team.GameControlId;
            }
        }
        else
        {
            if (self.Team.Is(ExtremeRoleType.Impostor))
            {
                return targetCore.Team == ExtremeRoleType.Impostor;
            }

            if (targetRole is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole != null)
            {
				return self.IsSameTeam(multiAssignRole.AnotherRole);
			}

            return false;
        }
    }

    public void SetControlId(int id)
    {
        this.GameControlId = id;
    }
}
