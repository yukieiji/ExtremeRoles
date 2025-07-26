using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.IronMate
{
    public class IronMateStatusModel : IStatusModel
    {
        public IronMateGurdSystem? system { get; set; }
        public int BlockNum { get; }

        public IronMateStatusModel(int blockNum)
        {
            BlockNum = blockNum;
        }
    }
}
