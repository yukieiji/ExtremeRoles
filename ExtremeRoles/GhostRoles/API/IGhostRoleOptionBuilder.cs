using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory;

namespace ExtremeRoles.GhostRoles.API;

public interface IGhostRoleOptionBuilder
{
	public void Build(OptionFactory factory);
}
