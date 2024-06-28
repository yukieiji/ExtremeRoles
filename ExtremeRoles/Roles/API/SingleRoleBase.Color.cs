using UnityEngine;

#nullable enable

namespace ExtremeRoles.Roles.API;

public abstract partial class SingleRoleBase
{
    public void SetNameColor(Color newColor)
    {
        this.NameColor = newColor;
    }

    public virtual Color GetNameColor(bool isTruthColor = false) => this.NameColor;

    public virtual Color GetTargetRoleSeeColor(
        SingleRoleBase? targetRole,
        byte targetPlayerId)
    {
		if (targetRole is null)
		{
			return Palette.White;
		}

        if (targetRole.Id == ExtremeRoleId.Xion)
        {
            return Module.ColorPalette.XionBlue;
        }

        if (targetRole is Solo.Impostor.OverLoader overLoader &&
			overLoader.IsOverLoad)
        {
			return Palette.ImpostorRed;
		}

        if ((targetRole.IsImpostor() || targetRole.FakeImposter) &&
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
