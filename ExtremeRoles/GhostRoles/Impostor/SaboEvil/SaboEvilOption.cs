using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.GhostRoles.Impostor.SaboEvil;

public sealed class SaboEvilOptionBuilder : IGhostRoleOptionBuilder
{
	public void Build(AutoParentSetOptionCategoryFactory factory)
	{
		GhostRoleAbilityFactory.CreateCountButtonOption(factory, 3, 20);
	}
}
