using ExtremeRoles.Module.NewOption;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.GhostRoles.API.Interface;

public interface ICombination
{
	public MultiAssignRoleBase.OptionOffsetInfo? OffsetInfo { get; set; }
	public OptionLoadWrapper WrappedCategory { get; }
}
