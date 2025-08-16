namespace ExtremeRoles.Roles.API
{
    public abstract partial class SingleRoleBase
    {

        public int GameControlId { get; private set; } = 0;

        public bool IsVanillaRole() => this.Core.Id == ExtremeRoleId.VanillaRole;

        public bool IsCrewmate() => this.Core.Team == ExtremeRoleType.Crewmate;

        public bool IsImpostor() => this.Core.Team == ExtremeRoleType.Impostor;

        public bool IsNeutral() => this.Core.Team == ExtremeRoleType.Neutral;

        public bool IsLiberal() => this.Team == ExtremeRoleType.Liberal;

        public bool IsLiberal() => this.Team == ExtremeRoleType.Liberal;

        public virtual bool IsSameTeam(SingleRoleBase targetRole)
        {
            if (this.IsLiberal())
            {
                return targetRole.Team == ExtremeRoleType.Liberal;
            }

            if (this.IsImpostor())
            {
                return targetRole.Core.Team == ExtremeRoleType.Impostor;
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

        public void SetControlId(int id)
        {
            this.GameControlId = id;
        }

        protected bool IsSameControlId(SingleRoleBase tarrgetRole)
        {
            return this.GameControlId == tarrgetRole.GameControlId;
        }

    }
}
