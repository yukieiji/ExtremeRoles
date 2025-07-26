using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.Yoko
{
    public class YokoStatusModel : IStatusModel
    {
        public YokoYashiroSystem? yashiro { get; set; }
    }
}
