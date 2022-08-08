namespace ExtremeRoles.Roles.API
{
    public abstract partial class SingleRoleBase
    {

        public int GameControlId = 0;

        public bool IsVanillaRole() => this.Id == ExtremeRoleId.VanillaRole;

        public bool IsCrewmate() => this.Team == ExtremeRoleType.Crewmate;

        public bool IsImpostor() => this.Team == ExtremeRoleType.Impostor;

        public bool IsNeutral() => this.Team == ExtremeRoleType.Neutral;

        public virtual bool IsSameTeam(SingleRoleBase targetRole)
        {

            if (this.IsImpostor())
            {
                return targetRole.Team == ExtremeRoleType.Impostor;
            }

            MultiAssignRoleBase multiAssignRole = targetRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.IsSameTeam(
                        multiAssignRole.AnotherRole);
                }
            }

            return false;
        }
        protected bool IsSameControlId(SingleRoleBase tarrgetRole)
        {
            return this.GameControlId == tarrgetRole.GameControlId;
        }

    }
}
