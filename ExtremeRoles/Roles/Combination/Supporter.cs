using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


#nullable enable

namespace ExtremeRoles.Roles.Combination;

public sealed class SupporterManager : FlexibleCombinationRoleManagerBase
{
    public SupporterManager() : base(
		CombinationRoleType.Supporter,
		new Supporter(), 1)
    { }

}

public sealed class Supporter : MultiAssignRoleBase, IRoleSpecialSetUp
{
    public override string RoleName =>
        string.Concat(this.roleNamePrefix, this.Core.Name);

    private byte supportTargetId;
    private string supportPlayerName = "";
    private string supportRoleName = "";
    private Color supportColor;

    private string roleNamePrefix = "";

    public Supporter(
        ) : base(
			RoleCore.BuildCrewmate(ExtremeRoleId.Supporter, ColorPalette.SupporterGreen),
            false, true, false, false,
            tab: OptionTab.CombinationTab)
    {}

    public void IntroBeginSetUp()
    {
        List<byte> target = new List<byte>();

        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            if (playerControl == null || playerControl.Data == null ||
                !ExtremeRoleManager.TryGetRole(playerControl.PlayerId, out var role) ||
				role.Core.Id == this.Core.Id)
            {
                continue;
            }

			var id = role.Core.Id;

            if (((id is ExtremeRoleId.Marlin) && this.IsCrewmate()) ||
                ((id is ExtremeRoleId.Assassin) && this.IsImpostor()))
            {
                target.Add(playerControl.PlayerId);
            }
        }

        if (target.Count == 0)
        {
            foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
            {
                if (playerControl == null ||
					playerControl.Data == null ||
                    !ExtremeRoleManager.TryGetRole(playerControl.PlayerId, out var role) ||
					role.Core.Id == this.Core.Id)
                {
                    continue;
                }

                if ((role.IsCrewmate() && this.IsCrewmate()) ||
                    (role.IsImpostor() && this.IsImpostor()))
                {
                    target.Add(playerControl.PlayerId);
                }
            }
        }

		if (target.Count == 0)
		{
			return;
		}

		this.supportTargetId = target.OrderBy(
			item => RandomGenerator.Instance.Next()).First();

        var targetPlayerControl = Player.GetPlayerControlById(this.supportTargetId);

        if (targetPlayerControl != null &&
			ExtremeRoleManager.TryGetRole(this.supportTargetId, out var supportRole))
        {
            this.supportRoleName = supportRole.GetColoredRoleName();
            this.supportColor = supportRole.GetNameColor(); // Use directly from supportRole
            this.supportPlayerName = targetPlayerControl.Data.PlayerName;
        }
        else
        {
            // Handle case where the target role is not found after selection or player control is null
            this.supportRoleName = "Unknown Role";
            this.supportPlayerName = (targetPlayerControl != null && targetPlayerControl.Data != null) ? targetPlayerControl.Data.PlayerName : "Unknown Player";
            this.supportColor = Color.white;
        }
    }

    public void IntroEndSetUp()
    {
        return;
    }

    public override string GetFullDescription()
    {

        string baseDesc;

        if (this.IsImpostor())
        {
            baseDesc = Tr.GetString(
                $"{this.Core.Id}ImposterFullDescription");
        }
        else
        {
            baseDesc = base.GetFullDescription();
        }

        return string.Format(
            baseDesc,
            this.supportPlayerName,
            this.supportRoleName);
    }
    public override string GetRolePlayerNameTag(
        SingleRoleBase targetRole, byte targetPlayerId)
    {
        if (targetPlayerId == this.supportTargetId)
        {
            return Design.ColoredString(
                ColorPalette.SupporterGreen,
                " ★");
        }

        return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
    }


    public override Color GetTargetRoleSeeColor(
        SingleRoleBase targetRole, byte targetPlayerId)
    {
        if (targetPlayerId == this.supportTargetId)
        {
            return this.supportColor;
        }

        return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
    }

    public override string GetIntroDescription()
    {
        return string.Format(
            base.GetIntroDescription(),
            Design.ColoredString(
                Palette.White,
                supportPlayerName),
            supportRoleName);
    }

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
	{
	}

    protected override void RoleSpecificInit()
    {
        this.roleNamePrefix = this.CreateImpCrewPrefix();
        this.supportTargetId = byte.MaxValue;
    }
}
