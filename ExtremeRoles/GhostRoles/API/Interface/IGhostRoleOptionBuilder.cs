using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory;

namespace ExtremeRoles.GhostRoles.API.Interface;

public interface IGhostRoleOptionBuilder
{
	public void Build(OptionFactory factory);
}
