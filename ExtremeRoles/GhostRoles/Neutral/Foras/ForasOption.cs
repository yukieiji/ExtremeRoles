using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.GhostRoles.Neutral.Foras;

public enum ForasOption
{
	Range,
	DelayTime,
	MissingTargetRate,
}

public sealed class ForasOptionBuilder : IGhostRoleOptionBuilder
{
	public void Build(AutoParentSetOptionCategoryFactory factory)
	{
		GhostRoleAbilityFactory.CreateCountButtonOption(factory, 3, 10, 25.0f);
		factory.CreateFloatOption(
			ForasOption.Range,
			1.0f, 0.1f, 3.6f, 0.1f);
		factory.CreateFloatOption(
			ForasOption.DelayTime,
			3.0f, 0.0f, 10.0f, 0.5f,
			format: OptionUnit.Second);
		factory.CreateIntOption(
			ForasOption.MissingTargetRate,
			10, 0, 90, 5,
			format: OptionUnit.Percentage);
	}
}
