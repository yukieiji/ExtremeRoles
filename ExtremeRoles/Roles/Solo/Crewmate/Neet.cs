using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
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
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Neet,
			ColorPalette.NeetSilver),
        false, false, false,
        false, false, false,
        false, false, false)
    { }

    public override string GetFullDescription()
    {
        if (this.IsNeutral())
        {
            return Tr.GetString(
                $"{this.Core.Id}NeutralFullDescription");
        }

        return base.GetFullDescription();
    }

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
	{
		var factory = categoryScope.Builder;
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
        var loader = this.Loader;

        this.CanCallMeeting = loader.GetValue<NeetOption, bool>(
            NeetOption.CanCallMeeting);
        this.CanRepairSabotage = loader.GetValue<NeetOption, bool>(
            NeetOption.CanRepairSabotage);
        this.HasTask = loader.GetValue<NeetOption, bool>(
            NeetOption.HasTask);

        if (loader.GetValue<NeetOption, bool>(NeetOption.IsNeutral))
        {
            this.Core.Team = ExtremeRoleType.Neutral;
        }

    }
}
