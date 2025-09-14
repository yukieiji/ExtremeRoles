using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.GhostRoles.Impostor.Ventgeist;

public enum Option
{
	Range,
}

public sealed class VentgeistOptionBuilder : IGhostRoleOptionBuilder
{
	public void Build(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateFloatOption(
			Option.Range, 1.0f,
			0.2f, 3.0f, 0.1f);
		GhostRoleAbilityFactory.CreateCountButtonOption(factory, 2, 10);
	}
}
