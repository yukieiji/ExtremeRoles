using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles.API;

using ExtremeRoles.Module.NewOption;
using ExtremeRoles.Module.NewOption.Factory;

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

    protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateBoolOption(
            NeetOption.CanCallMeeting,
            false);
        factory.CreateBoolOption(
            NeetOption.CanRepairSabotage,
            false);

        var neutralOps = factory.CreateBoolOption(
            NeetOption.IsNeutral,
            false);
        factory.CreateBoolOption(
            NeetOption.HasTask,
            false, neutralOps,
            invert: true);
    }

    protected override void RoleSpecificInit()
    {
        var allOption = OptionManager.Instance;

        this.CanCallMeeting = allOption.GetValue<bool>(
            NeetOption.CanCallMeeting));
        this.CanRepairSabotage = allOption.GetValue<bool>(
            NeetOption.CanRepairSabotage));
        this.HasTask = allOption.GetValue<bool>(
            NeetOption.HasTask));

        if (allOption.GetValue<bool>(
            NeetOption.IsNeutral)))
        {
            this.Team = ExtremeRoleType.Neutral;
        }

    }
}
