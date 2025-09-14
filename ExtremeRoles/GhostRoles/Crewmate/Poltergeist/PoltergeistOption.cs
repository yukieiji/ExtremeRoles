using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.GhostRoles.Crewmate.Poltergeist;

public enum Option
{
	Range,
}
public sealed class PoltergeistOptionBuilder : IGhostRoleOptionBuilder
{
	public void Build(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateFloatOption(
			Option.Range, 1.0f,
			0.2f, 3.0f, 0.1f);
		GhostRoleAbilityFactory.CreateCountButtonOption(factory, 1, 5, 3.0f);
	}
}
