using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.Yoko;

#nullable enable

public class YokoStatusModel(YokoYashiroSystem? system) : IStatusModel
{
	public YokoYashiroSystem? yashiro { get; } = system;
}
