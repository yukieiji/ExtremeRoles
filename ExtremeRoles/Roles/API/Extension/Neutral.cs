namespace ExtremeRoles.Roles.API.Extension.Neutral
{
    public static class NeutralRoleExtension
    {
        public static bool IsNeutralSameTeam(
            this SingleRoleBase self,
            SingleRoleBase targetRole)
        {
            if (self.Id == targetRole.Id)
            {
                if (OptionHolder.Ship.IsSameNeutralSameWin)
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
                    return targetRole.Team == ExtremeRoleType.Impostor;
                }

                var multiAssignRole = targetRole as MultiAssignRoleBase;
                if (multiAssignRole != null)
                {
                    if (multiAssignRole.AnotherRole != null)
                    {
                        return self.IsSameTeam(
                            multiAssignRole.AnotherRole);
                    }
                }

                return false;
            }
        }
    }
}
