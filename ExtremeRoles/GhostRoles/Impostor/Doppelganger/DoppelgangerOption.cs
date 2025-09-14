using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.GhostRoles.Impostor.Doppelganger;

public sealed class DoppelgangerOptionBuilder : IGhostRoleOptionBuilder
{
	public void Build(AutoParentSetOptionCategoryFactory factory)
	{
		GhostRoleAbilityFactory.CreateCountButtonOption(factory, 2, 10, 5.0f);
	}
}
