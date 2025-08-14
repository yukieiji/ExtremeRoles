using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.IronMate;

public class IronMateStatusModel(int blockNum) : IStatusModel
{
	public int BlockNum { get; } = blockNum;
}
