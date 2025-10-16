using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.IronMate;

public class IronMateStatusModel(int blockNum) : IStatusModel, IDeadBodyReportOverrideStatus
{
	public bool CanReport => false;

	public int BlockNum { get; } = blockNum;
}
