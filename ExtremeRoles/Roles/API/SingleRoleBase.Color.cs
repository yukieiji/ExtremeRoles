using UnityEngine;


namespace ExtremeRoles.Roles.API
{
    public abstract partial class SingleRoleBase
    {
        public void SetNameColor(Color newColor)
        {
            this.NameColor = newColor;
        }

        public virtual Color GetNameColor(bool isTruthColor = false) => this.NameColor;

        public virtual Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole,
            byte targetPlayerId)
        {
            if (targetRole.Id == ExtremeRoleId.Xion)
            { 
                return Palette.DisabledGrey; 
            }

            Solo.Impostor.OverLoader overLoader = targetRole as Solo.Impostor.OverLoader;

            if (overLoader != null)
            {
                if (overLoader.IsOverLoad)
                {
                    return Palette.ImpostorRed;
                }
            }

            if ((targetRole.IsImpostor() || targetRole.FakeImposter) &&
                this.IsImpostor())
            {
                return Palette.ImpostorRed;
            }
            MultiAssignRoleBase multiAssignRole = targetRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.GetTargetRoleSeeColor(
                        multiAssignRole.AnotherRole, targetPlayerId);
                }
            }

            return Palette.White;
        }
    }
}
