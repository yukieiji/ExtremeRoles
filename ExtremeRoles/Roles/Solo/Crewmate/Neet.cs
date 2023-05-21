using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Neet : SingleRoleBase
{
    public enum NeetOption
    {
        CanCallMeeting,
        CanRepairSabotage,
        HasTask,
        IsNeutral
    }

    public Neet() : base(
        ExtremeRoleId.Neet,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Neet.ToString(),
        ColorPalette.NeetSilver,
        false, false, false,
        false, false, false,
        false, false, false)
    { }

    public override string GetFullDescription()
    {
        if (this.IsNeutral())
        {
            return Translation.GetString(
                $"{this.Id}NeutralFullDescription");
        }

        return base.GetFullDescription();
    }

    protected override void CreateSpecificOption(IOptionInfo parentOps)
    {
        CreateBoolOption(
            NeetOption.CanCallMeeting,
            false, parentOps);
        CreateBoolOption(
            NeetOption.CanRepairSabotage,
            false, parentOps);
        
        var neutralOps = CreateBoolOption(
            NeetOption.IsNeutral,
            false, parentOps);
        CreateBoolOption(
            NeetOption.HasTask,
            false, neutralOps,
            invert: true,
            enableCheckOption: parentOps);
    }

    protected override void RoleSpecificInit()
    {
        var allOption = OptionManager.Instance;

        this.CanCallMeeting = allOption.GetValue<bool>(
            GetRoleOptionId(NeetOption.CanCallMeeting));
        this.CanRepairSabotage = allOption.GetValue<bool>(
            GetRoleOptionId(NeetOption.CanRepairSabotage));
        this.HasTask = allOption.GetValue<bool>(
            GetRoleOptionId(NeetOption.HasTask));

        if (allOption.GetValue<bool>(
            GetRoleOptionId(NeetOption.IsNeutral)))
        {
            this.Team = ExtremeRoleType.Neutral;
        }

    }
}
