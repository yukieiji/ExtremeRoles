using System;
using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


namespace ExtremeRoles.Roles.Solo;

public sealed class VanillaRoleWrapper : MultiAssignRoleBase
{
    public RoleTypes VanilaRoleId;

    private VanillaRoleWrapper(RoleTypes id, bool isImpostor) : base(
		new RoleCore(
			ExtremeRoleId.VanillaRole,
			isImpostor ? ExtremeRoleType.Impostor : ExtremeRoleType.Crewmate,
			isImpostor ? Palette.ImpostorRed : Palette.White,
			id.ToString()),
        canKill: isImpostor,
        hasTask: !isImpostor,
        useVent: isImpostor,
        useSabotage: isImpostor)
    {
        this.VanilaRoleId = id;

        var curOption = GameOptionsManager.Instance.CurrentGameOptions;
        if (this.CanKill)
        {
            this.KillCoolTime = Player.DefaultKillCoolTime;
            this.KillRange = curOption.GetInt(Int32OptionNames.KillDistance);
        }
        switch (id)
        {
            case RoleTypes.Engineer:
                this.UseVent = true;
                break;
            default:
                break;
        }
        this.CanHasAnotherRole = ExtremeGameModeManager.Instance.RoleSelector.IsVanillaRoleToMultiAssign;
    }

    public VanillaRoleWrapper(
        RoleTypes id) :
        this(id, 
			VanillaRoleProvider.IsImpostorlRole(id))
    { }

    public override void OverrideAnotherRoleSetting()
    {
        if (this.AnotherRole is VanillaRoleWrapper vanillaRole &&
            vanillaRole.VanilaRoleId == this.VanilaRoleId)
        {
            this.AnotherRole = null;
            this.CanHasAnotherRole = false;
        }
        else
        {
            this.CanCallMeeting = this.AnotherRole.CanCallMeeting;
            this.CanUseAdmin    = this.AnotherRole.CanUseAdmin   ;
            this.CanUseSecurity = this.AnotherRole.CanUseSecurity;
            this.CanUseVital    = this.AnotherRole.CanUseVital;
        }
    }

    public override string GetFullDescription()
    {
        return Tr.GetString(
            $"{this.VanilaRoleId}FullDescription");
    }

    public override string GetColoredRoleName(bool isTruthColor = false)
    {
        if (!isTruthColor &&
            (this.AnotherRole is IRoleAwake<RoleTypes> awakeRole && !awakeRole.IsAwake))
        {
            return Design.ColoedString(
                this.Core.Color,
                Tr.GetString(this.RoleName));
        }

        return base.GetColoredRoleName(isTruthColor);
    }

    public override string GetIntroDescription()
    {
        string baseIntro = Design.ColoedString(
            this.IsImpostor() ? Palette.ImpostorRed : Palette.CrewmateBlue,
            PlayerControl.LocalPlayer.Data.Role.Blurb);

        if (this.AnotherRole == null ||
            (this.AnotherRole is IRoleAwake<RoleTypes> awakeRole &&
             !awakeRole.IsAwake))
        {
            return baseIntro;
        }

        string concat = Design.ColoedString(
            Palette.White,
            string.Concat(
                "\n ", Tr.GetString("introAnd")));

        return string.Concat(baseIntro, concat, Design.ColoedString(
            this.AnotherRole.GetNameColor(),
            this.AnotherRole.GetIntroDescription()));

    }

    public override string GetImportantText(bool isContainFakeTask = true)
    {
        if (this.AnotherRole == null ||
            (this.AnotherRole is IRoleAwake<RoleTypes> awakeRole &&
             !awakeRole.IsAwake))
        {
            return getVanilaImportantText();
        }

        string baseString = getVanilaImportantText(false);
        string anotherRoleString = this.AnotherRole.GetImportantText(false);

        baseString = $"{baseString}\r\n{anotherRoleString}";

        if (isContainFakeTask && (!this.HasTask || !this.AnotherRole.HasTask))
        {
            string fakeTaskString = Design.ColoedString(
                this.Core.Color,
                TranslationController.Instance.GetString(
                    StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()));
            baseString = $"{baseString}\r\n{fakeTaskString}";
        }

        return baseString;

    }
    protected override void CommonInit()
    {
        return;
    }
    protected override void RoleSpecificInit()
    {
        return;
    }

    protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
    {
        throw new System.Exception("Don't call this class method!!");
    }

    private string getVanilaImportantText(bool isContainFakeTask = true)
    {
        if (this.IsImpostor())
        {
            var trans = TranslationController.Instance;

            return string.Concat(
			[
                trans.GetString(StringNames.ImpostorTask, Array.Empty<Il2CppSystem.Object>()),
                "\r\n",
                Palette.ImpostorRed.ToTextColor(),
                isContainFakeTask ?
                    trans.GetString(StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()) :
                    string.Empty,
                "</color>"
            ]);
        }

		var color = this.Core.Color;
        return Design.ColoedString(
			color,
            $"{Design.ColoedString(
				color, Tr.GetString(this.RoleName))}: {Tr.GetString("crewImportantText")}");
    }
}
