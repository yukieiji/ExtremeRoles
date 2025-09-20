using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.GhostRoles.Crewmate.Shutter;

public enum ShutterOption
{
	PhotoRange,
	RightPlayerNameRate
}
public sealed class ShutterOptionBuilder : IGhostRoleOptionBuilder
{
	public void Build(AutoParentSetOptionCategoryFactory factory)
	{
		GhostRoleAbilityFactory.CreateCountButtonOption(factory, 3, 10);
		factory.CreateFloatOption(
			ShutterOption.PhotoRange,
			7.5f, 0.5f, 25f, 0.5f);

		factory.CreateIntOption(
			ShutterOption.RightPlayerNameRate,
			50, 25, 100, 5,
			format: OptionUnit.Percentage);
	}
}
