using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.GhostRoles.Impostor.Igniter;

public enum IgniterOption
{
	IsEffectImpostor,
	IsEffectNeutral
}
public sealed class IgniterOptionBuilder : IGhostRoleOptionBuilder
{
	public void Build(AutoParentSetOptionCategoryFactory factory)
	{
		GhostRoleAbilityFactory.CreateCountButtonOption(factory, 3, 10, 15.0f);
		factory.CreateBoolOption(IgniterOption.IsEffectImpostor, false);
		factory.CreateBoolOption(IgniterOption.IsEffectNeutral, false);
	}
}
