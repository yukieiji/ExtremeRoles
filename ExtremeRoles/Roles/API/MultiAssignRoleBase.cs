using System;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Roles.API;

public abstract class MultiAssignRoleBase : SingleRoleBase
{
    
    public SingleRoleBase AnotherRole = null;
    public bool CanHasAnotherRole = false;
    protected int ManagerOptionOffset = 0;

    public MultiAssignRoleBase(
        ExtremeRoleId id,
        ExtremeRoleType team,
        string roleName,
        Color roleColor,
        bool canKill,
        bool hasTask,
        bool useVent,
        bool useSabotage,
        bool canCallMeeting = true,
        bool canRepairSabotage = true,
        bool canUseAdmin = true,
        bool canUseSecurity = true,
        bool canUseVital = true,
        OptionTab tab = OptionTab.General) : base(
            id, team, roleName, roleColor,
            canKill, hasTask, useVent,
            useSabotage, canCallMeeting,
            canRepairSabotage, canUseAdmin,
            canUseSecurity, canUseVital, tab)
    { }

    public void SetRoleType(RoleTypes roleType)
    {
        switch (roleType)
        {
            case RoleTypes.Shapeshifter:
            case RoleTypes.Impostor:
                this.Team = ExtremeRoleType.Impostor;
                this.NameColor = Palette.ImpostorRed;
                this.CanKill = true;
                this.UseVent = true;
                this.UseSabotage = true;
                this.HasTask = false;
                break;
            case RoleTypes.Crewmate:
            case RoleTypes.Engineer:
            case RoleTypes.Scientist:
                this.CanKill = false;
                this.UseVent = false;
                this.UseSabotage = false;
                this.HasTask = true;
                break;
            default:
                break;
        };
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
            string fakeTaskString = Design.ColoedString(
                this.NameColor,
                FastDestroyableSingleton<TranslationController>.Instance.GetString(
                    StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()));
            baseString = $"{baseString}\r\n{fakeTaskString}";
        }

        return baseString;
    }

    public override string GetIntroDescription()
    {

        string baseIntro = Translation.GetString(
            $"{this.Id}IntroDescription");

        if (this.AnotherRole == null)
        {
            return baseIntro;
        }

        string concat = Design.ColoedString(
            Palette.White,
            string.Concat(
                "\n ", Translation.GetString("introAnd")));


        return string.Concat(baseIntro, concat, Design.ColoedString(
            this.AnotherRole.GetNameColor(),
            this.AnotherRole.GetIntroDescription()));

    }
    public override string GetColoredRoleName(bool isTruthColor = false)
    {
        if (this.AnotherRole == null)
        {
            return base.GetColoredRoleName(isTruthColor);
        }

        string baseRole = Design.ColoedString(
            this.NameColor,
            Translation.GetString(this.RoleName));

        string anotherRole = this.AnotherRole.GetColoredRoleName(isTruthColor);

        string concat = Design.ColoedString(
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
    public int GetManagerOptionId<T>(T option) where T : struct, IConvertible
    {
        EnumCheck(option);

        return GetManagerOptionId(Convert.ToInt32(option));
    }

    public int GetManagerOptionId(int option) => this.ManagerOptionOffset + option;

    public void SetManagerOptionOffset(int offset)
    {
        this.ManagerOptionOffset = offset;
    }

    public int GetManagerOptionOffset() => this.ManagerOptionOffset;

    protected string CreateImpCrewPrefix() => this.IsImpostor() ? "Evil" : "Nice";
}
