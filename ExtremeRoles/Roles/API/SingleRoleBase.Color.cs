using ExtremeRoles.Roles.API.Interface.Status;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Roles.API;

public abstract partial class SingleRoleBase
{
    public virtual Color GetNameColor(bool isTruthColor = false) => this.Core.Color;

    public virtual Color GetTargetRoleSeeColor(
        SingleRoleBase targetRole,
        byte targetPlayerId)
    {
        if (targetRole.Core.Id == ExtremeRoleId.Xion)
        {
            return Module.ColorPalette.XionBlue;
        }

        if (targetRole is Solo.Impostor.OverLoader overLoader &&
			overLoader.IsOverLoad)
        {
			return Palette.ImpostorRed;
		}

        if ((
				targetRole.IsImpostor() || 
				(targetRole.Status is IFakeImpostorStatus fake && fake.IsFakeImpostor)
			) &&
            this.IsImpostor())
        {
            return Palette.ImpostorRed;
        }

        if (targetRole is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole != null)
        {
			return this.GetTargetRoleSeeColor(
				 multiAssignRole.AnotherRole, targetPlayerId);
		}

        return Palette.White;
    }
}
