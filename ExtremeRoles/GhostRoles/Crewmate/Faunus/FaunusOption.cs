using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.GhostRoles.Crewmate.Faunus;

public sealed class FaunusOptionBuilder : IGhostRoleOptionBuilder
{
	public void Build(AutoParentSetOptionCategoryFactory factory)
	{
		GhostRoleAbilityFactory.CreateCountButtonOption(factory, 1, 5, 3.0f);
	}
}
