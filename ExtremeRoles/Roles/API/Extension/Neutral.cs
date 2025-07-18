using ExtremeRoles.GameMode;

namespace ExtremeRoles.Roles.API.Extension.Neutral;

public static class NeutralRoleExtension
{
    public static bool IsNeutralSameTeam(
        this SingleRoleBase self,
        SingleRoleBase targetRole)
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
                return self.GameControlId == targetRole.GameControlId;
            }
        }
        else
        {
            if (self.IsImpostor())
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
}
