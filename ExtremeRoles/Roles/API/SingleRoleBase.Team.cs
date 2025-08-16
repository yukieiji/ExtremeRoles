﻿namespace ExtremeRoles.Roles.API
{
    public abstract partial class SingleRoleBase
    {

        public int GameControlId { get; private set; } = 0;

        public bool IsVanillaRole() => this.Id == ExtremeRoleId.VanillaRole;

        public bool IsCrewmate() => this.Team == ExtremeRoleType.Crewmate;

        public bool IsImpostor() => this.Team == ExtremeRoleType.Impostor;

        public bool IsNeutral() => this.Team == ExtremeRoleType.Neutral;

        public bool IsLiberal() => this.Team == ExtremeRoleType.Liberal;

        public virtual bool IsSameTeam(SingleRoleBase targetRole)
        {
            if (this.IsLiberal())
            {
                return targetRole.Team == ExtremeRoleType.Liberal;
            }

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
