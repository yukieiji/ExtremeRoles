using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.IronMate;

public sealed class IronMateStatusModel(int blockNum) : IStatusModel, IDeadBodyReportOverrideStatus
{
	public bool CanReport => false;

	public int BlockNum { get; } = blockNum;
}
