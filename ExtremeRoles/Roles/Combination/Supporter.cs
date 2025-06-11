using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Module.CustomOption.Factory;

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
        string.Concat(this.roleNamePrefix, this.RawRoleName);

    private byte supportTargetId;
    private string supportPlayerName = "";
    private string supportRoleName = "";
    private Color supportColor;

    private string roleNamePrefix = "";

    public Supporter(
        ) : base(
            ExtremeRoleId.Supporter,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Supporter.ToString(),
            ColorPalette.SupporterGreen,
            false, true, false, false,
            tab: OptionTab.CombinationTab)
    {}

    public void IntroBeginSetUp()
    {
        List<byte> target = new List<byte>();

        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            if (playerControl == null || playerControl.Data == null ||
                !ExtremeRoleManager.TryGetRole(playerControl.PlayerId, out var role))
            {
                continue;
            }

            if (role.Id == this.Id)
            {
                continue;
            }

            if (((role.Id == ExtremeRoleId.Marlin) && this.IsCrewmate()) ||
                ((role.Id == ExtremeRoleId.Assassin) && this.IsImpostor()))
            {
                target.Add(playerControl.PlayerId);
            }
        }

        if (target.Count == 0)
        {
            foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
            {
                if (playerControl == null || playerControl.Data == null ||
                    !ExtremeRoleManager.TryGetRole(playerControl.PlayerId, out var role))
                {
                    continue;
                }

                if (role.Id == this.Id)
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
        PlayerControl? targetPlayerControl = Player.GetPlayerControlById(this.supportTargetId);

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
                $"{this.Id}ImposterFullDescription");
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
            return Design.ColoedString(
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
            Design.ColoedString(
                Palette.White,
                supportPlayerName),
            supportRoleName);
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
		var imposterSetting = factory.Get((int)CombinationRoleCommonOption.IsAssignImposter);
		CreateKillerOption(factory, imposterSetting);
	}

    protected override void RoleSpecificInit()
    {
        this.roleNamePrefix = this.CreateImpCrewPrefix();
        this.supportTargetId = byte.MaxValue;
    }
}
