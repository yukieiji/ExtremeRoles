using System;

using UnityEngine;

using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.RoleAssign;

#nullable enable

namespace ExtremeRoles.Roles.API;

public abstract class MultiAssignRoleBase : SingleRoleBase
{
    public SingleRoleBase? AnotherRole = null;
    public bool CanHasAnotherRole = false;

	public MultiAssignRoleBase(
        RoleCore core,
        bool canKill,
        bool hasTask,
        bool useVent,
        bool useSabotage,
        bool canCallMeeting = true,
        bool canRepairSabotage = true,
        bool canUseAdmin = true,
        bool canUseSecurity = true,
        bool canUseVital = true,
        OptionTab tab = OptionTab.GeneralTab) : base(
			core,
            canKill, hasTask, useVent,
            useSabotage, canCallMeeting,
            canRepairSabotage, canUseAdmin,
            canUseSecurity, canUseVital, tab)
    { }

    public void SetRoleType(RoleTypes roleType)
    {
		if (VanillaRoleProvider.IsCrewmateRole(roleType))
		{
			this.CanKill = false;
			this.UseVent = false;
			this.UseSabotage = false;
			this.HasTask = true;
		}
		else if (VanillaRoleProvider.IsImpostorRole(roleType))
		{
			this.Core.Team = ExtremeRoleType.Impostor;
			this.Core.Color = Palette.ImpostorRed;
			this.CanKill = true;
			this.UseVent = true;
			this.UseSabotage = true;
			this.HasTask = false;
		}
    }

	public void SetAnotherRole(SingleRoleBase role)
    {

        if (this.CanHasAnotherRole)
        {
            this.AnotherRole = role;
            OverrideAnotherRoleSetting();
        }
    }

    public override string GetRolePlayerNameTag(
        SingleRoleBase targetRole,
        byte targetPlayerId)
    {
        if (this.AnotherRole != null)
        {
            return this.AnotherRole.GetRolePlayerNameTag(
                targetRole, targetPlayerId);
        }

        return string.Empty;
    }


    public override string GetImportantText(bool isContainFakeTask = true)
    {

        if (this.AnotherRole == null)
        {
            return base.GetImportantText();
        }

        string baseString = base.GetImportantText(false);
        string anotherRoleString = this.AnotherRole.GetImportantText(false);

        baseString = $"{baseString}\r\n{anotherRoleString}";

        if (isContainFakeTask && (!this.HasTask || !this.AnotherRole.HasTask))
        {
            string fakeTaskString = Design.ColoredString(
                this.Core.Color,
                TranslationController.Instance.GetString(
                    StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()));
            baseString = $"{baseString}\r\n{fakeTaskString}";
        }

        return baseString;
    }

    public override string GetIntroDescription()
    {

        string baseIntro = Tr.GetString(
            $"{this.Core.Id}IntroDescription");

        if (this.AnotherRole == null)
        {
            return baseIntro;
        }

        string concat = Design.ColoredString(
            Palette.White,
            string.Concat(
                "\n ", Tr.GetString("introAnd")));


        return string.Concat(baseIntro, concat, Design.ColoredString(
            this.AnotherRole.GetNameColor(),
            this.AnotherRole.GetIntroDescription()));

    }
    public override string GetColoredRoleName(bool isTruthColor = false)
    {
        if (this.AnotherRole == null)
        {
            return base.GetColoredRoleName(isTruthColor);
        }

        string baseRole = Design.ColoredString(
            this.Core.Color,
            Tr.GetString(this.RoleName));

        string anotherRole = this.AnotherRole.GetColoredRoleName(isTruthColor);

        string concat = Design.ColoredString(
            Palette.White, " + ");

        return string.Concat(
            baseRole, concat, anotherRole);
    }

    public override Color GetTargetRoleSeeColor(
        SingleRoleBase targetRole,
        byte targetPlayerId)
    {

        if (this.CanHasAnotherRole && this.AnotherRole != null)
        {
            Color color = this.AnotherRole.GetTargetRoleSeeColor(
                targetRole, targetPlayerId);

            if (color != Palette.White) { return color; }
        }

        return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
    }

	public virtual void OverrideAnotherRoleSetting()
	{
		return;
	}

    protected string CreateImpCrewPrefix() => this.IsImpostor() ? "Evil" : "Nice";
}
